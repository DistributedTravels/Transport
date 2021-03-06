using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;
using System.Configuration;

namespace Transport.Database
{
    public class TransportContext : DbContext
    {
        public TransportContext(DbContextOptions<TransportContext> options) : base(options) { } // service creation constructor

        public TransportContext() : 
            base(new DbContextOptionsBuilder<TransportContext>()
                .UseNpgsql(ConnString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .Options) { }

        public static string ConnString {get; set;}
        public virtual DbSet<Destination> Destinations { get; set; } // table definition
        public virtual DbSet<Source> Sources { get; set; }
        public virtual DbSet<Travel> Travels { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Destination>().ToTable("Destination"); // table name overwrite (removing the plural "s")
            modelBuilder.Entity<Travel>().ToTable("Travel");
            modelBuilder.Entity<Source>().ToTable("Source");
            modelBuilder.Entity<Reservation>().ToTable("Reservation");
            modelBuilder.Entity<Booking>().ToTable("Booking");
        }

        /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // config overwrite, possibly unneeded (?)
        {
            // MariaDB -> var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
            //var connectionString = @"Host=psql;Username=Transport;Password=transport;Database=Transport";
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql(connString)
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            }
        }*/
    }
}
