using Models;
using Models.Transport;
using Newtonsoft.Json;
using Transport.Database;
using Transport.Database.Tables;
using System.Data.Entity;
using System.Linq;

namespace Transport.Handlers
{
    public class ReserveTravelHandler : IHandler
    {
        public ReserveTravelHandler(Action<EventModel> publish, WebApplication app) : base(publish, app)
        {
            // any additional init here
        }

        public override async Task HandleEvent(string content)
        {
            // deserialize object from string (since we know the exact type)
            var @event = JsonConvert.DeserializeObject<ReserveTravelEvent>(content);
            // debug message
            Console.WriteLine($"Event received {@event.Id} msg: {content}");
            // find proper flight and "book" seats
            using (var contScope = this.app.Services.CreateScope())
            using (var context = contScope.ServiceProvider.GetRequiredService<TransportContext>())
            {
                var res = context.Travels.Where(x => x.Id == @event.FlightId).First();
                if (res != null)
                {
                    res.FreeSeats -= @event.Seats;
                    context.SaveChanges();
                }
                else
                {
                    Console.Error.WriteLine($"Error, no flight found with ID: {@event.Id}");
                }
            }
        }
    }
}
