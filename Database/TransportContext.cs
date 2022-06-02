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
        public virtual DbSet<Change> Changes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Destination>().ToTable("Destination"); // table name overwrite (removing the plural "s")
            modelBuilder.Entity<Travel>().ToTable("Travel");
            modelBuilder.Entity<Source>().ToTable("Source");
            modelBuilder.Entity<Reservation>().ToTable("Reservation");
            modelBuilder.Entity<Change>().ToTable("Change");
        }
    }
}
