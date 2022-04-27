using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;

namespace Transport.Database
{
    public class TransportContext : DbContext
    {
        public TransportContext(DbContextOptions options) : base(options) { }

        public DbSet<Destination> Destinations { get; set; } // table creation
    }
}
