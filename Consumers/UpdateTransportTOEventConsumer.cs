using MassTransit;
using Models.Transport;
using Models.Transport.Dto;
using Transport.Database;
using Transport.Database.Tables;

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
                    await HandleDestinationUpdate((DestinationChangeDto)@event.Details, @event.Action);
                    break;
                case UpdateTransportTOEvent.Tables.SOURCE:
                    await HandleSourceUpdate((SourceChangeDto)@event.Details, @event.Action);
                    break;
                case UpdateTransportTOEvent.Tables.TRAVEL:
                    await HandleTravelUpdate((TravelChangeDto)@event.Details, @event.Action);
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
                    case UpdateTransportTOEvent.Actions.NEW:
                        if (!dest.Any())
                            context.Add(new Destination { Name = dcd.Name, Distance = dcd.Distance });
                        break;
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        if (dest.Any())
                            dest.Single().Distance = dcd.Distance;
                        break;
                    case UpdateTransportTOEvent.Actions.DELETE:
                        if (dest.Any())
                            context.Remove(dest);
                        break;
                    default:
                        break;
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task HandleSourceUpdate(SourceChangeDto scd, UpdateTransportTOEvent.Actions action)
        {
            using (var context = new TransportContext())
            {
                var src = from source in context.Sources where source.Name == scd.Name select source;
                switch (action)
                {
                    case UpdateTransportTOEvent.Actions.NEW:
                        if (!src.Any())
                            context.Add(new Source { Name = scd.Name });
                        break;
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        // useless atm, need to redesign SourceChangeDto
                        if (src.Any())
                            src.Single().Name = scd.Name;
                        break;
                    case UpdateTransportTOEvent.Actions.DELETE:
                        if (src.Any())
                            context.Remove(src);
                        break;
                    default:
                        break;
                }
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
                            context.Add(new Travel { DepartureTime = tcd.DepartureTime.ToUniversalTime(), Source = tcd.Source, Destination = tcd.Destination, FreeSeats = tcd.AvailableSeats, Price = tcd.Price });
                        break;
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        if (trvl.Any())
                        {
                            var utrvl = trvl.Single();
                            utrvl.Destination = tcd.Destination;
                            utrvl.Source = tcd.Source;
                            utrvl.Price = tcd.Price;
                            utrvl.DepartureTime = tcd.DepartureTime.ToUniversalTime();
                            utrvl.FreeSeats = tcd.AvailableSeats;
                        }
                        break;
                    case UpdateTransportTOEvent.Actions.DELETE:
                        if (trvl.Any())
                            context.Remove(trvl);
                        break;
                    default:
                        break;
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
