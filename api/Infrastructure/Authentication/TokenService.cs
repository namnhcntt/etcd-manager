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
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"]),
                    new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"]),
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
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"]),
                    new Claim(JwtRegisteredClaimNames.Aud, _configuration["Jwt:Audience"]),
                    new Claim("refresh", "true")
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
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var principal = tokenHandler.ValidateToken(
            refreshToken,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
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

        // get claim name refresh type boolean, check if true
        var refreshClaim = principal.Claims.FirstOrDefault(x => x.Type == "refresh");
        if (
            refreshClaim == null
            || !bool.TryParse(refreshClaim.Value, out var isRefresh)
            || !isRefresh
        )
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
