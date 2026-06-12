using System.Net;
using System.Net.Sockets;

namespace EtcdManager.API.Core.Helpers;

/// <summary>
/// SSRF guard for user-supplied etcd server addresses.
/// Always blocks link-local/metadata (169.254.0.0/16), 0.0.0.0 and
/// multicast/reserved ranges. Loopback and RFC1918 private ranges are only
/// blocked when Etcd:BlockPrivateNetworks is enabled, because etcd servers
/// commonly live on private networks.
/// Only IP literals are checked; hostnames are allowed (no DNS resolution).
/// </summary>
public static class EtcdServerAddressValidator
{
    public static bool IsAllowed(string? server, bool blockPrivateNetworks)
    {
        if (string.IsNullOrWhiteSpace(server))
            return true; // NotEmpty rules handle empty input

        var host = ExtractHost(server);
        if (!IPAddress.TryParse(host, out var ip))
            return true; // plain hostname — no DNS resolution in the validator

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();

            // Always blocked: link-local / cloud metadata, unspecified, multicast/reserved
            if (bytes[0] == 169 && bytes[1] == 254)
                return false;
            if (ip.Equals(IPAddress.Any))
                return false;
            if (bytes[0] >= 224)
                return false;

            if (blockPrivateNetworks)
            {
                if (bytes[0] == 127)
                    return false;
                if (bytes[0] == 10)
                    return false;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return false;
                if (bytes[0] == 192 && bytes[1] == 168)
                    return false;
            }
        }
        else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Always blocked: link-local, multicast, unspecified
            if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.Equals(IPAddress.IPv6Any))
                return false;

            if (blockPrivateNetworks)
            {
                if (IPAddress.IsLoopback(ip))
                    return false;
                // fc00::/7 unique-local (IPv6 private range)
                if ((ip.GetAddressBytes()[0] & 0xFE) == 0xFC)
                    return false;
            }
        }

        return true;
    }

    private static string ExtractHost(string server)
    {
        var s = server.Trim();

        var schemeIndex = s.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex >= 0)
            s = s[(schemeIndex + 3)..];

        var slashIndex = s.IndexOf('/');
        if (slashIndex >= 0)
            s = s[..slashIndex];

        // Bracketed IPv6 literal, e.g. [::1]:2379
        if (s.StartsWith('['))
        {
            var end = s.IndexOf(']');
            return end > 0 ? s[1..end] : s;
        }

        var first = s.IndexOf(':');
        var last = s.LastIndexOf(':');
        if (first >= 0 && first == last)
            return s[..first]; // host:port

        return s; // no port, or bare IPv6 literal
    }
}
