using System.Net;
using dotnet_etcd;
using EtcdManager.API.Models;
using EtcdManager.API.utils;

namespace EtcdManager.API.Services
{
    public interface IConnectionService
    {
        Task<ResponseModel<string>> TestConnection(ConnectionModel connectionModel);
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

        public async Task<ResponseModel<string>> TestConnection(ConnectionModel connectionModel)
        {
            var arrServerStr = connectionModel.Server.Split(':');
            var host = arrServerStr[0];
            var port = arrServerStr.Length > 1 ? arrServerStr[1] : "2379";
            if (host.StartsWith("http://"))
            {
                host = host.Replace("http://", "https://");
            }
            else if (!host.StartsWith("https://"))
            {
                if (connectionModel.Insecure)
                    host = "http://" + host;
                else
                    host = "https://" + host;
            }

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