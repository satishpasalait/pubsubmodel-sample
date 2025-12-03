using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.AckQoS.Publisher
{
    public class PublisherAckQosQueue
    {
        private const string QueueName = "ackqos.queue";

        public async Task SendMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("AckQoS Queue Publisher is running...");
            Console.WriteLine("Sending 30 messages to demonstrate QoS...");

            for (int i = 0; i < 30; i++)
            {
                var message = $"Task #{i}";
                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: QueueName,
                    body: new ReadOnlyMemory<byte>(body));

                Console.WriteLine($" [x] Sent '{message}' to queue '{QueueName}'");
            }

            Console.WriteLine("All messages sent. Press [Enter] to exit...");
            Console.ReadLine();
        }
    }
}