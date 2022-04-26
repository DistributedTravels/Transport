using Models;

namespace Transport.Handlers
{
    /**
     * <summary>
     * Abstract class to create further handlers for each event type.
     * </summary>
     */
    public abstract class Handler
    {

        private Action<EventModel> publish;

        public Handler(Action<EventModel> publish)
        {
            this.publish = publish;
        }

        /**
         * <summary>
         * Method HandleEvent processes data transferred within event message and acts upon it.
         * </summary>
         */
        public abstract Task HandleEvent(EventModel @event);

        /**
         * <summary>
         * Method that uses EventManager's Publish to send "reply" events from within Handler.
         * </summary>
         */
        private void PublishEvent(EventModel @event)
        {
            this.publish(@event);
        }
    }
}
