using static Mvccpb.Event.Types;

namespace EtcdManager.API.ApplicationService.Queries.KeyValues;

public class KeyValueResult
{
    public string Key { get; set; } = null!;
    public long CreateRevision { get; set; }
    public long ModRevision { get; set; }
    public long Version { get; set; }
    public string Value { get; set; } = null!;
    public EventType? EventType { get; set; }
    public KeyValueResult? Prev { get; set; }
}

public class ListKeyValueResult
{
    public List<KeyValueResult> KeyValues { get; set; } = new List<KeyValueResult>();

    public int TotalCount
    {
        get { return KeyValues.Count; }
    }
}
