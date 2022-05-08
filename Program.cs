using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;
using MassTransit;
using Transport.Database;
using Transport.Consumers;
using Models.Transport;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var ConnString = builder.Configuration.GetConnectionString("PsqlConnection");
TransportContext.ConnString = ConnString;

builder.Services.AddDbContext<TransportContext>(cfg =>
{
    cfg.UseNpgsql(ConnString)
       .LogTo(Console.WriteLine, LogLevel.Information)
       .EnableDetailedErrors();
});

builder.Services.AddMassTransit(cfg =>
{
    // adding consumers
    cfg.AddConsumer<BookTravelEventConsumer>();
    cfg.AddConsumer<UnbookTravelEventConsumer>();
    cfg.AddConsumer<GetAvailableDestinationsEventConsumer>();

    // telling masstransit to use rabbitmq
    cfg.UsingRabbitMq((context, rabbitCfg) =>
    {
        // rabbitmq config
        rabbitCfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        // automatic endpoint configuration (and I think the reason why naming convention is important
        rabbitCfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
initDB();
// bus for publishing a message, to check if everything works
// THIS SHOULD NOT EXIST IN FINAL PROJECT

/*var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq", "/", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });
});
busControl.Start();
await busControl.Publish(new GetAvailableDestinationsEvent());
busControl.Stop();*/

app.Run();

void initDB()
{
    using (var scope = app.Services.CreateScope())
    using (var context = scope.ServiceProvider.GetRequiredService<TransportContext>())
    {
        // init DB here?
        context.Database.EnsureCreated();
        if (!context.Destinations.Any()) {
            using(var r = new StreamReader(@"Init/dest.json"))
            {
                string json = r.ReadToEnd();
                List<string> dests = JsonConvert.DeserializeObject<List<string>>(json);
                foreach(var dest in dests)
                {
                    context.Destinations.Add(new Destination { Name = dest });
                }
            }
            using (var r = new StreamReader(@"Init/sources.json"))
            {
                string json = r.ReadToEnd();
                List<string> srcs = JsonConvert.DeserializeObject<List<string>>(json);
                foreach (var src in srcs)
                {
                    context.Sources.Add(new Source { Name = src });
                }
            }
            context.SaveChanges();
        }
    };
}