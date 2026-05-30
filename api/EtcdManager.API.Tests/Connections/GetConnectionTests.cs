using EtcdManager.API.ApplicationService.Queries.EtcdConnections;
using EtcdManager.API.Domain;
using EtcdManager.API.Domain.Services;
using EtcdManager.API.Tests.Helpers;
using Moq;
using Xunit;
using static EtcdManager.API.ApplicationService.Queries.EtcdConnections.GetConnectionByIdQuery;

namespace EtcdManager.API.Tests.Connections;

public class GetConnectionTests
{
    [Fact]
    public async Task GetConnectionById_DoesNotExposePassword()
    {
        var db = TestDbHelper.CreateInMemoryDb("GetConn_NoPassword");
        db.EtcdConnections.Add(new EtcdConnection
        {
            Name = "Test",
            Server = "localhost:2379",
            Username = "etcduser",
            Password = "supersecret",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mockUser = new Mock<IUserPrincipalService>();
        mockUser.Setup(x => x.Id).Returns(1);

        var handler = new GetConnectionByIdQueryHandler(db, mockUser.Object);
        var result = await handler.Handle(
            new GetConnectionByIdQuery { Id = 1 },
            CancellationToken.None
        );

        var type = result.GetType();
        var passwordProp = type.GetProperty("Password");
        Assert.Null(passwordProp);
    }

    [Fact]
    public async Task GetConnectionById_ReturnsCorrectFields()
    {
        var db = TestDbHelper.CreateInMemoryDb("GetConn_Fields");
        db.EtcdConnections.Add(new EtcdConnection
        {
            Name = "MyCluster",
            Server = "localhost:2379",
            Username = "etcduser",
            Password = "supersecret",
            OwnerId = 1,
            EnableAuthenticated = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mockUser = new Mock<IUserPrincipalService>();
        mockUser.Setup(x => x.Id).Returns(1);

        var handler = new GetConnectionByIdQueryHandler(db, mockUser.Object);
        var result = await handler.Handle(
            new GetConnectionByIdQuery { Id = 1 },
            CancellationToken.None
        );

        Assert.Equal("MyCluster", result.Name);
        Assert.Equal("localhost:2379", result.Server);
        Assert.Equal("etcduser", result.Username);
        Assert.True(result.EnableAuthenticated);
    }
}
