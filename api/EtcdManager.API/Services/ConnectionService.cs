using dotnet_etcd;

namespace EtcdManager.API.Services
{
    public interface IConnectionService
    {
        bool TestConnection(string server, string userName, string password);
    }

    public class ConnectionService : IConnectionService
    {
        public ConnectionService(

        )
        {
        }

        public bool TestConnection(string server, string userName, string password)
        {
            var client = new EtcdClient(server, username: userName, password: password);
            return client != null;
        }
    }
}