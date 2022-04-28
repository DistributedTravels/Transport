using Models;
using Models.Transport;
using Newtonsoft.Json;
using Transport.Database;
using Transport.Database.Tables;
using System.Data.Entity;
using System.Linq;

namespace Transport.Handlers
{
    public class ReserveTransportHandler : IHandler
    {
        public ReserveTransportHandler(Action<EventModel> publish, WebApplication app) : base(publish, app)
        {
            // any additional init here
        }

        public override async Task HandleEvent(string content)
        {
            // deserialize object from string (since we know the exact type)
            var @event = JsonConvert.DeserializeObject<ReserveTransportEvent>(content);
            // do event actions
            Console.WriteLine($"Event received {@event.Id} msg: {content}");
            // example of reading from DB
            using (var contScope = this.app.Services.CreateScope())
            using (var context = contScope.ServiceProvider.GetRequiredService<TransportContext>())
            {
                // LINQ works too (async one)
                var res = context.Destinations.Where(x => x.Id == @event.DestId).FirstOrDefault();
                if (res != null)
                    Console.WriteLine($"First or Default Destination name in event: {res.Name}");

                // this async didn't seem to want to work!
                /*var ares = await (from b in context.Destinations
                                  where b.Id == @event.DestId
                                  select b).FirstOrDefaultAsync();
                if (ares != null)
                        Console.WriteLine($"Async First or Default Destination name in event: {ares.Name}");*/
            }
            //throw new NotImplementedException();
            //await Task.Yield();
        }
    }
}
