using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.Infrastructure.Database;

public class EtcdManagerDataContext : DbContext
{
    private readonly string _connectionString = "";

    public DbSet<EtcdConnection> EtcdConnections { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Snapshot> Snapshots { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public EtcdManagerDataContext(string connectionString)
    {
        this._connectionString = connectionString;
    }

    public EtcdManagerDataContext(DbContextOptions<EtcdManagerDataContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(this._connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EtcdConnection>(entity =>
        {
            entity.ToTable("EtcdConnections");
            // set Id auto increment
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<Snapshot>(entity =>
        {
            entity.ToTable("Snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
        });
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedOnAdd();
            entity.HasIndex(x => x.TokenHash).IsUnique();
        });
    }

    public void SeedData(
        ILogger<EtcdManagerDataContext> logger,
        IWebHostEnvironment webHostEnvironment
    )
    {
        // check db file exist
        var dataDir = Path.Combine(webHostEnvironment.WebRootPath, "data");
        var filePath = Path.Combine(dataDir, "etcd-manager.db");
        var rawFilePath = Path.Combine(webHostEnvironment.WebRootPath, "rawdb", "etcd-manager.db");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        if (!File.Exists(filePath))
        {
            logger.LogInformation("Default database does not exist, copy from rawdb");
            File.Copy(rawFilePath, filePath, true);
        }
        // The schema originates from the pre-built rawdb file, so new tables
        // must be created explicitly for existing databases.
        if (this.Database.IsRelational())
        {
            this.Database.ExecuteSqlRaw(
                @"CREATE TABLE IF NOT EXISTS RefreshTokens (
                    Id INTEGER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY AUTOINCREMENT,
                    TokenHash TEXT NOT NULL,
                    UserId INTEGER NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    ConsumedAt TEXT NULL,
                    CreatedAt TEXT NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS IX_RefreshTokens_TokenHash ON RefreshTokens (TokenHash);"
            );

            // opportunistic purge so the RefreshTokens table does not grow unbounded
            var now = DateTime.UtcNow;
            var consumedCutoff = now.AddDays(-7);
            this.Database.ExecuteSql(
                $"DELETE FROM RefreshTokens WHERE ExpiresAt < {now} OR (ConsumedAt IS NOT NULL AND ConsumedAt < {consumedCutoff})"
            );
        }
        logger.LogInformation("Start seed data");
        if (!this.AppUsers.Any(x => x.Username == "root"))
        {
            logger.LogInformation("User root does not exist, create new...");
            var defaultPassword = Environment.GetEnvironmentVariable("ROOT_ACCOUNT_PASSWORD");

            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                throw new InvalidOperationException(
                    "ROOT_ACCOUNT_PASSWORD environment variable must be set before starting the application."
                );
            }

            logger.LogInformation("Creating root account from ROOT_ACCOUNT_PASSWORD.");

            this.AppUsers.Add(
                new AppUser()
                {
                    Username = "root",
                    Password = CommonHelper.HashPassword(defaultPassword)
                }
            );
            this.SaveChanges();
        }
        logger.LogInformation("seed data completed");
    }
}
