namespace EtcdManager.API.Domain;

/// <summary>
/// Represents a connection to an etcd server.
/// </summary>
public class EtcdConnection
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PermissionUsers { get; set; }
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The account id of the owner of this connection.
    /// </summary>
    public int OwnerId { get; set; }
    public string? AgentDomain { get; set; }
    public string Host
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Server))
                return string.Empty;

            var host = Server.Split(':')[0];

            if (!host.StartsWith("https://"))
            {
                if (host.StartsWith("http://"))
                {
                    host = host.Replace("http://", "https://");
                }
                else if (Insecure)
                {
                    host = "http://" + host;
                }
                else
                {
                    host = "https://" + host;
                }
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
