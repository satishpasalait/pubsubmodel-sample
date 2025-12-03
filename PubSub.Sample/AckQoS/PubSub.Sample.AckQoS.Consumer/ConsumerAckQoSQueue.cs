using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.AckQoS.Consumer
{
    public class ConsumerAckQoSQueue
    {
        private const string QueueName = "ackqos.queue";

        public async Task ReceiveMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false);

            Console.WriteLine("AckQoS Queue Consumer is running...");
            Console.WriteLine("Waiting for messages. Press [Enter] to exit...");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received '{message}'");

                // Simulate slow processing
                await Task.Delay(2500);

                Console.WriteLine($" [x] Processing '{message}'");

                // Simulate error
                if (message.Contains("error"))
                {
                    Console.WriteLine($" [x] Error processing '{message}' - will be requeued");
                    await channel.BasicNackAsync(
                        deliveryTag: e.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
                else
                {
                    await channel.BasicAckAsync(
                        deliveryTag: e.DeliveryTag,
                        multiple: false);
                    Console.WriteLine($" [x] Completed processing '{message}'");
                    Console.WriteLine($" [x] Acknowledged message '{message}'");
                }
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);

            Console.ReadLine();
            Console.WriteLine("Exiting...");
        }
    }
}