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
        private readonly int GenerationSourceCount = 7; // might be too little
        private readonly int GenerationDestinationCount = 14; // might be too little
        private readonly int SeatMin = 80;
        private readonly int SeatMax = 130;
        private readonly Random random = new(); // fancy!
        public GetAvailableTravelsHandler(Action<EventModel> publish, Action<EventModel, string, string> reply, string connString) 
            : base(publish, reply, connString)
        {
        }

        public override async Task HandleEvent(string content, string replyTo, string cId)
        {
            GetAvailableTravelsEvent? @event = JsonConvert.DeserializeObject<GetAvailableTravelsEvent>(content);
            if (@event != null)
            {
                Console.WriteLine($"Event {@event.GetType().Name} received.");
                // check for flights already for that day
                var StartDate = @event.DepartureTime.Date;
                var EndDate = @event.DepartureTime.Date.AddDays(1);
                IEnumerable<TravelItem>? final;
                using (var context = new TransportContext(this.GetDbOptions()))
                {
                    var res = from travel in context.Travels
                              where travel.DepartureTime >= StartDate && travel.DepartureTime < EndDate
                              select new Travel
                              {
                                  Id = travel.Id,
                                  DepartureTime = travel.DepartureTime,
                                  FreeSeats = travel.FreeSeats,
                                  Source = travel.Source,
                                  Destination = travel.Destination,
                              };

                    if (!res.Any())
                    {
                        // no flights that day, generate some new ones
                        res = GenerateTravels(StartDate, context).AsQueryable();
                    }
                    final = FilterResult(res, @event.FreeSeats, @event.Source, @event.Destination);
                }
                // final is null if no transport found
                this.reply(new GetAvailableTravelsReplyEvent(@event.Id, final), replyTo, cId);
            } else
            {
                Console.Error.WriteLine("Error deserializing event type: GetAvailableTravelsEvent");
            }
        }

        public IEnumerable<TravelItem>? FilterResult(IEnumerable<Travel> travels, int freeSeats, string source, string dest)
        {
            var tmp = new List<Travel>();
            foreach (var travel in travels)
                if ((travel.Source == source || source == "any") // if source matches or any source is fine
                    && (travel.Destination == dest || dest == "any") // if destination matches or any dest is fine
                    && freeSeats <= travel.FreeSeats) // if there is enough seats
                    tmp.Add(travel);

            var final = new List<TravelItem>();
            foreach (var travel in tmp)
                final.Add(new TravelItem(travel.Id, travel.Source, travel.Destination, travel.DepartureTime, travel.FreeSeats));

            if(final.Count == 0)
                return null;

            return final;
        }

        public IEnumerable<Travel> GenerateTravels(DateTime date, TransportContext context)
        {
            var generated = new List<Travel>();

            var sources = (from source in context.Sources
                          orderby Guid.NewGuid() // introducing random order
                          select source.Name).Take(GenerationSourceCount);

            var destinations = (from dest in context.Destinations
                           orderby Guid.NewGuid() // introducing random order
                           select dest.Name).Take(GenerationDestinationCount);

            foreach (var dest in destinations)
            {
                foreach (var source in sources)
                {
                    // generate new transport from every source to each destination
                    generated.Add(new Travel { 
                        DepartureTime = date.AddHours(random.Next(0,23)).AddMinutes(random.Next(0,11) * 5), 
                        Source = source, 
                        Destination = dest, 
                        FreeSeats = random.Next(SeatMin, SeatMax) 
                    });
                }
            }

            context.AddRange(generated);
            context.SaveChanges();

            return generated;
        }
    }
}
