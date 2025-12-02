using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.Topic.Publisher
{
    public class PublisherTopicExchange
    {
        private const string ExchangeName = "topic.exchange";

        public async Task SendMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Topic Exchange Publisher is running...");
            Console.WriteLine("Format: <routingKey> <message>");
            Console.WriteLine("Example: order.created New Order 123");
            Console.WriteLine("Type a message and press Enter. Empty line to exit.");

            while (true)
            {
                Console.Write("Message: ");
                var message = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(message))
                {
                    Console.WriteLine("Exiting...");
                    break;
                }

                var firstSpaceIndex = message.IndexOf(' ');
                if (firstSpaceIndex == -1)
                {
                    Console.WriteLine("Invalid message format. Please use <routingKey> <message> format.");
                    continue;
                }

                var routingKey = message[..firstSpaceIndex];
                var messageBody = message[(firstSpaceIndex + 1)..];

                var body = Encoding.UTF8.GetBytes(messageBody);

                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine($" [x] Sent '{messageBody}' to exchange '{ExchangeName}' with routing key '{routingKey}'");
            }
        }
    }
}