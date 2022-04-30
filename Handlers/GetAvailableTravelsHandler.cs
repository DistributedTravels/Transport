using Models;
using Models.Transport;
using Newtonsoft.Json;
using Transport.Database;
using Transport.Database.Tables;
using System.Data.Entity;
using System.Linq;

namespace Transport.Handlers
{
    public class GetAvailableTravelsHandler : IHandler
    {
        public GetAvailableTravelsHandler(Action<EventModel> publish, WebApplication app) : base(publish, app)
        {
        }

        public override async Task HandleEvent(string content)
        {
            GetAvailableTravelsEvent? @event = JsonConvert.DeserializeObject<GetAvailableTravelsEvent>(content);
            if (@event != null)
            {
                Console.WriteLine($"Event {@event.GetType().Name} received.");
                // check for flights already for that day
                var StartDate = @event.DepartureTime.Date;
                var EndDate = @event.DepartureTime.Date.AddDays(1);
                IEnumerable<TravelItem>? final;
                using (var scope = this.app.Services.CreateScope())
                using(var context = scope.ServiceProvider.GetRequiredService<TransportContext>())
                {
                    var res = (from travel in context.Travels
                              where travel.DepartureTime >= StartDate && travel.DepartureTime < EndDate
                              select new Travel
                              {
                                  Id = travel.Id,
                                  DepartureTime = travel.DepartureTime,
                                  Type = travel.Type,
                                  FreeSeats = travel.FreeSeats,
                                  Source = travel.Source,
                                  Destination = travel.Destination,
                              }) as IEnumerable<Travel>;

                    if (res == null)
                    {
                        // no flights that day, generate some new ones
                        res = await GenerateTravels(StartDate);
                    }
                    final = FilterResult(res, @event.Type, @event.FreeSeats, @event.Source, @event.Destination);
                }
                // final is null if no transport found
                this.PublishEvent(new GetAvailableTravelsReplyEvent(@event.Id, final));
            } else
            {
                Console.Error.WriteLine("Error deserializing event type: GetAvailableTravelsEvent");
            }
        }

        private IEnumerable<TravelItem>? FilterResult(IEnumerable<Travel> travels, string type, int freeSeats, string source, string dest)
        {
            throw new NotImplementedException();
        }

        private Task<IEnumerable<Travel>> GenerateTravels(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
