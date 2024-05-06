using EtcdManager.API.Core.Helpers;
using EtcdManager.API.Domain;
using Microsoft.EntityFrameworkCore;

namespace EtcdManager.API.Infrastructure.Database
{
    public class EtcdManagerDataContext : DbContext
    {
        private readonly string _connectionString = "";

        public DbSet<EtcdConnection> EtcdConnections { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }

        public EtcdManagerDataContext(string connectionString)
        {
            this._connectionString = connectionString;
        }
        public EtcdManagerDataContext(DbContextOptions<EtcdManagerDataContext> options) : base(options) { }

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
        }

        internal void SeedData(ILogger<EtcdManagerDataContext> logger, IWebHostEnvironment webHostEnvironment)
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
            logger.LogInformation("Start seed data");
            if (!this.AppUsers.Any(x => x.Username == "root"))
            {
                logger.LogInformation("User root does not exist, create new...");
                var defaultPassword = Environment.GetEnvironmentVariable("ROOT_ACCOUNT_PASSWORD");

                if (!string.IsNullOrWhiteSpace(defaultPassword))
                {
                    logger.LogInformation("default root password is defined in ROOT_ACCOUNT_PASSWORD");
                }
                else
                {
                    logger.LogInformation("ROOT_ACCOUNT_PASSWORD does not set, use default password 'root'");
                    defaultPassword = "root";
                }

                this.AppUsers.Add(new AppUser()
                {
                    Username = "root",
                    Password = CommonHelper.SHA256Hash(defaultPassword)
                });
                this.SaveChanges();
            }
            logger.LogInformation("seed data completed");
        }
    }
}
