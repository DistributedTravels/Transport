using Transport;
using Transport.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB connection creation
var connectionString = "server=mariadb;user=Transport;password=transport;database=Transport";
builder.Services.AddDbContext<TransportContext>(
            dbContextOptions => dbContextOptions
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );

var app = builder.Build();
var manager = new EventManager();
manager.ListenForEvents();
app.Run();