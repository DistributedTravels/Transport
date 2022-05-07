using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class GetAvailableDestinationsEventConsumer : IConsumer<GetAvailableDestinationsEvent>
    {
        public async Task Consume(ConsumeContext<GetAvailableDestinationsEvent> context)
        {
            Console.WriteLine($"Received Event to pass destination list");
            var dests = new List<string>();
            using (var dbcon = new TransportContext())
            {
                var res = from dest in dbcon.Destinations
                          orderby dest.Name
                          select dest.Name;
                dests.AddRange(res);
            }
            if (dests.Count > 0) {
                Console.WriteLine($"Found destinations: {dests[0]}");
                await context.Publish(new GetAvailableDestinationsReplyEvent(context.Message.Id, dests));
            }
        }
    }
}
