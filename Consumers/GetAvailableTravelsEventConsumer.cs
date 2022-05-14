using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class GetAvailableTravelsEventConsumer : IConsumer<GetAvailableTravelsEvent>
    {

        private readonly int GenerationSourceCount = 7; // might be too little
        private readonly int GenerationDestinationCount = 14; // might be too little
        private readonly int SeatMin = 80;
        private readonly int SeatMax = 130;
        private readonly Random random = new(); // fancy!

        public async Task Consume(ConsumeContext<GetAvailableTravelsEvent> context)
        {
            var @event = context.Message;
            if (@event != null)
            {
                if (@event.TravelId != null)
                {
                    List<TravelItem>? items = new List<TravelItem>();
                    using(var dbcon = new TransportContext())
                    {
                        var res = from travel in dbcon.Travels
                                  where travel.Id == @event.TravelId
                                  select new Travel
                                  {
                                      Id = travel.Id,
                                      DepartureTime = travel.DepartureTime.ToUniversalTime(),
                                      FreeSeats = travel.FreeSeats,
                                      Source = travel.Source,
                                      Destination = travel.Destination,
                                      Price = travel.Price,
  
                                  };
                        var reserv = from reservations in dbcon.Reservations
                                     where reservations.TravelId == @event.TravelId
                                     select reservations.ReservedSeats;
                        var reserved = reserv.Any() ? reserv.Aggregate(0, (a, b) => a + b) : 0;
                        if (res.Any())
                        {
                            var data = res.Single();
                            items.Add(new TravelItem(data.Id, data.Source, data.Destination, data.DepartureTime, data.FreeSeats -  reserved, data.Price));
                        } else
                            items = null;
                        await context.RespondAsync(new GetAvailableTravelsReplyEvent(@event.Id, @event.CorrelationId, items));
                    }
                }
                else
                {
                    Console.WriteLine($"Event {@event.GetType().Name} received.");
                    // check for flights already for that day
                    var StartDate = @event.DepartureTime.Date.ToUniversalTime();
                    var EndDate = @event.DepartureTime.Date.AddDays(1).ToUniversalTime();
                    IEnumerable<TravelItem>? final;
                    using (var dbcon = new TransportContext())
                    {
                        var res = from travel in dbcon.Travels
                                  where travel.DepartureTime >= StartDate && travel.DepartureTime < EndDate
                                  select new Travel
                                  {
                                      Id = travel.Id,
                                      DepartureTime = travel.DepartureTime.ToUniversalTime(),
                                      FreeSeats = travel.FreeSeats,
                                      Source = travel.Source,
                                      Destination = travel.Destination,
                                  };

                        if (!res.Any())
                        {
                            // no flights that day, generate some new ones
                            res = GenerateTravels(StartDate, dbcon).AsQueryable();
                        }
                        final = FilterResult(res, @event.FreeSeats, @event.Source, @event.Destination);
                        final = CheckForReservations(final, @event.FreeSeats, dbcon);
                    }
                    // final is null if no transport found
                    await context.Publish(new GetAvailableTravelsReplyEvent(@event.Id, @event.CorrelationId, final));
                }
            }
            else
            {
                Console.Error.WriteLine("Error deserializing event type: GetAvailableTravelsEvent");
            }
        }

        public double UpdatePrice(string dest, TransportContext context)
        {
            var dist = from destin in context.Destinations
                        where dest == destin.Name
                        select destin.Distance;
            var price = dist.Single() / 2.0 + random.NextDouble() * 300;
            return Math.Round(price, 2);
        }

        public IEnumerable<TravelItem>? CheckForReservations(IEnumerable<TravelItem>? travels, int seats, TransportContext dbcon)
        {
            if (travels == null || travels.Count() <= 0)
                return null;
            var final = new List<TravelItem>();
            foreach (var travel in travels)
            {
                var res = from reserv in dbcon.Reservations
                          where reserv.TravelId == travel.TravelId
                          select reserv.ReservedSeats;
                if (res.Any())
                {
                    travel.AvailableSeats -= res.Aggregate(0, (a, b) => a + b);
                }
                if (travel.AvailableSeats >= seats)
                    final.Add(travel);
            }

            if (!final.Any())
                final = null;
            return final;
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
                final.Add(new TravelItem(travel.Id, travel.Source, travel.Destination, travel.DepartureTime, travel.FreeSeats, 0));

            if (final.Count == 0)
                return null;

            return final;
        }

        public IEnumerable<Travel> GenerateTravels(DateTime date, TransportContext context)
        {
            var generated = new List<Travel>();

            var sources = (from source in context.Sources
                           //orderby Guid.NewGuid() // introducing random order
                           select source.Name).ToList().OrderBy(s => Guid.NewGuid()).Take(GenerationSourceCount).ToList();

            var destinations = (from dest in context.Destinations
                                //orderby Guid.NewGuid() // introducing random order
                                select dest.Name).ToList().OrderBy(d => Guid.NewGuid()).Take(GenerationDestinationCount).ToList();

            foreach (var dest in destinations)
            {
                foreach (var source in sources)
                {
                    // generate new transport from every source to each destination
                    generated.Add(new Travel
                    {
                        DepartureTime = date.AddHours(random.Next(0, 23)).AddMinutes(random.Next(0, 11) * 5),
                        Source = source,
                        Destination = dest,
                        FreeSeats = random.Next(SeatMin, SeatMax),
                        Price = UpdatePrice(dest, context)
                    });
                    // generate new tranport from every dest to each source
                    generated.Add(new Travel
                    {
                        DepartureTime = date.AddHours(random.Next(0, 23)).AddMinutes(random.Next(0, 11) * 5),
                        Source = dest,
                        Destination = source,
                        FreeSeats = random.Next(SeatMin, SeatMax),
                        Price = UpdatePrice(dest, context)
                    });
                }
            }

            context.AddRange(generated);
            context.SaveChanges();

            return generated;
        }
    }
}
