using EtcdManager.API.Infrastructure.Database;
using EtcdManager.API.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EtcdManager.API.Tests.Auth;

public class SeedDataTests
{
    private static readonly string WebRootPath =
        "/Users/vietprogrammer/Code/namnhcntt/etcd-manager/api/wwwroot";

    [Fact]
    public void SeedData_WithoutRootAccountPassword_ThrowsInvalidOperationException()
    {
        var db = TestDbHelper.CreateInMemoryDb("SeedTest_NoPassword");
        var logger = new Mock<ILogger<EtcdManagerDataContext>>().Object;
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(x => x.WebRootPath).Returns(WebRootPath);

        Environment.SetEnvironmentVariable("ROOT_ACCOUNT_PASSWORD", null);

        Assert.Throws<InvalidOperationException>(() => db.SeedData(logger, env.Object));
    }

    [Fact]
    public void SeedData_WithRootAccountPassword_CreatesRootUser()
    {
        var db = TestDbHelper.CreateInMemoryDb("SeedTest_WithPassword");
        var logger = new Mock<ILogger<EtcdManagerDataContext>>().Object;
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(x => x.WebRootPath).Returns(WebRootPath);

        Environment.SetEnvironmentVariable("ROOT_ACCOUNT_PASSWORD", "StrongP@ss123");

        try
        {
            db.SeedData(logger, env.Object);
            Assert.True(db.AppUsers.Any(x => x.Username == "root"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("ROOT_ACCOUNT_PASSWORD", null);
        }
    }
}
