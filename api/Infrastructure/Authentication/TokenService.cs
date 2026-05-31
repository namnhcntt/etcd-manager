using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using Microsoft.IdentityModel.Tokens;

namespace EtcdManager.API.Infrastructure.Authentication;

public class TokenService(IConfiguration _configuration) : ITokenService
{
    private const int EXPIRES_IN = 90000;

    public Task<JwtTokenData> GenerateJwtTokenData(int userId, string userName)
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
            Expires = DateTime.UtcNow.AddSeconds(EXPIRES_IN),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        // refresh token is jwt token with 30 days expiry
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
                }
            ),
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var refreshToken = tokenHandler.CreateToken(refreshTokenDescriptor);

        return Task.FromResult(
            new JwtTokenData
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = tokenHandler.WriteToken(refreshToken),
                ExpiresIn = EXPIRES_IN,
            }
        );
    }

    public Task<JwtTokenData> RefreshToken(string refreshToken)
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

        return GenerateJwtTokenData(userId, userName);
    }
}
