using static Mvccpb.Event.Types;

namespace EtcdManager.API.Models
{
    public class KeyVersionModel
    {
        public string Key { get; set; }
        public long CreateRevision { get; set; }
        public long ModRevision { get; set; }
        public long Version { get; set; }
        public string Value { get; set; }
        public EventType? EventType { get; set; }
        public KeyVersionModel? Prev { get; set; }
    }
}