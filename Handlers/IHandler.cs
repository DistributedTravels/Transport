using Models;

namespace Transport.Handlers
{
    /**
     * <summary>
     * Abstract class to create further handlers for each event type.
     * </summary>
     */
    public abstract class IHandler
    {

        protected Action<EventModel> publish; // to allow for publishing within handler
        protected readonly WebApplication app; // required for database calls

        public IHandler(Action<EventModel> publish, WebApplication app)
        {
            this.publish = publish;
            this.app = app;
        }

        /**
         * <summary>
         * Method HandleEvent processes data transferred within event message and acts upon it.
         * </summary>
         */
        public abstract Task HandleEvent(string content);

        /**
         * <summary>
         * Method that uses EventManager's Publish to send "reply" events from within Handler.
         * </summary>
         */
        protected void PublishEvent(EventModel @event)
        {
            this.publish(@event);
        }
    }
}
