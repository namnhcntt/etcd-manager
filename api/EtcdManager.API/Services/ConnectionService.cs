using dotnet_etcd;
using EtcdManager.API.utils;

namespace EtcdManager.API.Services
{
    public interface IConnectionService
    {
        Task<string> TestConnection(string server, int port, string userName, string password, bool insecure = false);
    }

    public class ConnectionService : IConnectionService
    {
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(
            ILogger<ConnectionService> logger
        )
        {
            this._logger = logger;
        }

        public async Task<string> TestConnection(string server, int port, string userName, string password, bool insecure = false)
        {
            if (server.StartsWith("http://"))
            {
                server = server.Replace("http://", "https://");
            }
            else if (!server.StartsWith("https://"))
            {
                if (insecure)
                    server = "http://" + server;
                else
                    server = "https://" + server;
            }

            try
            {
                var client = new EtcdClient($"{server}:{port}");
                var authRes = await client.AuthenticateAsync(new Etcdserverpb.AuthenticateRequest()
                {
                    Name = userName,
                    Password = password
                });
                return !string.IsNullOrWhiteSpace(authRes.Token) ? "ok" : "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return CommonUtils.GetEtcdResponseErrorMessage(ex.Message);
            }
        }
    }
}