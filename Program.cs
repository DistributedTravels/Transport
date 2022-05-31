using Microsoft.EntityFrameworkCore;
using Transport.Database.Tables;
using MassTransit;
using Transport.Database;
using Transport.Consumers;
using Newtonsoft.Json;
using Models.Transport;
using Models.Transport.Dto;

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
    cfg.AddConsumer<GetAvailableSourcesEventConsumer>();
    cfg.AddConsumer<GetAvailableTravelsEventConsumer>();
    cfg.AddConsumer<ReserveTravelEventConsumer>();
    cfg.AddConsumer<UnreserveTravelEventConsumer>();
    cfg.AddConsumer<UpdateTransportTOEventConsumer>();

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

//testChanges();

app.Run();

/*async void testChanges()
{
    // bus for publishing a message, to check if everything works
    // THIS SHOULD NOT EXIST IN FINAL PROJECT
    await Task.Delay(15000);
    var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
    busControl.Start();
    /* testing Distance table */
    /*await busControl.Publish(new UpdateTransportTOEvent() { 
        Action = UpdateTransportTOEvent.Actions.NEW, 
        Table = UpdateTransportTOEvent.Tables.DESTINATION,
        DestinationDetails = new DestinationChangeDto { Name = "Abcd", Distance=2000 }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.UPDATE,
        Table = UpdateTransportTOEvent.Tables.DESTINATION,
        DestinationDetails = new DestinationChangeDto { Name = "Abcd", NewName = "Efgh", Distance = 1000 }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.DESTINATION,
        DestinationDetails = new DestinationChangeDto { Name = "Abcd" }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.DESTINATION,
        DestinationDetails = new DestinationChangeDto { Name = "Efgh" }
    });
    /* testing Source table */
    /*await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.NEW,
        Table = UpdateTransportTOEvent.Tables.SOURCE,
        SourceDetails = new SourceChangeDto { Name = "Abcd"}
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.UPDATE,
        Table = UpdateTransportTOEvent.Tables.SOURCE,
        SourceDetails = new SourceChangeDto { Name = "Abcd", NewName = "Efgh"}
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.SOURCE,
        SourceDetails = new SourceChangeDto { Name = "Abcd" }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.SOURCE,
        SourceDetails = new SourceChangeDto { Name = "Efgh" }
    });
    /* testing Travel table */
    /*await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.NEW,
        Table = UpdateTransportTOEvent.Tables.TRAVEL,
        TravelDetails = new TravelChangeDto { Source = "Abcd" }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.NEW,
        Table = UpdateTransportTOEvent.Tables.TRAVEL,
        TravelDetails = new TravelChangeDto { Source = "Warszawa", Destination = "Hiszpania", AvailableSeats = 10, DepartureTime = DateTime.Now, Price = 1869.25 }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.UPDATE,
        Table = UpdateTransportTOEvent.Tables.TRAVEL,
        TravelDetails = new TravelChangeDto { Id = 1, Price = 226.99 }
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.TRAVEL,
        TravelDetails = new TravelChangeDto {}
    });
    await Task.Delay(4000);
    await busControl.Publish(new UpdateTransportTOEvent()
    {
        Action = UpdateTransportTOEvent.Actions.DELETE,
        Table = UpdateTransportTOEvent.Tables.TRAVEL,
        TravelDetails = new TravelChangeDto { Id = 1, Source = "Efgh" }
    });
    /* end of tests */
    /*busControl.Stop();
}*/

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
                List<Dest> dests = JsonConvert.DeserializeObject<List<Dest>>(json);
                foreach(var dest in dests)
                {
                    context.Destinations.Add(new Destination { Name = dest.Destination, Distance = dest.Distance });
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

public class Dest
{
    public string Destination { get; set; }
    public int Distance { get; set; }

}