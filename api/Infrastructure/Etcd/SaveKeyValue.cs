using EtcdManager.API.Domain;

namespace EtcdManager.API.Infrastructure.Etcd
{
    public class SaveKeyValue : KeyValue
    {
        public bool? IsInsert { get; set; }
    }
}
