using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.Fanout.Consumer
{
    public class ConsumerFanoutExchange
    {
        private const string ExchangeName = "fanout.exchange";

        public async Task ReceiveMessage()
        {
            var subscriberName = $"sub-{Guid.NewGuid():N}".Substring(0, 8);

            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: false,
                autoDelete: false,
                arguments: null);

            var queueName = await channel.QueueDeclareAsync(
                queue: string.Empty,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: string.Empty);

            Console.WriteLine($"Fanout Subscriber [{subscriberName}] is running...");
            Console.WriteLine($"Bound queue [{queueName}] to exchange [{ExchangeName}]");
            Console.WriteLine("Press [Enter] to exit...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [{subscriberName}] Received '{message}'");
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer);

            Console.ReadLine();
            Console.WriteLine("Exiting...");
        }
    }
}