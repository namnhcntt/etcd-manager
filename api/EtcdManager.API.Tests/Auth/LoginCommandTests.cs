using EtcdManager.API.ApplicationService.Commands.Auths;
using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain;
using EtcdManager.API.Tests.Helpers;
using Moq;
using Xunit;
using EtcdManager.API.Domain.Services;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand.LoginCommandResult;

namespace EtcdManager.API.Tests.Auth;

public class LoginCommandTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var db = TestDbHelper.CreateInMemoryDb("LoginTest_Valid");
        var password = "testpassword";
        db.AppUsers.Add(new AppUser
        {
            Username = "testuser",
            Password = CommonHelper.HashPassword(password)
        });
        await db.SaveChangesAsync();

        var mockTokenService = new Mock<ITokenService>();
        mockTokenService
            .Setup(x => x.GenerateJwtTokenData(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new JwtTokenData
            {
                Token = "fake-token",
                RefreshToken = "fake-refresh",
                ExpiresIn = 90000
            });

        var handler = new LoginCommandHandler(mockTokenService.Object, db);

        var result = await handler.Handle(
            new LoginCommand { Username = "testuser", Password = password },
            CancellationToken.None
        );

        Assert.Equal("fake-token", result.Token);
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsException()
    {
        var db = TestDbHelper.CreateInMemoryDb("LoginTest_Invalid");
        db.AppUsers.Add(new AppUser
        {
            Username = "testuser",
            Password = CommonHelper.HashPassword("correctpassword")
        });
        await db.SaveChangesAsync();

        var mockTokenService = new Mock<ITokenService>();
        var handler = new LoginCommandHandler(mockTokenService.Object, db);

        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(
                new LoginCommand { Username = "testuser", Password = "wrongpassword" },
                CancellationToken.None
            )
        );
    }
}
