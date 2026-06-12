using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EtcdManager.API.Infrastructure.Authentication;

public class TokenService(IConfiguration _configuration, EtcdManagerDataContext _dataContext)
    : ITokenService
{
    private const int ExpiresInSeconds = 90000;
    private const int RefreshTokenExpiresInDays = 30;

    public async Task<JwtTokenData> GenerateJwtTokenData(int userId, string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key (Jwt:Key) is not configured.");
        var key = Encoding.UTF8.GetBytes(jwtKey);
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? string.Empty;
        var jwtAudience = _configuration["Jwt:Audience"] ?? string.Empty;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
                    new Claim(JwtRegisteredClaimNames.Aud, jwtAudience),
                    new Claim("token_type", "access"),
                }
            ),
            Expires = DateTime.UtcNow.AddSeconds(ExpiresInSeconds),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        // refresh token is jwt token with limited expiry
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiresInDays);
        var refreshTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
                    new Claim(JwtRegisteredClaimNames.Aud, jwtAudience),
                    new Claim("token_type", "refresh"),
                    // unique id so every issued refresh token (and its hash) is distinct
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }
            ),
            Expires = refreshTokenExpiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var refreshToken = tokenHandler.WriteToken(
            tokenHandler.CreateToken(refreshTokenDescriptor)
        );

        // persist hash of the issued refresh token for rotation / reuse detection
        _dataContext.RefreshTokens.Add(
            new RefreshToken
            {
                TokenHash = ComputeTokenHash(refreshToken),
                UserId = userId,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
            }
        );
        await _dataContext.SaveChangesAsync();

        return new JwtTokenData
        {
            Token = tokenHandler.WriteToken(token),
            RefreshToken = refreshToken,
            ExpiresIn = ExpiresInSeconds,
        };
    }

    public async Task<JwtTokenData> RefreshToken(string refreshToken)
    {
        // decode refresh token jwt
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey2 = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key (Jwt:Key) is not configured.");
        var key = Encoding.UTF8.GetBytes(jwtKey2);
        var principal = tokenHandler.ValidateToken(
            refreshToken,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? string.Empty,
                ValidAudience = _configuration["Jwt:Audience"] ?? string.Empty,
            },
            out var securityToken
        );

        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (
            jwtSecurityToken == null
            || !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase
            )
        )
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // validate token_type == "refresh"
        var tokenTypeClaim = principal.Claims.FirstOrDefault(x => x.Type == "token_type");
        if (tokenTypeClaim == null || tokenTypeClaim.Value != "refresh")
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        var userName = principal.Identity?.Name;
        var userId = int.Parse(
            principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value
        );
        if (userName == null)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // rotation / reuse detection: atomically consume the token so two concurrent
        // refreshes with the same token cannot both succeed (TOCTOU)
        var tokenHash = ComputeTokenHash(refreshToken);
        var now = DateTime.UtcNow;
        var consumed = await TryConsumeToken(tokenHash, now);

        if (!consumed)
        {
            var existing = await _dataContext
                .RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
            if (existing?.ConsumedAt != null)
            {
                // reuse of an already-consumed token: assume theft, revoke all tokens for the user
                await RevokeAllUserTokens(existing.UserId, now);
            }
            throw new SecurityTokenException("Invalid refresh token");
        }

        // GenerateJwtTokenData persists the rotated refresh token and saves changes
        return await GenerateJwtTokenData(userId, userName);
    }

    private async Task<bool> TryConsumeToken(string tokenHash, DateTime now)
    {
        if (_dataContext.Database.IsRelational())
        {
            // single conditional UPDATE: only one concurrent caller can win the row
            var affected = await _dataContext
                .RefreshTokens.Where(x =>
                    x.TokenHash == tokenHash && x.ConsumedAt == null && x.ExpiresAt > now
                )
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now));
            return affected > 0;
        }

        // EF InMemory provider (tests) does not support ExecuteUpdateAsync
        var storedToken = await _dataContext.RefreshTokens.FirstOrDefaultAsync(x =>
            x.TokenHash == tokenHash && x.ConsumedAt == null && x.ExpiresAt > now
        );
        if (storedToken == null)
        {
            return false;
        }
        storedToken.ConsumedAt = now;
        await _dataContext.SaveChangesAsync();
        return true;
    }

    private async Task RevokeAllUserTokens(int userId, DateTime now)
    {
        if (_dataContext.Database.IsRelational())
        {
            await _dataContext
                .RefreshTokens.Where(x => x.UserId == userId && x.ConsumedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.ConsumedAt, now));
            return;
        }

        var userTokens = await _dataContext
            .RefreshTokens.Where(x => x.UserId == userId && x.ConsumedAt == null)
            .ToListAsync();
        foreach (var userToken in userTokens)
        {
            userToken.ConsumedAt = now;
        }
        await _dataContext.SaveChangesAsync();
    }

    private static string ComputeTokenHash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
