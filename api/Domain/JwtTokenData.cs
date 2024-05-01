namespace EtcdManager.API.Domain
{
    public class JwtTokenData
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int ExpiresIn { get; set; } = 900;
    }
}
