using EtcdManager.API.Models;

namespace EtcdManager.API.Services
{
    public interface IKeyValueService
    {
        Task<ResponseModel<List<string>>> GetAll(ConnectionModel connection);
        Task<ResponseModel<KeyModel>> Get(ConnectionModel connection, string key);
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
    }
}