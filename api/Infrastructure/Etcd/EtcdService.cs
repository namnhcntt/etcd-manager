﻿using System.Collections.Concurrent;
using dotnet_etcd;
using EtcdManager.API.Domain;
using EtcdManager.API.Infrastructure.Cache;
using Etcdserverpb;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Polly;

namespace EtcdManager.API.Infrastructure.Etcd;

public class EtcdService(ICacheService _cacheService, ILogger<EtcdService> _logger) : IEtcdService
{
    public async Task<bool> TestConnection(
        string host,
        string port,
        bool enableAuthenticated,
        bool insecure,
        string? username,
        string? password
    )
    {
        var client = new EtcdClient(
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
        var userDetail = await client.Instance.UserGetAsync(
            new AuthUserGetRequest { Name = etcdConnection.Username },
            AddDefaultHeader(client)
        );
        if (userDetail == null)
        {
            throw new Exception("User not found");
        }
        var roles = userDetail.Roles;
        var hasRoot = roles.Any(x => x == "root");
        if (!hasRoot)
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
        if (etcdConnection.EnableAuthenticated)
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
        await client.Instance.DeleteRangeAsync(key, AddDefaultHeader(client));

        if (deleteRecursive)
        {
            await client.Instance.DeleteRangeAsync($"{key}/", AddDefaultHeader(client));
        }
    }

    public async Task RenameKey(string oldKey, string newKey, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var oldKeyValue = await client.Instance.GetValAsync(oldKey, AddDefaultHeader(client));
        await client.Instance.DeleteAsync(oldKey, AddDefaultHeader(client));
        await client.Instance.PutAsync(newKey, oldKeyValue, AddDefaultHeader(client));
    }

    public async Task<List<KeyVersion>> GetRevisions(string key, EtcdConnection etcdConnection)
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var keyResult = await client.Instance.GetAsync(key, AddDefaultHeader(client));

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
        bool done = false;
        var cancelTokenSource = new CancellationTokenSource();

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
                        done = true;
                        break;
                    }
                }
            },
            AddDefaultHeader(client),
            null,
            cancelTokenSource.Token
        );

        var policy = Policy
            .HandleResult<bool>(x => !x)
            .WaitAndRetryAsync(5, retry => TimeSpan.FromSeconds(retry));
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

    public async Task<List<KeyVersion>> GetByKeyPrefix(
        string keyPrefix,
        EtcdConnection etcdConnection
    )
    {
        var client = await GetEtcdClientInstance(etcdConnection);
        var range = client.Instance.GetRange(keyPrefix, AddDefaultHeader(client));
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
        bool done = false;
        var cancelTokenSource = new CancellationTokenSource();
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
                    done = true;
                }
            },
            AddDefaultHeader(client),
            null,
            cancelTokenSource.Token
        );

        var policy = Policy
            .HandleResult<bool>(x => !x)
            .WaitAndRetryAsync(5, retry => TimeSpan.FromSeconds(retry));
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

    private async Task<EtcdClientInstance> GetEtcdClientInstance(EtcdConnection etcdConnection)
    {
        var cacheKey = $"{etcdConnection.OwnerId}_{etcdConnection.Server}";
        var existClient = await _cacheService.Get<EtcdClientInstance>(cacheKey);
        if (existClient != null)
        {
            // test token, if invalid auth, then re-auth
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
            }
            catch (Exception ex)
            {
                await _cacheService.Remove(cacheKey);
                return await GetEtcdClientInstance(etcdConnection);
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
            var token = string.Empty;
            var authRes = await client.AuthenticateAsync(
                new Etcdserverpb.AuthenticateRequest()
                {
                    Name = etcdConnection.Username,
                    Password = etcdConnection.Password
                }
            );
            token = authRes != null ? authRes.Token : null;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var etcdClientInstance = new EtcdClientInstance()
                {
                    Instance = client,
                    Token = token,
                    EnableAuthenticated = true
                };
                await _cacheService.Set(cacheKey, etcdClientInstance);
                return etcdClientInstance;
            }
            throw new Exception($"Cannot get token from etcd server {etcdConnection.Server}");
        }
        else
        {
            var etcdClientInstance = new EtcdClientInstance()
            {
                Instance = client,
                Token = string.Empty,
                EnableAuthenticated = false
            };
            await _cacheService.Set(cacheKey, etcdClientInstance);
            return etcdClientInstance;
        }
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
