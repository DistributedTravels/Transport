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
        public ReserveTravelHandler(Action<EventModel> publish, Action<EventModel, string, string> reply, string connString) 
            : base(publish, reply, connString)
        {
            // any additional init here
        }

        public override async Task HandleEvent(string content, string replyTo, string cId)
        {
            // deserialize object from string (since we know the exact type)
            var @event = JsonConvert.DeserializeObject<BookTravelEvent>(content);
            // debug message
            Console.WriteLine($"Event received {@event.Id} msg: {content}");
            // find proper flight and "book" seats
            using (var context = GetDbContext())
            {
                var res = await UpdateFlightSeats(context, @event);
                if (res == null)
                    Console.Error.WriteLine($"Error updating seats, no flight found with id: {@event.FlightId}");
            }
        }

        public async Task<int?> UpdateFlightSeats(TransportContext context, BookTravelEvent @event)
        {
            var res = from travels in context.Travels
                      where travels.Id == @event.FlightId
                      select travels;
            if (res.Any())
            {
                var travel = res.Single();
                travel.FreeSeats -= @event.Seats;
                await context.SaveChangesAsync();
                return travel.FreeSeats;
            }
            return null;
        }
    }
}
