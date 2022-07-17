using System.Net;
using dotnet_etcd;
using EtcdManager.API.Models;
using EtcdManager.API.utils;

namespace EtcdManager.API.Services
{
    public interface IConnectionService
    {
        Task<ResponseModel<string>> TestConnection(ConnectionModel connectionModel);
        Task<EtcdClientInstance> GetClient(ConnectionModel connectionModel);
    }

    public class ConnectionService : IConnectionService
    {
        private Dictionary<Guid, EtcdClientInstance> _clients = new Dictionary<Guid, EtcdClientInstance>();
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(
            ILogger<ConnectionService> logger
        )
        {
            this._logger = logger;
        }

        public async Task<EtcdClientInstance> GetClient(ConnectionModel connectionModel)
        {
            if (_clients.ContainsKey(connectionModel.Id))
            {
                return _clients[connectionModel.Id];
            }
            var client = new EtcdClient($"{connectionModel.Host}:{connectionModel.Port}");
            var authRes = await client.AuthenticateAsync(new Etcdserverpb.AuthenticateRequest()
            {
                Name = connectionModel.UserName,
                Password = connectionModel.Password
            });
            var newInstance = new EtcdClientInstance() { Instance = client, Token = authRes.Token };
            _clients.Add(connectionModel.Id, newInstance);
            return newInstance;
        }

        public async Task<ResponseModel<string>> TestConnection(ConnectionModel connectionModel)
        {
            var host = connectionModel.Host;
            var port = connectionModel.Port;

            try
            {
                var client = new EtcdClient($"{host}:{port}");
                var authRes = await client.AuthenticateAsync(new Etcdserverpb.AuthenticateRequest()
                {
                    Name = connectionModel.UserName,
                    Password = connectionModel.Password
                });
                if (!string.IsNullOrWhiteSpace(authRes.Token))
                {
                    return ResponseModel<string>.ResponseWithData("ok");
                }
                else
                {
                    return ResponseModel<string>.ResponseWithError("ERR_AUTHENTICATE", HttpStatusCode.BadRequest, "Authenticate failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseModel<string>.ResponseWithError("ERR_AUTHENTICATE", HttpStatusCode.BadRequest, CommonUtils.GetEtcdResponseErrorMessage(ex.Message));
            }
        }
    }
}