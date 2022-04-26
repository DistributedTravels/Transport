using Models;

namespace Transport.Handlers
{
    public class ReserveTransportHandler : Handler
    {
        public ReserveTransportHandler(Action<EventModel> publish) : base(publish)
        {
        }

        public override Task HandleEvent(EventModel @event)
        {
            throw new NotImplementedException();
        }
    }
}
