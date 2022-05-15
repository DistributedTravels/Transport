using MassTransit;
using Models.Transport;
using Transport.Database;
using Transport.Database.Tables;

namespace Transport.Consumers
{
    public class GetAvailableSourcesEventConsumer : IConsumer<GetAvailableSourcesEvent>
    {
        public async Task Consume(ConsumeContext<GetAvailableSourcesEvent> context)
        {
            Console.WriteLine($"Received Event to pass destination list");
            var srcs = new List<string>();
            using (var dbcon = new TransportContext())
            {
                var res = from dest in dbcon.Sources
                          orderby dest.Name
                          select dest.Name;
                srcs.AddRange(res);
            }
            await context.Publish(new GetAvailableSourcesReplyEvent(context.Message.Id, context.Message.CorrelationId, srcs));
        }
    }
}
