using MassTransit;
using Models.Transport;
using Transport.Database;

namespace Transport.Consumers
{
    public class UnbookTravelEventConsumer : IConsumer<UnbookTravelEvent>
    {
        public async Task Consume(ConsumeContext<UnbookTravelEvent> context)
        {
            var @event = context.Message;
            Console.WriteLine($"Event received to unbook {@event.Seats} seats for flight {@event.FlightId}.");
            using (var dbcon = new TransportContext())
            {
                var res = await UpdateFlightSeats(dbcon, @event);
                if (res == null)
                    Console.Error.WriteLine($"Error updating seats, no flight found with id: {@event.FlightId}");
            }
        }

        private async Task<int?> UpdateFlightSeats(TransportContext context, UnbookTravelEvent @event)
        {
            var res = from travels in context.Travels
                      where travels.Id == @event.FlightId
                      select travels;
            if (res.Any())
            {
                var travel = res.Single();
                travel.FreeSeats += @event.Seats;
                await context.SaveChangesAsync();
                return travel.FreeSeats;
            }
            return null;
        }
    }
}
