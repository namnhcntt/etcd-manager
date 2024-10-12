using static Mvccpb.Event.Types;

namespace EtcdManager.API.Domain;

public class KeyVersion
{
    public string Key { get; set; } = null!;
    public long CreateRevision { get; set; }
    public long ModRevision { get; set; }
    public long Version { get; set; }
    public string Value { get; set; } = null!;
    public EventType? EventType { get; set; }
    public KeyVersion? Prev { get; set; }
}
