using EtcdManager.API.ApplicationService.Commands.Auths;
using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain;
using EtcdManager.API.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using EtcdManager.API.Domain.Services;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand;
using static EtcdManager.API.ApplicationService.Commands.Auths.LoginCommand.LoginCommandResult;

namespace EtcdManager.API.Tests.Auth;

public class LoginCommandTests
{
    private static IMemoryCache CreateMemoryCache() =>
        new MemoryCache(new MemoryCacheOptions());

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

        var handler = new LoginCommandHandler(mockTokenService.Object, db, CreateMemoryCache());

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
        var handler = new LoginCommandHandler(mockTokenService.Object, db, CreateMemoryCache());

        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(
                new LoginCommand { Username = "testuser", Password = "wrongpassword" },
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Login_UnknownUsername_ThrowsSameMessageAsWrongPassword()
    {
        var db = TestDbHelper.CreateInMemoryDb("LoginTest_Unknown");
        db.AppUsers.Add(new AppUser
        {
            Username = "testuser",
            Password = CommonHelper.HashPassword("correctpassword")
        });
        await db.SaveChangesAsync();

        var mockTokenService = new Mock<ITokenService>();
        var handler = new LoginCommandHandler(mockTokenService.Object, db, CreateMemoryCache());

        var unknownUserEx = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(
                new LoginCommand { Username = "nosuchuser", Password = "whatever" },
                CancellationToken.None
            )
        );
        var wrongPasswordEx = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(
                new LoginCommand { Username = "testuser", Password = "wrongpassword" },
                CancellationToken.None
            )
        );

        Assert.Equal("Invalid username or password", unknownUserEx.Message);
        Assert.Equal(wrongPasswordEx.Message, unknownUserEx.Message);
    }

    [Fact]
    public async Task Login_TooManyFailedAttempts_LocksUsername()
    {
        var db = TestDbHelper.CreateInMemoryDb("LoginTest_Lockout");
        var password = "correctpassword";
        db.AppUsers.Add(new AppUser
        {
            Username = "testuser",
            Password = CommonHelper.HashPassword(password)
        });
        await db.SaveChangesAsync();

        var mockTokenService = new Mock<ITokenService>();
        var handler = new LoginCommandHandler(mockTokenService.Object, db, CreateMemoryCache());

        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(
                    new LoginCommand { Username = "testuser", Password = "wrongpassword" },
                    CancellationToken.None
                )
            );
        }

        // even the correct password is rejected while locked out
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(
                new LoginCommand { Username = "testuser", Password = password },
                CancellationToken.None
            )
        );
        Assert.Equal("Too many failed login attempts. Please try again later.", ex.Message);
    }
}
