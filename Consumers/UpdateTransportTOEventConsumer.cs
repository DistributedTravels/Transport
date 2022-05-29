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
                    await HandleDestinationUpdate((DestinationChangeDto)@event.Details);
                    break;
                case UpdateTransportTOEvent.Tables.SOURCE:
                    await HandleSourceUpdate((SourceChangeDto)@event.Details);
                    break;
                case UpdateTransportTOEvent.Tables.TRAVEL:
                    await HandleTravelUpdate((TravelChangeDto)@event.Details);
                    break;
                default:
                    break;
            }
        }

        public async Task HandleDestinationUpdate(DestinationChangeDto dcd)
        {
            throw new NotImplementedException();
        }

        public async Task HandleSourceUpdate(SourceChangeDto dcd)
        {
            throw new NotImplementedException();
        }

        public async Task HandleTravelUpdate(TravelChangeDto dcd)
        {
            throw new NotImplementedException();
        }
    }
}
