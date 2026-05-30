using EtcdManager.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.Tests.Helpers;

public static class TestDbHelper
{
    public static EtcdManagerDataContext CreateInMemoryDb(string dbName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<EtcdManagerDataContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new EtcdManagerDataContext(options);
    }
}
