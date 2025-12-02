using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.Topic.Consumer
{
    public class ConsumerTopicExchange
    {
        private const string ExchangeName = "topic.exchange";

        public async Task ReceiveMessage(string routingKey)
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
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
                routingKey: routingKey);

            Console.WriteLine($"Topic Subscriber [{routingKey}] is running...");
            Console.WriteLine($"Bound queue [{queueName}] to exchange [{ExchangeName}]");
            Console.WriteLine("Press [Enter] to exit...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Routing Key: {routingKey}, Received '{message}'");
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