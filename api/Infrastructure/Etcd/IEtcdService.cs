
using EtcdManager.API.Domain;

namespace EtcdManager.API.Infrastructure.Etcd
{
    public interface IEtcdService
    {
        Task Delete(string key, bool deleteRecursive, EtcdConnection etcdConnection);
        Task<List<KeyVersion>> GetAll(EtcdConnection etcdConnection);
        Task<List<string>> GetAllKeys(EtcdConnection etcdConnection);
        Task<KeyVersion> GetByKey(string key, EtcdConnection etcdConnection);
        Task<List<KeyVersion>> GetByKeyPrefix(string keyPrefix, EtcdConnection etcdConnection);
        Task<KeyVersion?> GetRevision(string key, long revision, EtcdConnection etcdConnection);
        Task<List<KeyVersion>> GetRevisions(string key, EtcdConnection etcdConnection);
        Task ImportNodes(KeyValue[] keyModels, EtcdConnection etcdConnection);
        Task RenameKey(string oldKey, string newKey, EtcdConnection etcdConnection);
        Task Save(KeyValue saveKeyValue, EtcdConnection etcdConnection);
        Task<bool> TestConnection(string host, string port, string username, string password);
    }
}
