using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;

namespace Transport.Database
{
    public class TransportContext : DbContext
    {
        public TransportContext(DbContextOptions<TransportContext> options) : base(options) { } // service creation constructor
        public TransportContext() { } // empty constructor, possibly unneeded (?)
        public DbSet<Destination> Destinations { get; set; } // table definition

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Destination>().ToTable("Destination"); // table name overwrite (removing the plural "s")
        }

        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // config overwrite, possibly unneeded (?)
        {
            // MariaDB -> var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
            var connectionString = @"Host=psql;Username=Transport;Password=transport;Database=Transport";
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql(connectionString)
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
        }*/
    }
}
