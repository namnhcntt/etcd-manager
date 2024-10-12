using Microsoft.Extensions.Caching.Memory;

namespace EtcdManager.API.Infrastructure.Cache;

public class CacheService(IMemoryCache _memoryCache) : ICacheService
{
    public Task Set<T>(string key, T value)
    {
        _memoryCache.Set(key, value);
        return Task.CompletedTask;
    }

    public Task<T> Get<T>(string key)
    {
        return Task.FromResult(_memoryCache.Get<T>(key));
    }

    public Task Remove(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
