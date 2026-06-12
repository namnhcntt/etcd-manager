using EtcdManager.API.ApplicationService.Commands.EtcdConnections;
using Microsoft.Extensions.Configuration;
using static EtcdManager.API.ApplicationService.Commands.EtcdConnections.TestConnectionCommand;

namespace EtcdManager.API.Tests.Connections;

public class ServerAddressValidationTests
{
    private static TestConnectionCommandValidator CreateValidator(bool blockPrivateNetworks)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Etcd:BlockPrivateNetworks"] = blockPrivateNetworks.ToString()
            })
            .Build();
        return new TestConnectionCommandValidator(configuration);
    }

    [Theory]
    [InlineData("169.254.169.254:2379")] // cloud metadata
    [InlineData("169.254.0.1")]
    [InlineData("0.0.0.0:2379")]
    [InlineData("224.0.0.1:2379")] // multicast
    [InlineData("http://169.254.169.254/latest/meta-data")]
    public void Validator_AlwaysBlocksLinkLocalAndReserved(string server)
    {
        var validator = CreateValidator(blockPrivateNetworks: false);
        var result = validator.Validate(new TestConnectionCommand { Server = server });
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("10.0.0.5:2379")]
    [InlineData("172.16.1.1:2379")]
    [InlineData("192.168.1.10:2379")]
    [InlineData("127.0.0.1:2379")]
    public void Validator_AllowsPrivateRanges_WhenFlagDisabled(string server)
    {
        var validator = CreateValidator(blockPrivateNetworks: false);
        var result = validator.Validate(new TestConnectionCommand { Server = server });
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("10.0.0.5:2379")]
    [InlineData("172.16.1.1:2379")]
    [InlineData("192.168.1.10:2379")]
    [InlineData("127.0.0.1:2379")]
    public void Validator_BlocksPrivateRanges_WhenFlagEnabled(string server)
    {
        var validator = CreateValidator(blockPrivateNetworks: true);
        var result = validator.Validate(new TestConnectionCommand { Server = server });
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("etcd.example.com:2379")]
    [InlineData("my-etcd-host")]
    [InlineData("8.8.8.8:2379")]
    public void Validator_AllowsHostnamesAndPublicIps(string server)
    {
        var validator = CreateValidator(blockPrivateNetworks: true);
        var result = validator.Validate(new TestConnectionCommand { Server = server });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAndUpdateValidators_AlsoBlockMetadataAddress()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var createValidator = new CreateConnectionCommand.CreateConnectionCommandValidator(configuration);
        var createResult = createValidator.Validate(new CreateConnectionCommand
        {
            Name = "Test",
            Server = "169.254.169.254:2379"
        });
        Assert.False(createResult.IsValid);

        var updateValidator = new UpdateConnectionCommand.UpdateConnectionCommandValidator(configuration);
        var updateResult = updateValidator.Validate(new UpdateConnectionCommand
        {
            Id = 1,
            Name = "Test",
            Server = "169.254.169.254:2379"
        });
        Assert.False(updateResult.IsValid);
    }
}
