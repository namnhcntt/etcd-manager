namespace EtcdManager.API.Domain.Services
{
    public interface ITokenService
    {
        Task<JwtTokenData> GenerateJwtTokenData(string userName);
        Task<JwtTokenData> RefreshToken(string refreshToken);
    }
}
