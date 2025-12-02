using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.Fanout.Publisher
{
    public class PublisherFanoutExchange
    {
        private const string ExchangeName = "fanout.exchange";

        public async Task SendMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Fanout Exchange Publisher is running...");
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

                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: string.Empty,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine($" [x] Sent '{message}' to exchange '{ExchangeName}'");
            }
        }
    }
}