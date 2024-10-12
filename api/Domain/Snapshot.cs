namespace EtcdManager.API.Domain;

public class Snapshot
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int ConnectionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
