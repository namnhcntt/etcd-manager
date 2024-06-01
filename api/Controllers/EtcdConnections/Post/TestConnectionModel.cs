namespace EtcdManager.API.Controllers.EtcdConnections.Post
{
    public class TestConnectionModel
    {
        public string Server { get; set; } = null!;
        public bool EnableAuthenticated { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool Insecure { get; set; }
    }
}
