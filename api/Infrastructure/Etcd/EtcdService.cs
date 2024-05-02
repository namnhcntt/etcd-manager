using dotnet_etcd;

namespace EtcdManager.API.Infrastructure.Etcd
{
    public class EtcdService: IEtcdService
    {
        public EtcdService()
        {

        }

        public async Task<bool> TestConnection(string host, string port, string username, string password)
        {
            var client = new EtcdClient($"{host}:{port}");
            var token = string.Empty;
            var authRes = await client.AuthenticateAsync(new Etcdserverpb.AuthenticateRequest()
            {
                Name = username,
                Password = password
            });
            token = authRes != null ? authRes.Token : null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                return true;
            }
         
            return false;
        }
    }
}
