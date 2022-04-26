using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Models;
using Models.Transport;
using Transport.Handlers;
using Newtonsoft.Json;
using System.Text;

namespace Transport
{
    public class EventManager
    {
        private List<Type> events = new List<Type>();
        private Dictionary<string, Handler> handlers =  new Dictionary<string, Handler>();
        private IConnection _connection;
        public EventManager()
        {
            this.RegisterHandler(new ReserveTransportHandler(this.Publish), typeof(ReserveTransportEvent));
            // register the rest of Handlers
        }

        public void ListenForEvents()
        {
            var factory = new ConnectionFactory() {HostName="rabbitmq", Password="guest", UserName="guest"}; // connection details to "rabbitmq" broker on default port with credentials guest:guest
            _connection = factory.CreateConnection();
            var channel = _connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += ReceiveEvent;
            foreach (var @event in this.events)
            {
                channel.QueueDeclare(queue: @event.Name, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicConsume(queue: @event.Name, autoAck: false, consumer: consumer);
            }
        }

        private void RegisterHandler(Handler handler, Type @event)
        {
            this.RegisterEvent(@event);
            this.handlers.Add(@event.Name, handler);
        }

        private void RegisterEvent(Type @event)
        {
           this.events.Add(@event);
        }

        private (Type?, Handler?) FindHandler(string eventName)
        {
            foreach(var @event in this.events)
            {
                if (@event.Name == eventName)
                    return (@event, this.handlers[eventName]);
            }
            return (null, null);
        }

        private async Task ReceiveEvent(object sender, BasicDeliverEventArgs args)
        {
            var eventName = args.RoutingKey; // type of message in queue
            (Type eventType, var handler) = FindHandler(eventName);
            if (handler != null && eventType != null)
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());
                // a little magic Type casting with Newtonsoft JSON
                dynamic @event = JsonConvert.DeserializeObject(message, eventType);
                await handler.HandleEvent(@event);
            } else {
                // write log about unknown event received?
            }
        }

        private void Publish(EventModel @event)
        {
            // TODO: it'd be nice to add retry policy here
            var eventName = @event.GetType().Name; // name of IntegrationEvent that gets published
            using(var channel = _connection.CreateModel())
            {
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                channel.BasicPublish(
                    exchange:"", // set exchange name
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            }
        }

        private async Task ExampleProcessEvent(object sender, BasicDeliverEventArgs args)
        {
            var message = Encoding.UTF8.GetString(args.Body.ToArray());
            // processing of the event here
            var data = JsonConvert.DeserializeObject(message) as EventModel;
            Console.WriteLine($"Received: {message}");
            var newEvent = new EventModel();
            // reply if needed
            Publish(newEvent);
            await Task.Yield();
        }
    }
}
