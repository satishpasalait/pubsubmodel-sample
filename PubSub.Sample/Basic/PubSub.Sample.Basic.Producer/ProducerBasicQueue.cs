using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.Basic.Producer
{
    public class ProducerBasicQueue
    {
        private const string QueueName = "basic.queue";

        public async Task SendMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName, 
                durable: false, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            Console.WriteLine("Basic Queue Producer is running...");
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
                    exchange: "",
                    routingKey: QueueName,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine($" [x] Sent '{message}' to queue '{QueueName}'");
            }
        }
    }
}