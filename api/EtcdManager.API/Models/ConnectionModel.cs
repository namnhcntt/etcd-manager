namespace EtcdManager.API.Models
{
    public class ConnectionModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? PermissionUsers { get; set; }
        public bool EnableAuthenticated { get; set; }
        public bool Insecure { get; set; }
        public string Host
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server))
                    return string.Empty;
                var host = Server.Split(':')[0];

                if (host.StartsWith("http://"))
                {
                    host = host.Replace("http://", "https://");
                }
                else if (!host.StartsWith("https://"))
                {
                    if (Insecure)
                        host = "http://" + host;
                    else
                        host = "https://" + host;
                }
                return host;
            }
        }
        public string Port
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Server))
                    return string.Empty;
                var arr = Server.Split(':');
                return arr.Length > 1 ? arr[1] : "2379";
            }
        }
    }
}