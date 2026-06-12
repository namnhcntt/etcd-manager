namespace EtcdManager.API.Infrastructure.Cache;

public interface ICacheService
{
    Task<T?> Get<T>(string key);
    Task Remove(string key);
    Task Set<T>(string key, T value);
    Task Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, Action<T>? onEvicted = null);
}
