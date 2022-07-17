namespace EtcdManager.API.Models
{
    public class ConnectionModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool EnableAuthenticated { get; set; }
        public bool Insecure { get; set; }
    }
}