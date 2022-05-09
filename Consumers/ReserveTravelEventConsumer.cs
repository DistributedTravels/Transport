using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class ReserveTravelEventConsumer : IConsumer<ReserveTravelEvent>
    {
        public async Task Consume(ConsumeContext<ReserveTravelEvent> context)
        {
            var @event = context.Message;
            var state = ReserveTravelReplyEvent.State.NOT_RESERVED;
            using(var dbcon = new TransportContext())
            {
                var res = from travel in dbcon.Travels
                          where travel.Id == @event.TravelId && travel.FreeSeats >= @event.Seats
                          select travel;
                if (res.Any())
                {
                    dbcon.Reservations.Add(new Reservation { ReservedSeats = @event.Seats, TravelId = @event.TravelId, ReserveId = @event.ReserveId});
                    await dbcon.SaveChangesAsync();
                    state = ReserveTravelReplyEvent.State.RESERVED;
                }
            }
            await context.Publish(new ReserveTravelReplyEvent(state, @event.CorrelationId));
        }
    }
}
