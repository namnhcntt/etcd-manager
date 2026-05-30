using EtcdManager.API.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EtcdManager.API.Tests.Auth;

public class TokenServiceTests
{
    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestKeyForUnitTestingPurposesOnly12345678901234567890123456789012",
                ["Jwt:Issuer"] = "etcdmanager.internal",
                ["Jwt:Audience"] = "etcdmanager.internal"
            })
            .Build();
    }

    [Fact]
    public async Task GenerateToken_ThenRefresh_Succeeds()
    {
        var config = BuildConfig();
        var service = new TokenService(config);

        var tokenData = await service.GenerateJwtTokenData(1, "testuser");
        Assert.NotEmpty(tokenData.Token);
        Assert.NotEmpty(tokenData.RefreshToken);

        var refreshed = await service.RefreshToken(tokenData.RefreshToken);
        Assert.NotEmpty(refreshed.Token);
    }

    [Fact]
    public async Task GenerateToken_HasCorrectExpiry()
    {
        var config = BuildConfig();
        var service = new TokenService(config);

        var tokenData = await service.GenerateJwtTokenData(1, "testuser");
        Assert.Equal(90000, tokenData.ExpiresIn);
    }
}
