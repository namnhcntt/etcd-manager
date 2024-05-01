namespace EtcdManager.API.Domain.Services
{
    public interface ITokenService
    {
        Task<JwtTokenData> GenerateJwtTokenData(int userId, string userName);
        Task<JwtTokenData> RefreshToken(string refreshToken);
    }
}
