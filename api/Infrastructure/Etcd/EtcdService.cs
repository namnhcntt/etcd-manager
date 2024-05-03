using dotnet_etcd;
using EtcdManager.API.Domain;
using EtcdManager.API.Infrastructure.Cache;
using Etcdserverpb;
using Google.Protobuf;
using Polly;
using System.Collections.Concurrent;

namespace EtcdManager.API.Infrastructure.Etcd
{
    public class EtcdService : IEtcdService
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<EtcdService> _logger;
        
        public EtcdService(ICacheService cacheService, ILogger<EtcdService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
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

        public async Task<List<KeyVersion>> GetAll(EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var keys = await client.Instance.GetRangeAsync("/", new Grpc.Core.Metadata() { { "token", client.Token } });
            var op = new List<KeyVersion>();
            foreach (var key in keys.Kvs)
            {
                op.Add(new KeyVersion()
                {
                    Key = key.Key.ToStringUtf8(),
                    Value = key.Value.ToStringUtf8(),
                    Version = key.Version,
                    CreateRevision = key.CreateRevision,
                    ModRevision = key.ModRevision
                });
            }
            return op;
        }

        public async Task<List<string>> GetAllKeys(EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var userDetail = await client.Instance.UserGetAsync(new AuthUserGetRequest { Name = etcdConnection.Username }, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            if (userDetail == null)
            {
                throw new Exception("User not found");
            }
            var roles = userDetail.Roles;
            var hasRoot = roles.Any(x => x == "root");
            if (!hasRoot)
            {
                var lstStr = new ConcurrentBag<string>();
                var ind = 0;
                var count = roles.Count;
                // var result 
                Parallel.ForEach(roles, async role =>
                {
                    try
                    {
                        var roleResult = await client.Instance.RoleGetASync(new AuthRoleGetRequest { Role = role }, new Grpc.Core.Metadata() {
                            new Grpc.Core.Metadata.Entry("token",client.Token)
                        });
                        if (roleResult != null)
                        {
                            var rolePermissions = roleResult.Perm;
                            foreach (var permission in rolePermissions)
                            {
                                var key = permission.Key.ToStringUtf8();
                                if (!lstStr.Contains(key))
                                {
                                    lstStr.Add(key);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                    ind++;
                });

                var policy = Policy.HandleResult<bool>(x => !x).WaitAndRetryAsync(10, retry => TimeSpan.FromSeconds(retry));
                await policy.ExecuteAsync(() =>
                {
                    return Task.FromResult(ind == count);
                });
                var op = new ConcurrentBag<string>();
                var ind2 = 0;
                var count2 = lstStr.Count;
                foreach (var key in lstStr)
                {
                    try
                    {
                        var keyResult = await client.Instance.GetRangeAsync(key, new Grpc.Core.Metadata() {
                            new Grpc.Core.Metadata.Entry("token",client.Token)
                        });
                        if (keyResult != null)
                        {
                            var keys = keyResult.Kvs;
                            foreach (var kv in keys)
                            {
                                var k = kv.Key.ToStringUtf8();
                                if (!op.Contains(k))
                                {
                                    op.Add(k);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                    ind2++;
                };
                var policy2 = Policy.HandleResult<bool>(x => !x).WaitAndRetryAsync(10, retry => TimeSpan.FromSeconds(retry));
                await policy2.ExecuteAsync(() =>
                {
                    return Task.FromResult(ind2 == count2);
                });
                return op.ToList();
            }
            else
            {
                var keys = await client.Instance.GetRangeAsync("/", new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });

                var op = new List<string>();
                foreach (var key in keys.Kvs)
                {
                    op.Add(key.Key.ToStringUtf8());
                }
                return op;
            }
        }
        public async Task<KeyVersion> GetByKey(string key, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var keyResult = await client.Instance.GetAsync(key, new Grpc.Core.Metadata() { { "token", client.Token } });
            if (keyResult != null)
            {
                var keys = keyResult.Kvs;
                foreach (var kv in keys)
                {
                    return new KeyVersion()
                    {
                        Key = kv.Key.ToStringUtf8(),
                        Value = kv.Value.ToStringUtf8(),
                        CreateRevision = kv.CreateRevision,
                        ModRevision = kv.ModRevision,
                        Version = kv.Version
                    };
                }
            }
            throw new Exception($"Cannot get key {key}");
        }

        public async Task Save(KeyValue saveKeyValue, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            if (!saveKeyValue.Key.StartsWith("/"))
            {
                saveKeyValue.Key = "/" + saveKeyValue.Key;
            }

            await client.Instance.PutAsync(saveKeyValue.Key, saveKeyValue.Value, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
        }

        public async Task Delete(string key, bool deleteRecursive, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            await client.Instance.DeleteRangeAsync(key, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });

            if (deleteRecursive)
            {
                await client.Instance.DeleteRangeAsync($"{key}/", new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
            }
        }

        public async Task RenameKey(string oldKey, string newKey, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var oldKeyValue = await client.Instance.GetValAsync(oldKey, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
            await client.Instance.DeleteAsync(oldKey, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
            await client.Instance.PutAsync(newKey, oldKeyValue, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
                });
        }

        public async Task<List<KeyVersion>> GetRevisions(string key, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var keyResult = await client.Instance.GetAsync(key, new Grpc.Core.Metadata() {
                    new Grpc.Core.Metadata.Entry("token",client.Token)
            });

            var op = await GetAllRevisions(client, key, keyResult.Kvs[0].Version);
            return op;
        }

        public async Task<KeyVersion?> GetRevision(string key, long revision, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var keyResult = await client.Instance.GetValAsync(key, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            KeyVersion? op = null;
            bool done = false;
            var cancelTokenSource = new CancellationTokenSource();

            // nếu chưa khởi tạo thì khởi tạo 1 lần
            client.Instance.Watch(new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    StartRevision = revision
                }
            }, (WatchResponse watchEvent) =>
            {
                if (!watchEvent.Created)
                {
                    foreach (var evt in watchEvent.Events)
                    {
                        var newObj = new KeyVersion();
                        newObj.Key = evt.Kv.Key.ToStringUtf8();
                        newObj.CreateRevision = evt.Kv.CreateRevision;
                        newObj.ModRevision = evt.Kv.ModRevision;
                        newObj.Version = evt.Kv.Version;
                        newObj.Value = evt.Kv.Value.ToStringUtf8();
                        newObj.EventType = evt.Type;
                        newObj.Prev =
                            evt.PrevKv == null ?
                            null :
                         new KeyVersion()
                         {
                             Key = evt.PrevKv.Key.ToStringUtf8(),
                             CreateRevision = evt.PrevKv.CreateRevision,
                             ModRevision = evt.PrevKv.ModRevision,
                             Version = evt.PrevKv.Version,
                             Value = evt.PrevKv.Value.ToStringUtf8(),
                         };
                        op = newObj;
                        done = true;
                        break;
                    }
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

        public async Task<List<KeyVersion>> GetByKeyPrefix(string keyPrefix, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            var range = client.Instance.GetRange(keyPrefix, new Grpc.Core.Metadata() {
                new Grpc.Core.Metadata.Entry("token",client.Token)
            });
            var op = new List<KeyVersion>();
            foreach (var key in range.Kvs)
            {
                op.Add(new KeyVersion()
                {
                    Key = key.Key.ToStringUtf8(),
                    Value = key.Value.ToStringUtf8(),
                    CreateRevision = key.CreateRevision,
                    ModRevision = key.ModRevision,
                    Version = key.Version
                });
            }
            return op;
        }

        public async Task ImportNodes(KeyValue[] keyModels, EtcdConnection etcdConnection)
        {
            var client = await GetEtcdToken(etcdConnection);
            foreach (var keyModel in keyModels)
            {
                await client.Instance.PutAsync(keyModel.Key, keyModel.Value, new Grpc.Core.Metadata() {
                        new Grpc.Core.Metadata.Entry("token",client.Token)
                    });
            }
        }

        private async Task<List<KeyVersion>> GetAllRevisions(EtcdClientInstance client, string key, long currentVersion)
        {
            var op = new List<KeyVersion>();
            bool done = false;
            var cancelTokenSource = new CancellationTokenSource();
            // nếu chưa khởi tạo thì khởi tạo 1 lần
            _ = client.Instance.WatchAsync(new WatchRequest()
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
                        var newObj = new KeyVersion();
                        newObj.Key = evt.Kv.Key.ToStringUtf8();
                        newObj.CreateRevision = evt.Kv.CreateRevision;
                        newObj.ModRevision = evt.Kv.ModRevision;
                        newObj.Version = evt.Kv.Version;
                        newObj.Value = evt.Kv.Value.ToStringUtf8();
                        newObj.EventType = evt.Type;
                        newObj.Prev =
                            evt.PrevKv == null ?
                            null :
                         new KeyVersion()
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

        private async Task<EtcdClientInstance> GetEtcdToken(EtcdConnection etcdConnection)
        {
            var cacheKey = $"{etcdConnection.OwnerId}_{etcdConnection.Server}";
            var existToken = await _cacheService.Get<EtcdClientInstance>(cacheKey);
            if (existToken != null)
            {
                // test token, if invalid auth, then re-auth
                try
                {
                    var userDetail = await existToken.Instance.UserGetAsync(new AuthUserGetRequest { Name = etcdConnection.Username }, new Grpc.Core.Metadata() {
                        new Grpc.Core.Metadata.Entry("token",existToken.Token)
                    });
                    if (userDetail == null)
                    {
                        throw new Exception("User not found");
                    }
                }
                catch (Exception ex)
                {
                    await _cacheService.Remove(cacheKey);
                    return await GetEtcdToken(etcdConnection);
                }

                return existToken;
            }

            var client = new EtcdClient($"{etcdConnection.Host}:{etcdConnection.Port}");
            var token = string.Empty;
            var authRes = await client.AuthenticateAsync(new Etcdserverpb.AuthenticateRequest()
            {
                Name = etcdConnection.Username,
                Password = etcdConnection.Password
            });
            token = authRes != null ? authRes.Token : null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var etcdClientInstance = new EtcdClientInstance()
                {
                    Instance = client,
                    Token = token
                };
                await _cacheService.Set(cacheKey, etcdClientInstance);
                return etcdClientInstance;
            }
            throw new Exception($"Cannot get token from etcd server {etcdConnection.Server}");
        }
    }
}
