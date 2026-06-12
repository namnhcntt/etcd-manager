using EtcdManager.API.Core.Helpers;

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
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The account id of the owner of this connection.
    /// </summary>
    public int OwnerId { get; set; }
    public string? AgentDomain { get; set; }
    public string Host => EtcdServerParser.ParseHostAndPort(Server, Insecure).Host;

    public string Port => EtcdServerParser.ParseHostAndPort(Server, Insecure).Port;
}
