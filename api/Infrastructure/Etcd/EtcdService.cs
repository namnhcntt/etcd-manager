using System.Collections.Concurrent;
using dotnet_etcd;
using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Cache;
using Etcdserverpb;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace EtcdManager.API.Infrastructure.Etcd;

public class EtcdService(
    ICacheService _cacheService,
    ILogger<EtcdService> _logger,
    IPasswordProtectorService _passwordProtector
) : IEtcdService
{
    // etcd auth tokens default to a 5-minute TTL; expire cached clients before that.
    private static readonly TimeSpan ClientCacheTtl = TimeSpan.FromMinutes(4);

    // Must be shorter than ClientCacheTtl, otherwise entries expire before
    // revalidation ever triggers and the revalidation path is dead code.
    private static readonly TimeSpan ClientValidationInterval = TimeSpan.FromMinutes(1);

    // How long to wait for a watch event before giving up.
    private static readonly TimeSpan WatchTimeout = TimeSpan.FromSeconds(15);

    public async Task<bool> TestConnection(
        string host,
        string port,
        bool enableAuthenticated,
        bool insecure,
        string? username,
        string? password
    )
    {
        using var client = new EtcdClient(
            $"{host}:{port}",
            configureChannelOptions: channelOptions =>
            {
                if (insecure)
                {
                    channelOptions.Credentials = ChannelCredentials.Insecure;
                }
            }
        );
        if (enableAuthenticated)
        {
            var token = string.Empty;
            var authRes = await client.AuthenticateAsync(
                new Etcdserverpb.AuthenticateRequest() { Name = username, Password = password }
            );
            token = authRes != null ? authRes.Token : null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                return true;
            }
        }
        else
        {
            // test the client is online with authenticate disabled
            var response = await client.GetAsync("/");
            if (response != null)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<List<KeyVersion>> GetAll(EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        bool hasRoot = false;
        RepeatedField<string> roles = new RepeatedField<string>();
        if (etcdConnection.EnableAuthenticated && !string.IsNullOrWhiteSpace(etcdConnection.Username))
        {
            var userDetail = await client.Instance.UserGetAsync(
                new AuthUserGetRequest { Name = etcdConnection.Username },
                AddDefaultHeader(client)
            );
            if (userDetail == null)
            {
                throw new Exception("User not found");
            }
            roles = userDetail.Roles;
            hasRoot = roles.Any(x => x == "root");
        }
        if (etcdConnection.EnableAuthenticated && !hasRoot)
        {
            var op = new ConcurrentBag<KeyVersion>();
            // var result
            await Parallel.ForEachAsync(
                roles,
                async (string role, CancellationToken cancellation) =>
                {
                    try
                    {
                        var roleResult = await client.Instance.RoleGetAsync(
                            new AuthRoleGetRequest { Role = role },
                            AddDefaultHeader(client)
                        );
                        if (roleResult != null)
                        {
                            var rolePermissions = roleResult.Perm;
                            foreach (var permission in rolePermissions)
                            {
                                var keyResult = await client.Instance.GetRangeAsync(
                                    permission.Key.ToStringUtf8(),
                                    AddDefaultHeader(client)
                                );

                                if (keyResult != null)
                                {
                                    foreach (var key in keyResult.Kvs)
                                    {
                                        if (!op.Any(x => x.Key == key.Key.ToStringUtf8()))
                                        {
                                            op.Add(
                                                new KeyVersion()
                                                {
                                                    Key = key.Key.ToStringUtf8(),
                                                    Value = key.Value.ToStringUtf8(),
                                                    CreateRevision = key.CreateRevision,
                                                    ModRevision = key.ModRevision,
                                                    Version = key.Version
                                                }
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            );
            return op.OrderBy(x => x.Key).ToList();
        }
        else
        {
            var keys = await client.Instance.GetRangeAsync("/", AddDefaultHeader(client));

            var op = new List<KeyVersion>();
            foreach (var key in keys.Kvs)
            {
                op.Add(
                    new KeyVersion()
                    {
                        Key = key.Key.ToStringUtf8(),
                        Value = key.Value.ToStringUtf8(),
                        CreateRevision = key.CreateRevision,
                        ModRevision = key.ModRevision,
                        Version = key.Version
                    }
                );
            }
            return op.OrderBy(x => x.Key).ToList();
        }
    }

    public async Task<List<string>> GetAllKeys(EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        bool hasRoot = false;
        RepeatedField<string> roles = new RepeatedField<string>();
        if (etcdConnection.EnableAuthenticated && !string.IsNullOrWhiteSpace(etcdConnection.Username))
        {
            var userDetail = await client.Instance.UserGetAsync(
                new AuthUserGetRequest { Name = etcdConnection.Username },
                AddDefaultHeader(client)
            );
            if (userDetail == null)
            {
                throw new Exception("User not found");
            }
            roles = userDetail.Roles;
            hasRoot = roles.Any(x => x == "root");
        }
        if (etcdConnection.EnableAuthenticated && !hasRoot)
        {
            var op = new ConcurrentBag<string>();
            // var result
            await Parallel.ForEachAsync(
                roles,
                async (string role, CancellationToken cancellation) =>
                {
                    try
                    {
                        var roleResult = await client.Instance.RoleGetAsync(
                            new AuthRoleGetRequest { Role = role },
                            AddDefaultHeader(client)
                        );
                        if (roleResult != null)
                        {
                            var rolePermissions = roleResult.Perm;
                            foreach (var permission in rolePermissions)
                            {
                                var key = permission.Key.ToStringUtf8();
                                if (!op.Contains(key))
                                {
                                    op.Add(key);
                                }

                                var keyResult = await client.Instance.GetRangeAsync(
                                    key,
                                    AddDefaultHeader(client)
                                );
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
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
            );

            return op.OrderBy(x => x).ToList();
        }
        else
        {
            var keys = await client.Instance.GetRangeAsync("/", AddDefaultHeader(client));

            var op = new List<string>();
            foreach (var key in keys.Kvs)
            {
                op.Add(key.Key.ToStringUtf8());
            }
            return op.OrderBy(x => x).ToList();
        }
    }

    public async Task<KeyVersion> GetByKey(string key, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var keyResult = await client.Instance.GetAsync(key, AddDefaultHeader(client));
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
        var client = await GetEtcdClientInstance(etcdConnection);
        if (!saveKeyValue.Key.StartsWith("/"))
        {
            saveKeyValue.Key = "/" + saveKeyValue.Key;
        }

        await client.Instance.PutAsync(
            saveKeyValue.Key,
            saveKeyValue.Value,
            AddDefaultHeader(client)
        );
    }

    public async Task Delete(string key, bool deleteRecursive, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        if (deleteRecursive)
        {
            await client.Instance.DeleteRangeAsync($"{key}/", AddDefaultHeader(client));
        }
        await client.Instance.DeleteAsync(key, AddDefaultHeader(client));
    }

    public async Task RenameKey(string oldKey, string newKey, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var oldKeyResult = await client.Instance.GetAsync(oldKey, AddDefaultHeader(client));
        if (oldKeyResult == null || oldKeyResult.Kvs.Count == 0)
        {
            throw new Exception($"Key '{oldKey}' not found in etcd.");
        }

        // Atomically put the new key and delete the old key in a single transaction,
        // guarded by a check that the old key is unchanged since we read it (CAS on
        // ModRevision), so a concurrent write makes the txn fail instead of
        // resurrecting a stale value.
        var txnRequest = new TxnRequest();
        txnRequest.Compare.Add(
            new Compare
            {
                Key = ByteString.CopyFromUtf8(oldKey),
                Target = Compare.Types.CompareTarget.Mod,
                Result = Compare.Types.CompareResult.Equal,
                ModRevision = oldKeyResult.Kvs[0].ModRevision
            }
        );
        txnRequest.Success.Add(
            new RequestOp
            {
                RequestPut = new PutRequest
                {
                    Key = ByteString.CopyFromUtf8(newKey),
                    Value = oldKeyResult.Kvs[0].Value
                }
            }
        );
        txnRequest.Success.Add(
            new RequestOp
            {
                RequestDeleteRange = new DeleteRangeRequest
                {
                    Key = ByteString.CopyFromUtf8(oldKey)
                }
            }
        );
        var txnResponse = await client.Instance.TransactionAsync(
            txnRequest,
            AddDefaultHeader(client)
        );
        if (!txnResponse.Succeeded)
        {
            throw new Exception(
                $"Key '{oldKey}' was modified concurrently or no longer exists in etcd; rename aborted."
            );
        }
    }

    public async Task<List<KeyVersion>> GetRevisions(string key, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var keyResult = await client.Instance.GetAsync(key, AddDefaultHeader(client));
        if (keyResult == null || keyResult.Kvs.Count == 0)
        {
            throw new Exception($"Key '{key}' not found in etcd.");
        }

        var op = await GetAllRevisions(client, key, keyResult.Kvs[0].Version);
        return op;
    }

    public async Task<KeyVersion?> GetRevision(
        string key,
        long revision,
        EtcdConnection etcdConnection
    )
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var keyResult = await client.Instance.GetValAsync(key, AddDefaultHeader(client));
        KeyVersion? op = null;
        var done = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var cancelTokenSource = new CancellationTokenSource();

        // nếu chưa khởi tạo thì khởi tạo 1 lần
        _ = client.Instance.WatchAsync(
            new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    StartRevision = revision
                }
            },
            (WatchResponse watchEvent) =>
            {
                try
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
                                evt.PrevKv == null
                                    ? null
                                    : new KeyVersion()
                                    {
                                        Key = evt.PrevKv.Key.ToStringUtf8(),
                                        CreateRevision = evt.PrevKv.CreateRevision,
                                        ModRevision = evt.PrevKv.ModRevision,
                                        Version = evt.PrevKv.Version,
                                        Value = evt.PrevKv.Value.ToStringUtf8(),
                                    };
                            op = newObj;
                            done.TrySetResult(true);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    done.TrySetException(ex);
                }
            },
            AddDefaultHeader(client),
            null,
            cancelTokenSource.Token
        );

        try
        {
            await done.Task.WaitAsync(WatchTimeout);
        }
        catch (TimeoutException)
        {
            // no watch event arrived in time; return whatever was collected (null)
        }
        finally
        {
            cancelTokenSource.Cancel();
        }

        return op;
    }

    public async Task<List<KeyVersion>> GetByKeyPrefix(
        string keyPrefix,
        EtcdConnection etcdConnection
    )
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var range = await client.Instance.GetRangeAsync(keyPrefix, AddDefaultHeader(client));
        var op = new List<KeyVersion>();
        foreach (var key in range.Kvs)
        {
            op.Add(
                new KeyVersion()
                {
                    Key = key.Key.ToStringUtf8(),
                    Value = key.Value.ToStringUtf8(),
                    CreateRevision = key.CreateRevision,
                    ModRevision = key.ModRevision,
                    Version = key.Version
                }
            );
        }
        return op;
    }

    public async Task ImportNodes(KeyValue[] keyModels, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        foreach (var keyModel in keyModels)
        {
            await client.Instance.PutAsync(keyModel.Key, keyModel.Value, AddDefaultHeader(client));
        }
    }

    private async Task<List<KeyVersion>> GetAllRevisions(
        EtcdClientInstance client,
        string key,
        long currentVersion
    )
    {
        var op = new List<KeyVersion>();
        var done = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var cancelTokenSource = new CancellationTokenSource();
        // nếu chưa khởi tạo thì khởi tạo 1 lần
        _ = client.Instance.WatchAsync(
            new WatchRequest()
            {
                CreateRequest = new WatchCreateRequest()
                {
                    Key = ByteString.CopyFromUtf8(key),
                    StartRevision = 1
                }
            },
            (WatchResponse watchEvent) =>
            {
                try
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
                                evt.PrevKv == null
                                    ? null
                                    : new KeyVersion()
                                    {
                                        Key = evt.PrevKv.Key.ToStringUtf8(),
                                        CreateRevision = evt.PrevKv.CreateRevision,
                                        ModRevision = evt.PrevKv.ModRevision,
                                        Version = evt.PrevKv.Version,
                                        Value = evt.PrevKv.Value.ToStringUtf8(),
                                    };
                            op.Add(newObj);
                        }
                        done.TrySetResult(true);
                    }
                }
                catch (Exception ex)
                {
                    done.TrySetException(ex);
                }
            },
            AddDefaultHeader(client),
            null,
            cancelTokenSource.Token
        );

        try
        {
            await done.Task.WaitAsync(WatchTimeout);
        }
        catch (TimeoutException)
        {
            // no watch event arrived in time; return whatever was collected
        }
        finally
        {
            cancelTokenSource.Cancel();
        }
        return op;
    }

    private async Task<EtcdClientInstance> GetEtcdClientInstance(EtcdConnection etcdConnection)
    {
        var cacheKey = $"{etcdConnection.OwnerId}_{etcdConnection.Server}";
        var existClient = await _cacheService.Get<EtcdClientInstance>(cacheKey);
        if (existClient != null)
        {
            // test token, if invalid auth, then re-auth.
            // Skip the validation round-trip while the last validation is still fresh.
            var validationNeeded =
                existClient.EnableAuthenticated
                && DateTimeOffset.UtcNow - existClient.LastValidatedAt > ClientValidationInterval;
            if (validationNeeded)
            {
                try
                {
                    var userDetail = await existClient.Instance.UserGetAsync(
                        new AuthUserGetRequest { Name = etcdConnection.Username },
                        AddDefaultHeader(existClient)
                    );
                    if (userDetail == null)
                    {
                        throw new Exception("User not found");
                    }
                    existClient.LastValidatedAt = DateTimeOffset.UtcNow;
                }
                catch (Exception)
                {
                    await _cacheService.Remove(cacheKey);
                    return await GetEtcdClientInstance(etcdConnection);
                }
            }

            return existClient;
        }

        var client = new EtcdClient(
            $"{etcdConnection.Host}:{etcdConnection.Port}",
            configureChannelOptions: channelOptions =>
            {
                if (etcdConnection.Insecure)
                {
                    channelOptions.Credentials = ChannelCredentials.Insecure;
                }
            }
        );
        if (etcdConnection.EnableAuthenticated)
        {
            string? token;
            try
            {
                var authRes = await client.AuthenticateAsync(
                    new Etcdserverpb.AuthenticateRequest()
                    {
                        Name = etcdConnection.Username,
                        Password = _passwordProtector.Unprotect(etcdConnection.Password)
                    }
                );
                token = authRes != null ? authRes.Token : null;
            }
            catch
            {
                client.Dispose();
                throw;
            }
            if (!string.IsNullOrWhiteSpace(token))
            {
                var etcdClientInstance = new EtcdClientInstance()
                {
                    Instance = client,
                    Token = token,
                    EnableAuthenticated = true,
                    LastValidatedAt = DateTimeOffset.UtcNow
                };
                await CacheClientInstance(cacheKey, etcdClientInstance);
                return etcdClientInstance;
            }
            client.Dispose();
            throw new Exception($"Cannot get token from etcd server {etcdConnection.Server}");
        }
        else
        {
            var etcdClientInstance = new EtcdClientInstance()
            {
                Instance = client,
                Token = string.Empty,
                EnableAuthenticated = false,
                LastValidatedAt = DateTimeOffset.UtcNow
            };
            await CacheClientInstance(cacheKey, etcdClientInstance);
            return etcdClientInstance;
        }
    }

    private Task CacheClientInstance(string cacheKey, EtcdClientInstance etcdClientInstance)
    {
        // Residual race: a request that obtained the cached client just before
        // TTL expiry may use it after this callback disposes it. Low probability,
        // and self-healing: the failed request triggers re-auth on the next call.
        return _cacheService.Set(
            cacheKey,
            etcdClientInstance,
            ClientCacheTtl,
            evicted => evicted.Instance?.Dispose()
        );
    }

    private Grpc.Core.Metadata? AddDefaultHeader(EtcdClientInstance client)
    {
        if (client.EnableAuthenticated)
        {
            return new Grpc.Core.Metadata() { new Grpc.Core.Metadata.Entry("token", client.Token) };
        }
        else
        {
            return null;
        }
    }
}
