namespace EtcdManager.API.Controllers.EtcdConnections.Post;

public class CreateConnectionModel
{
    public string Name { get; set; } = null!;
    public string Server { get; set; } = null!;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PermissionUsers { get; set; }
    public bool EnableAuthenticated { get; set; }
    public bool Insecure { get; set; }
    public string? AgentDomain { get; set; }
}
