using MassTransit;
using Models.Transport;
using Models.Transport.Dto;
using Transport.Database;
using Transport.Database.Tables;
using Newtonsoft.Json;

namespace Transport.Consumers
{
    public class UpdateTransportTOEventConsumer : IConsumer<UpdateTransportTOEvent>
    {
        public async Task Consume(ConsumeContext<UpdateTransportTOEvent> context)
        {
            var @event = context.Message;

            switch (@event.Table)
            {
                case UpdateTransportTOEvent.Tables.DESTINATION:
                    await HandleDestinationUpdate(@event.DestinationDetails, @event.Action);
                    break;
                case UpdateTransportTOEvent.Tables.TRAVEL:
                    await HandleTravelUpdate(@event.TravelDetails, @event.Action);
                    break;
                default:
                    break;
            }
        }

        public async Task HandleDestinationUpdate(DestinationChangeDto dcd, UpdateTransportTOEvent.Actions action)
        {
            using (var context = new TransportContext())
            {
                var dest = from destination in context.Destinations where destination.Name == dcd.Name select destination;
                switch (action)
                {
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        if (dest.Any())
                        {
                            var single = dest.Single();
                            single.Distance = dcd.Distance >= 0 ? dcd.Distance : single.Distance; // only affects price
                        }
                        break;
                    default:
                        break;
                }
                if (context.ChangeTracker.HasChanges())
                    Console.WriteLine($"Transport: Change {action} in table DESTINATION");

                await context.SaveChangesAsync();
            }
        }

        public async Task HandleTravelUpdate(TravelChangeDto tcd, UpdateTransportTOEvent.Actions action)
        {
            using (var context = new TransportContext())
            {
                var trvl = from travel in context.Travels where travel.Id == tcd.Id select travel;
                switch (action)
                {
                    case UpdateTransportTOEvent.Actions.NEW:
                        if (!trvl.Any())
                            if(tcd.Destination.Length > 0 && tcd.Source.Length > 0 && tcd.AvailableSeats >= 0 && tcd.DepartureTime != DateTime.UnixEpoch && tcd.Price > 0)
                                context.Add(new Travel { DepartureTime = tcd.DepartureTime.ToUniversalTime(), Source = tcd.Source, Destination = tcd.Destination, FreeSeats = tcd.AvailableSeats, Price = tcd.Price });
                        break;
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        if (trvl.Any())
                        {
                            var utrvl = trvl.Single();
                            utrvl.Destination = tcd.Destination.Length > 0 ? tcd.Destination : utrvl.Destination;
                            utrvl.Source = tcd.Source.Length > 0 ? tcd.Source : utrvl.Source;
                            utrvl.Price = tcd.Price > 0 ? tcd.Price : utrvl.Price;
                            utrvl.DepartureTime = tcd.DepartureTime != DateTime.UnixEpoch ? tcd.DepartureTime.ToUniversalTime() : utrvl.DepartureTime;
                            utrvl.FreeSeats = tcd.AvailableSeats > 0 ? tcd.AvailableSeats : utrvl.FreeSeats;
                        }
                        break;
                    case UpdateTransportTOEvent.Actions.DELETE:
                        if (trvl.Any())
                            context.Remove(trvl.Single());
                        break;
                    default:
                        break;
                }
                if (context.ChangeTracker.HasChanges())
                    Console.WriteLine($"Transport: Change {action} in table TRAVEL");

                await context.SaveChangesAsync();
            }
        }
    }
}
