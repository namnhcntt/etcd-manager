using Microsoft.Extensions.Caching.Memory;

namespace EtcdManager.API.Infrastructure.Cache;

public class CacheService(IMemoryCache _memoryCache) : ICacheService
{
    public Task Set<T>(string key, T value)
    {
        _memoryCache.Set(key, value);
        return Task.CompletedTask;
    }

    public Task Set<T>(
        string key,
        T value,
        TimeSpan absoluteExpirationRelativeToNow,
        Action<T>? onEvicted = null
    )
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
        };
        if (onEvicted != null)
        {
            options.RegisterPostEvictionCallback(
                (_, evictedValue, reason, _) =>
                {
                    // Skip Replaced: when a Set overwrites an existing entry, the old
                    // value may have just been handed out to a concurrent reader and
                    // must not be cleaned up out from under it.
                    if (reason != EvictionReason.Replaced && evictedValue is T typedValue)
                    {
                        onEvicted(typedValue);
                    }
                }
            );
        }
        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task<T?> Get<T>(string key)
    {
        return Task.FromResult(_memoryCache.Get<T>(key));
    }

    public Task Remove(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
