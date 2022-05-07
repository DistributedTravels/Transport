﻿using MassTransit;
using Models.Transport;
using Transport.Database;

namespace Transport.Consumers
{
    public class BookTravelEventConsumer : IConsumer<BookTravelEvent>
    {
        public async Task Consume(ConsumeContext<BookTravelEvent> context)
        {
            var @event = context.Message;
            Console.WriteLine($"Event received to book {@event.Seats} seats for flight {@event.FlightId}.");
            using (var dbcon = new TransportContext())
            {
                var res = await UpdateFlightSeats(dbcon, @event);
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