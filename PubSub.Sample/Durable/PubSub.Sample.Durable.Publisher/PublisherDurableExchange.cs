using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.Durable.Publisher
{
    public class PublisherDurableExchange
    {
        private const string ExchangeName = "durable.exchange";
        private const string RoutingKey = "Important";

        public async Task SendMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Durable Exchange Publisher is running...");
            Console.WriteLine("Type a message and press Enter. Empty line to exit.");

            var queueDeclareOk = await channel.QueueDeclareAsync(
                queue: "durable.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var queueName = queueDeclareOk.QueueName;

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            Console.WriteLine($"Bound queue [{queueName}] to exchange [{ExchangeName}] with routing key [{RoutingKey}]");

            while (true)
            {
                Console.Write("Message: ");
                var message = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("Exiting...");
                    break;
                }

                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine($" [x] Sent '{message}' to exchange [{ExchangeName}] with routing key [{RoutingKey}]");
            }
        }
    }
}