using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class BookTravelEventConsumer : IConsumer<BookTravelEvent>
    {
        public async Task Consume(ConsumeContext<BookTravelEvent> context)
        {
            var @event = context.Message;
            Console.WriteLine($"Event received to book seats for reservation: {@event.ReserveId}.");
            using (var dbcon = new TransportContext())
            {
                // remove reservation
                var reserv = await RemoveReservation(dbcon, @event);
                if (reserv == null)
                    return;
                // set booking
                var ret = await AddBooking(dbcon, reserv);
                if (!ret)
                    return;
                // update flightseats
                var res = await UpdateFlightSeats(dbcon, reserv);
                if (res == null)
                    Console.Error.WriteLine($"Couldn't update flight seat numbers");
            }
        }

        private async Task<Reservation?> RemoveReservation(TransportContext context, BookTravelEvent @event)
        {
            var reserv = from rsrv in context.Reservations
                         where rsrv.ReserveId == @event.ReserveId
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
        }
    }
}
