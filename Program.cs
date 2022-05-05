using Transport;
using Transport.Database;
using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;

var builder = WebApplication.CreateBuilder(args);

// DB connection creation
// User, Password and Database are configured in Docker/init/db/initdb.sql file
// MariaDB -> var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
//var connectionString = @"Host=psql;Username=Transport;Password=transport;Database=Transport";
// setting up DB as app service, some logging should be disabled for production
/*builder.Services.AddDbContext<TransportContext>(
            dbContextOptions => dbContextOptions
                .UseNpgsql(builder.Configuration.GetConnectionString("PsqlConnection"))
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );*/
var app = builder.Build();
var connString = builder.Configuration.GetConnectionString("PsqlConnection");
var manager = new EventManager(connString);
initDB();
manager.ListenForEvents();

/*var options = new DbContextOptionsBuilder<TransportContext>()
                .UseNpgsql(connString)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .Options;

using( var context =  new TransportContext(options))
{
    var test = new Destination { Name = "TestDest" };
    context.Destinations.Add(test); // add new item
    context.SaveChanges(); // save to DB
}*/

// example of inserting new Data to Database, Ensure created should be called at init of service (?)
/*using (var contScope = app.Services.CreateScope())
using (var context = contScope.ServiceProvider.GetRequiredService<TransportContext>())
{
    // Ensure Deleted possible to use for testing
    //context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
    var test = new Destination { Name = "TestDest" };
    context.Destinations.Add(test); // add new item
    context.SaveChanges(); // save to DB
    Console.WriteLine("Done inserting test data");
    // manager.Publish(new ReserveTransportEvent(1));
}*/
app.Run();

void initDB()
{

}