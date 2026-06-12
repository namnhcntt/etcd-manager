namespace EtcdManager.API.Core.Helpers;

/// <summary>
/// Shared host/port parsing for etcd server addresses (previously duplicated
/// between EtcdConnection and TestConnectionCommand).
/// </summary>
public static class EtcdServerParser
{
    public static (string Host, string Port) ParseHostAndPort(string? server, bool insecure)
    {
        if (string.IsNullOrWhiteSpace(server))
            return (string.Empty, string.Empty);

        var arr = server.Split(':');
        var host = arr[0];

        if (!host.StartsWith("https://"))
        {
            if (host.StartsWith("http://"))
            {
                host = host.Replace("http://", "https://");
            }
            else if (insecure)
            {
                host = "http://" + host;
            }
            else
            {
                host = "https://" + host;
            }
        }

        var port = arr.Length > 1 ? arr[1] : "2379";
        return (host, port);
    }
}
