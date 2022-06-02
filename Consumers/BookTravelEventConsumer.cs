using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class BookTravelEventConsumer : IConsumer<PurchaseTravelEvent>
    {
        public async Task Consume(ConsumeContext<PurchaseTravelEvent> context)
        {
            var @event = context.Message;
            using (var dbcon = new TransportContext())
            {
                // change reservation state if there is reservation
                var reserv = from rsrv in dbcon.Reservations
                             where rsrv.ReserveId == @event.ReserveId
                             && rsrv.State == ReservationState.RESERVED
                             select rsrv;
                if (reserv.Any())
                {
                    var data = reserv.Single();
                    data.State = ReservationState.PURCHASED;
                    Console.WriteLine($"Seats purchased for reservation: {@event.ReserveId} for travel: {data.TravelId}.");
                    await dbcon.SaveChangesAsync();  
                }
            }
        }

        /*private async Task<Reservation?> UpdateReservation(TransportContext context, BookTravelEvent @event)
        {
            var reserv = from rsrv in context.Reservations
                         where rsrv.ReserveId == @event.ReserveId
                         && rsrv.State == Reservation.ReservationState.RESERVED
                         select rsrv;
            if (reserv.Any())
            {
                var data = reserv.Single();
                var ret = new Reservation { TravelId = data.TravelId, ReservedSeats = data.ReservedSeats, ReserveId = data.ReserveId };
                context.Reservations.Remove(data);
                return ret;
            }
            return null;
        }

        private async Task<bool> AddBooking(TransportContext context, Reservation reserv)
        {
            var seats = from travel in context.Travels
                        where travel.Id == reserv.TravelId
                        select travel.FreeSeats;
            var res_seats = from res in context.Reservations
                            where res.ReserveId != reserv.ReserveId && res.TravelId == reserv.TravelId
                            select res.ReservedSeats;
            int free = seats.Single();
            if (res_seats.Any())
            {
                var count = res_seats.Aggregate(0, (a, b) => a + b);
                free -= count;
            }
            if (free < reserv.ReservedSeats)
                return false;
            context.Bookings.Add(new Booking { BookedId = reserv.ReserveId, ReservedSeats = reserv.ReservedSeats, TravelId = reserv.TravelId });
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<int?> UpdateFlightSeats(TransportContext context, Reservation rsrv)
        {
            var res = from travels in context.Travels
                      where travels.Id == rsrv.TravelId
                      select travels;
            if (res.Any())
            {
                var travel = res.Single();
                travel.FreeSeats -= rsrv.ReservedSeats;
                await context.SaveChangesAsync();
                return travel.FreeSeats;
            }
            return null;
        }*/
    }
}
