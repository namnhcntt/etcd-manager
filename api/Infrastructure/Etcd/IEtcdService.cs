
namespace EtcdManager.API.Infrastructure.Etcd
{
    public interface IEtcdService
    {
        Task<bool> TestConnection(string host, string port, string username, string password);
    }
}
