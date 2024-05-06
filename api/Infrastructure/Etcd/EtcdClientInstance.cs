﻿using dotnet_etcd;

namespace EtcdManager.API.Infrastructure.Etcd
{
    public class EtcdClientInstance
    {
        public EtcdClient Instance { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
