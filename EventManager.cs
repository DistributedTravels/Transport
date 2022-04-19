using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Transport.Models;
using Newtonsoft.Json;
using System.Text;

namespace Transport
{
    public class EventManager
    {
        private string _queueName;
        private IConnection _connection;
        public EventManager(string queueName)
        {
            this._queueName = queueName;
        }

        public void ListenForEvents()
        {
            var factory = new ConnectionFactory() {HostName="rabbitmq", Password="guest", UserName="guest"}; // connection details to "rabbitmq" broker on default port with credentials guest:guest
            _connection = factory.CreateConnection();
            var channel = _connection.CreateModel();
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += ReceiveEvent;
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        private async Task ReceiveEvent(object sender, BasicDeliverEventArgs args)
        {
            var eventName = args.RoutingKey; // type of message in queue
            switch(eventName)
            {
                default:
                    await ExampleProcessEvent(sender, args);
                    break;
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
