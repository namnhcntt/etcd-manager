using EtcdManager.API.Infrastructure.Authentication;
using EtcdManager.API.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        var db = TestDbHelper.CreateInMemoryDb("TokenTest_Refresh");
        var service = new TokenService(config, db);

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
        var db = TestDbHelper.CreateInMemoryDb("TokenTest_Expiry");
        var service = new TokenService(config, db);

        var tokenData = await service.GenerateJwtTokenData(1, "testuser");
        Assert.Equal(90000, tokenData.ExpiresIn);
    }

    [Fact]
    public async Task RefreshToken_Rotates_OldTokenIsConsumed()
    {
        var config = BuildConfig();
        var db = TestDbHelper.CreateInMemoryDb("TokenTest_Rotation");
        var service = new TokenService(config, db);

        var tokenData = await service.GenerateJwtTokenData(1, "testuser");
        var refreshed = await service.RefreshToken(tokenData.RefreshToken);

        Assert.NotEqual(tokenData.RefreshToken, refreshed.RefreshToken);
        Assert.Equal(2, db.RefreshTokens.Count());
        Assert.Equal(1, db.RefreshTokens.Count(x => x.ConsumedAt != null));
    }

    [Fact]
    public async Task RefreshToken_ReuseOfConsumedToken_RevokesAllUserTokens()
    {
        var config = BuildConfig();
        var db = TestDbHelper.CreateInMemoryDb("TokenTest_Reuse");
        var service = new TokenService(config, db);

        var tokenData = await service.GenerateJwtTokenData(1, "testuser");
        var refreshed = await service.RefreshToken(tokenData.RefreshToken);

        // re-presenting the consumed token must be rejected...
        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshToken(tokenData.RefreshToken)
        );

        // ...and all of the user's tokens revoked, so the rotated token is dead too
        Assert.All(db.RefreshTokens, x => Assert.NotNull(x.ConsumedAt));
        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshToken(refreshed.RefreshToken)
        );
    }

    [Fact]
    public async Task RefreshToken_UnknownToken_IsRejected()
    {
        var config = BuildConfig();
        var issuingDb = TestDbHelper.CreateInMemoryDb("TokenTest_Unknown_Issuer");
        var issuingService = new TokenService(config, issuingDb);
        var tokenData = await issuingService.GenerateJwtTokenData(1, "testuser");

        // valid JWT, but not present in this database
        var db = TestDbHelper.CreateInMemoryDb("TokenTest_Unknown");
        var service = new TokenService(config, db);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            service.RefreshToken(tokenData.RefreshToken)
        );
    }
}
