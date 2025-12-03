using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.Durable.Consumer
{
    public class ConsumerDurableExchange
    {
        private const string ExchangeName = "durable.exchange";
        private const string RoutingKey = "Important";
        private const string QueueName = "durable.queue";

        public async Task ReceiveMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null);

            var queueDeclareOk = await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var queueName = queueDeclareOk.QueueName;

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: RoutingKey);

            Console.WriteLine($"Durable Subscriber is running...");
            Console.WriteLine($"Bound queue [{queueName}] to exchange [{ExchangeName}] with routing key [{RoutingKey}]");
            Console.WriteLine("Press [Enter] to exit...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received '{message}'");
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