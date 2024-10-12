using System.Text.Json;

namespace EtcdManager.API.Core.Helpers;

public static class EnvironmentHelper
{
    private static readonly string _scopeEnvironmentKey = "ASPNETCORE_ENVIRONMENT";

    public static bool IsProduction()
    {
        var rs = GetEnvironmentVariable<string>(_scopeEnvironmentKey)
            ?.ToLower()
            ?.StartsWith("production");
        return rs.HasValue && rs.Value;
    }

    public static T? GetEnvironmentVariable<T>(string key)
    {
        return GetEnvironmentVariable(key, default(T));
    }

    public static T? GetEnvironmentVariable<T>(string key, T defaultValue)
    {
        var val = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(val))
        {
            if (
                typeof(T) == typeof(string)
                || typeof(T) == typeof(DateTime)
                || typeof(T) == typeof(DateTime?)
                || typeof(T) == typeof(bool)
                || typeof(T) == typeof(bool?)
                || typeof(T) == typeof(int)
                || typeof(T) == typeof(int?)
                || typeof(T) == typeof(long)
                || typeof(T) == typeof(long?)
                || typeof(T) == typeof(decimal)
                || typeof(T) == typeof(decimal?)
            )
            {
                var data = (T)Convert.ChangeType(val, typeof(T));
                return data;
            }
            else
            {
                return JsonSerializer.Deserialize<T?>(val);
            }
        }
        return defaultValue;
    }
}
