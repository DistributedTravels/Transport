using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class UnreserveTravelEventConsumer : IConsumer<UnreserveTravelEvent>
    {
        public async Task Consume(ConsumeContext<UnreserveTravelEvent> context)
        {
            var @event = context.Message;
            using (var dbcon = new TransportContext())
            {
                var res = from reserv in dbcon.Reservations
                          where reserv.ReserveId == @event.ReserveId
                          select reserv;
                if (res.Any())
                {
                    var reserve = res.Single();
                    reserve.State = ReservationState.UNRESERVED;
                    await dbcon.SaveChangesAsync();
                }
            }
        }
    }
}
