using dotnet_etcd;

namespace EtcdManager.API.Models
{
    public class EtcdClientInstance
    {
        public EtcdClient Instance { get; set; }
        public string Token { get; set; }
    }
}