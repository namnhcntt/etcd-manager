using EtcdManager.API.ApplicationService.Commands.EtcdConnections;
using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Infrastructure.Authentication;
using EtcdManager.API.Tests.Helpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static EtcdManager.API.ApplicationService.Commands.EtcdConnections.CreateConnectionCommand;
using static EtcdManager.API.ApplicationService.Commands.EtcdConnections.UpdateConnectionCommand;

namespace EtcdManager.API.Tests.Connections;

public class ConnectionPasswordTests
{
    private static IPasswordProtectorService CreateProtector()
    {
        return new PasswordProtectorService(
            new EphemeralDataProtectionProvider(),
            NullLogger<PasswordProtectorService>.Instance
        );
    }

    private static Mock<IUserPrincipalService> MockUser(int id = 1)
    {
        var mockUser = new Mock<IUserPrincipalService>();
        mockUser.Setup(x => x.Id).Returns(id);
        return mockUser;
    }

    [Fact]
    public async Task CreateConnection_ProtectsPasswordAtRest()
    {
        var db = TestDbHelper.CreateInMemoryDb("CreateConn_ProtectsPassword");
        var protector = CreateProtector();
        var handler = new CreateConnectionCommandHandler(db, MockUser().Object, protector);

        await handler.Handle(
            new CreateConnectionCommand
            {
                Name = "Test",
                Server = "localhost:2379",
                Username = "etcduser",
                Password = "supersecret",
                EnableAuthenticated = true
            },
            CancellationToken.None
        );

        var stored = db.EtcdConnections.Single();
        Assert.NotEqual("supersecret", stored.Password);
        Assert.Equal("supersecret", protector.Unprotect(stored.Password));
    }

    [Fact]
    public async Task UpdateConnection_EmptyPassword_DoesNotClobberStoredPassword()
    {
        var db = TestDbHelper.CreateInMemoryDb("UpdateConn_KeepPassword");
        var protector = CreateProtector();
        var existingProtected = protector.Protect("supersecret");
        db.EtcdConnections.Add(new EtcdConnection
        {
            Name = "Test",
            Server = "localhost:2379",
            Username = "etcduser",
            Password = existingProtected,
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateConnectionCommandHandler(db, MockUser().Object, protector);
        await handler.Handle(
            new UpdateConnectionCommand
            {
                Id = 1,
                Name = "Renamed",
                Server = "localhost:2379",
                Username = "etcduser",
                Password = null
            },
            CancellationToken.None
        );

        var stored = db.EtcdConnections.Single();
        Assert.Equal("Renamed", stored.Name);
        Assert.Equal(existingProtected, stored.Password);
    }

    [Fact]
    public async Task UpdateConnection_NewPassword_IsProtected()
    {
        var db = TestDbHelper.CreateInMemoryDb("UpdateConn_NewPassword");
        var protector = CreateProtector();
        db.EtcdConnections.Add(new EtcdConnection
        {
            Name = "Test",
            Server = "localhost:2379",
            Password = protector.Protect("oldsecret"),
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateConnectionCommandHandler(db, MockUser().Object, protector);
        await handler.Handle(
            new UpdateConnectionCommand
            {
                Id = 1,
                Name = "Test",
                Server = "localhost:2379",
                Password = "newsecret"
            },
            CancellationToken.None
        );

        var stored = db.EtcdConnections.Single();
        Assert.NotEqual("newsecret", stored.Password);
        Assert.Equal("newsecret", protector.Unprotect(stored.Password));
    }

    [Fact]
    public void Unprotect_LegacyPlaintextValue_ReturnsValueAsIs()
    {
        var protector = CreateProtector();
        Assert.Equal("legacy-plaintext", protector.Unprotect("legacy-plaintext"));
    }
}
