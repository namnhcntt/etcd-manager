using EtcdManager.API.Models;

namespace EtcdManager.API.Services
{
    public interface IKeyValueService
    {
        Task<ResponseModel<List<string>>> GetAll(ConnectionModel connection);
        Task<ResponseModel<KeyModel>> Get(ConnectionModel connection, string key);
        Task<ResponseModel<bool>> Save(SaveKeyModel keyModel);
    }

    public class KeyValueService : IKeyValueService
    {
        private readonly IConnectionService _connectionService;

        public KeyValueService(
            IConnectionService connectionService
        )
        {
            this._connectionService = connectionService;
        }

        public async Task<ResponseModel<List<string>>> GetAll(ConnectionModel connection)
        {
            var client = await _connectionService.GetClient(connection);
            var keys = await client.Instance.GetRangeAsync("/", new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            var op = new List<string>();
            foreach (var key in keys.Kvs)
            {
                op.Add(key.Key.ToStringUtf8());
            }
            return ResponseModel<List<string>>.ResponseWithData(op);
        }

        public async Task<ResponseModel<KeyModel>> Get(ConnectionModel connection, string key)
        {
            var client = await _connectionService.GetClient(connection);
            var value = await client.Instance.GetAsync(key, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            return ResponseModel<KeyModel>.ResponseWithData(new KeyModel()
            {
                Key = key,
                Value = value.Kvs[0].Value.ToStringUtf8()
            });
        }

        public async Task<ResponseModel<bool>> Save(SaveKeyModel keyModel)
        {
            var client = await _connectionService.GetClient(keyModel.Connection);
            if (!keyModel.Key.StartsWith("/"))
            {
                keyModel.Key = "/" + keyModel.Key;
            }

            if (keyModel.IsInsert)
            {
                var existKey = await client.Instance.GetAsync(keyModel.Key, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
                if (existKey?.Kvs.Count > 0)
                {
                    return ResponseModel<bool>.ResponseWithError("ERR_DUPLICATED", System.Net.HttpStatusCode.Conflict, "Key already exists");
                }
            }
            await client.Instance.PutAsync(keyModel.Key, keyModel.Value, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
            return ResponseModel<bool>.ResponseWithData(true);
        }
    }
}