using EtcdManager.API.ApplicationService.Commands.KeyValues;
using EtcdManager.API.Domain;
using static EtcdManager.API.ApplicationService.Commands.KeyValues.ImportNodesCommand;

namespace EtcdManager.API.Tests.KeyValues;

public class ImportNodesValidatorTests
{
    private readonly ImportNodesCommandValidator _validator = new();

    [Fact]
    public void ValidItems_Pass()
    {
        var result = _validator.Validate(new ImportNodesCommand
        {
            EtcdConnectionId = 1,
            KeyValues = new[] { new KeyValue { Key = "/app/config", Value = "value" } }
        });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ItemWithEmptyKey_Fails()
    {
        var result = _validator.Validate(new ImportNodesCommand
        {
            EtcdConnectionId = 1,
            KeyValues = new[] { new KeyValue { Key = "", Value = "value" } }
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ItemWithKeyOver1024Chars_Fails()
    {
        var result = _validator.Validate(new ImportNodesCommand
        {
            EtcdConnectionId = 1,
            KeyValues = new[] { new KeyValue { Key = new string('k', 1025), Value = "value" } }
        });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ItemWithValueOver1MB_Fails()
    {
        var result = _validator.Validate(new ImportNodesCommand
        {
            EtcdConnectionId = 1,
            KeyValues = new[] { new KeyValue { Key = "/app/big", Value = new string('v', 1_048_577) } }
        });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Each value must be under 1MB.");
    }
}
