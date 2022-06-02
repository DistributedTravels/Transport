using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class UnbookTravelEventConsumer : IConsumer<UnpurchaseTravelEvent>
    {
        public async Task Consume(ConsumeContext<UnpurchaseTravelEvent> context)
        {
            var @event = context.Message;
            using (var dbcon = new TransportContext())
            {
                var res = from reservation in dbcon.Reservations
                          where reservation.ReserveId == @event.ReserveId
                          select reservation;

                if (res.Any())
                {
                    var reserv = res.Single();
                    reserv.State = ReservationState.INVALIDATED;
                    await dbcon.SaveChangesAsync();
                }
            }
        }

        /*private async Task<int?> UpdateFlightSeats(TransportContext context, UnbookTravelEvent @event)
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
        }*/
    }
}
