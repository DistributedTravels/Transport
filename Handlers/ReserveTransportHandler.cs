using Models;

namespace Transport.Handlers
{
    public class ReserveTransportHandler : Handler
    {
        public ReserveTransportHandler(Action<EventModel> publish) : base(publish)
        {
            // additional init here
        }

        public override Task HandleEvent(EventModel @event)
        {
            // do event actions
            throw new NotImplementedException();
        }
    }
}
