using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain;

namespace EtcdManager.API.Tests.Helpers;

public class EtcdServerParserTests
{
    [Fact]
    public void HostAndPort_Secure()
    {
        var (host, port) = EtcdServerParser.ParseHostAndPort("localhost:2379", insecure: false);
        Assert.Equal("https://localhost", host);
        Assert.Equal("2379", port);
    }

    [Fact]
    public void HostAndPort_Insecure()
    {
        var (host, port) = EtcdServerParser.ParseHostAndPort("localhost:2379", insecure: true);
        Assert.Equal("http://localhost", host);
        Assert.Equal("2379", port);
    }

    [Fact]
    public void HostWithoutPort_DefaultsTo2379()
    {
        var (host, port) = EtcdServerParser.ParseHostAndPort("etcd.example.com", insecure: false);
        Assert.Equal("https://etcd.example.com", host);
        Assert.Equal("2379", port);
    }

    [Fact]
    public void EmptyServer_ReturnsEmpty()
    {
        var (host, port) = EtcdServerParser.ParseHostAndPort("", insecure: false);
        Assert.Equal(string.Empty, host);
        Assert.Equal(string.Empty, port);
    }

    [Fact]
    public void MatchesDomainEtcdConnectionProperties()
    {
        var connection = new EtcdConnection { Server = "10.0.0.5:4001", Insecure = true };
        Assert.Equal("http://10.0.0.5", connection.Host);
        Assert.Equal("4001", connection.Port);
    }
}
