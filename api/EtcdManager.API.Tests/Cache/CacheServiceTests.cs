using EtcdManager.API.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace EtcdManager.API.Tests.Cache;

public class CacheServiceTests
{
    private static CacheService CreateService(out MemoryCache memoryCache)
    {
        memoryCache = new MemoryCache(new MemoryCacheOptions());
        return new CacheService(memoryCache);
    }

    [Fact]
    public async Task Set_WithoutTtl_StoresValue()
    {
        var service = CreateService(out _);

        await service.Set("key", "value");

        Assert.Equal("value", await service.Get<string>("key"));
    }

    [Fact]
    public async Task Set_WithTtl_ValueAvailableBeforeExpiration()
    {
        var service = CreateService(out _);

        await service.Set("key", "value", TimeSpan.FromMinutes(4));

        Assert.Equal("value", await service.Get<string>("key"));
    }

    [Fact]
    public async Task Set_WithTtl_ValueExpiresAfterTtl()
    {
        var service = CreateService(out _);

        await service.Set("key", "value", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        Assert.Null(await service.Get<string>("key"));
    }

    [Fact]
    public async Task Set_WithEvictionCallback_NotInvokedWhenReplaced()
    {
        var service = CreateService(out var memoryCache);
        string? evictedValue = null;

        await service.Set("key", "old", TimeSpan.FromMinutes(4), v => evictedValue = v);
        await service.Set("key", "new", TimeSpan.FromMinutes(4), v => evictedValue = v);

        // Eviction callbacks run asynchronously; give them time to fire.
        await Task.Delay(100);

        // The "old" entry was evicted with reason Replaced -> callback skipped.
        Assert.Null(evictedValue);
        Assert.Equal("new", memoryCache.Get<string>("key"));
    }

    [Fact]
    public async Task Set_WithEvictionCallback_InvokedOnRemove()
    {
        var service = CreateService(out var memoryCache);
        string? evictedValue = null;

        await service.Set("key", "value", TimeSpan.FromMinutes(4), v => evictedValue = v);
        await service.Remove("key");

        // Eviction callbacks fire asynchronously; compacting forces processing.
        memoryCache.Compact(0);
        for (var i = 0; i < 50 && evictedValue == null; i++)
        {
            await Task.Delay(10);
        }

        Assert.Equal("value", evictedValue);
    }
}
