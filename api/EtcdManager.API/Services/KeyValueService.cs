using EtcdManager.API.Models;
using Etcdserverpb;
using Google.Protobuf;
using Polly;

namespace EtcdManager.API.Services
{
    public interface IKeyValueService
    {
        Task<ResponseModel<List<string>>> GetAllKeys(ConnectionModel connection);
        Task<ResponseModel<KeyModel>> Get(ConnectionModel connection, string key);
        Task<ResponseModel<bool>> Save(SaveKeyModel keyModel);
        Task<ResponseModel<bool>> Delete(ConnectionModel connection, string key, bool deleteRecursive = false);
        Task<ResponseModel<bool>> RenameKey(ConnectionModel connection, string oldKey, string newKey);
        Task<ResponseModel<List<KeyVersionModel>>> GetRevisionOfKey(ConnectionModel connection, string key);
        Task<ResponseModel<List<KeyVersionModel>>> GetAll(ConnectionModel connection);
        Task<ResponseModel<bool>> ImportNodes(ConnectionModel connection, KeyModel[] keyModels);
        Task<ResponseModel<List<KeyVersionModel>>> GetByKeyPrefix(ConnectionModel connection, string keyPrefix);
    }

    public class KeyValueService : IKeyValueService
    {
        private readonly IConnectionService _connectionService;
        private Dictionary<string, long> _watchs = new Dictionary<string, long>();
        public KeyValueService(
            IConnectionService connectionService
        )
        {
            this._connectionService = connectionService;
        }

        public async Task<ResponseModel<List<string>>> GetAllKeys(ConnectionModel connection)
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

        public async Task<ResponseModel<bool>> Delete(ConnectionModel connection, string key, bool deleteRecursive = false)
        {
            var client = await _connectionService.GetClient(connection);
            await client.Instance.DeleteAsync(key, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            if (deleteRecursive)
            {
                await client.Instance.DeleteRangeAsync($"{key}/", new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
            }
            return ResponseModel<bool>.ResponseWithData(true);
        }

        public async Task<ResponseModel<bool>> RenameKey(ConnectionModel connection, string oldKey, string newKey)
        {
            try
            {
                var client = await _connectionService.GetClient(connection);
                var oldKeyValue = await client.Instance.GetValAsync(oldKey, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
                await client.Instance.DeleteAsync(oldKey, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
                await client.Instance.PutAsync(newKey, oldKeyValue, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
                return ResponseModel<bool>.ResponseWithData(true);
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.ResponseWithError("ERR_UNKNOWN", System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ResponseModel<List<KeyVersionModel>>> GetRevisionOfKey(ConnectionModel connection, string key)
        {
            var client = await _connectionService.GetClient(connection);
            var value = await client.Instance.GetAsync(key, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });

            var op = await GetAllRevision(client, key, value.Kvs[0].Version);
            return ResponseModel<List<KeyVersionModel>>.ResponseWithData(op);
        }

        private async Task<List<KeyVersionModel>> GetAllRevision(EtcdClientInstance client, string key, long currentVersion)
        {
            var op = new List<KeyVersionModel>();
            bool done = false;
            var cancelTokenSource = new CancellationTokenSource();

            // nếu chưa khởi tạo thì khởi tạo 1 lần
            var _ = client.Instance.Watch(new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    StartRevision = 1
                }
            }, (WatchResponse watchEvent) =>
            {
                if (!watchEvent.Created)
                {
                    foreach (var evt in watchEvent.Events)
                    {
                        var newObj = new KeyVersionModel();
                        newObj.Key = evt.Kv.Key.ToStringUtf8();
                        newObj.CreateRevision = evt.Kv.CreateRevision;
                        newObj.ModRevision = evt.Kv.ModRevision;
                        newObj.Version = evt.Kv.Version;
                        newObj.Value = evt.Kv.Value.ToStringUtf8();
                        newObj.EventType = evt.Type;
                        newObj.Prev =
                            evt.PrevKv == null ?
                            null :
                         new KeyVersionModel()
                         {
                             Key = evt.PrevKv.Key.ToStringUtf8(),
                             CreateRevision = evt.PrevKv.CreateRevision,
                             ModRevision = evt.PrevKv.ModRevision,
                             Version = evt.PrevKv.Version,
                             Value = evt.PrevKv.Value.ToStringUtf8(),
                         };
                        op.Add(newObj);
                    }
                    done = true;
                }
                else
                {
                    _watchs.Add(key, watchEvent.WatchId);
                }
            }, new Grpc.Core.Metadata() { new Grpc.Core.Metadata.Entry("token", client.Token) }, null, cancelTokenSource.Token);

            var policy = Policy.HandleResult<bool>(x => !x).WaitAndRetryAsync(5, retry => TimeSpan.FromSeconds(retry));
            await policy.ExecuteAsync(() =>
            {
                if (done)
                {
                    cancelTokenSource.Cancel();
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            });
            return op;
        }

        public async Task<ResponseModel<List<KeyVersionModel>>> GetAll(ConnectionModel connection)
        {
            var client = await _connectionService.GetClient(connection);
            var keys = await client.Instance.GetRangeAsync("/", new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            var op = new List<KeyVersionModel>();
            foreach (var key in keys.Kvs)
            {
                op.Add(new KeyVersionModel()
                {
                    Key = key.Key.ToStringUtf8(),
                    CreateRevision = key.CreateRevision,
                    ModRevision = key.ModRevision,
                    Version = key.Version,
                    Value = key.Value.ToStringUtf8()
                });
            }
            return ResponseModel<List<KeyVersionModel>>.ResponseWithData(op);
        }

        public async Task<ResponseModel<bool>> ImportNodes(ConnectionModel connection, KeyModel[] keyModels)
        {
            try
            {
                var client = await _connectionService.GetClient(connection);
                foreach (var keyModel in keyModels)
                {
                    await client.Instance.PutAsync(keyModel.Key, keyModel.Value, new Grpc.Core.Metadata() {
                        new Grpc.Core.Metadata.Entry("token",client.Token)
                    });
                }
            }
            catch (Exception ex)
            {
                return ResponseModel<bool>.ResponseWithError("ERR_UNKNOWN", System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
            return ResponseModel<bool>.ResponseWithData(true);
        }

        public async Task<ResponseModel<List<KeyVersionModel>>> GetByKeyPrefix(ConnectionModel connection, string keyPrefix)
        {
            var client = await _connectionService.GetClient(connection);
            var range = client.Instance.GetRange(keyPrefix, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            var op = new List<KeyVersionModel>();
            foreach (var key in range.Kvs)
            {
                op.Add(new KeyVersionModel()
                {
                    Key = key.Key.ToStringUtf8(),
                    Value = key.Value.ToStringUtf8(),
                    CreateRevision = key.CreateRevision,
                    ModRevision = key.ModRevision,
                    Version = key.Version
                });
            }
            return ResponseModel<List<KeyVersionModel>>.ResponseWithData(op);
        }
    }
}