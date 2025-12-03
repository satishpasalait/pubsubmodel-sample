using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.ErrorHandling.Consumer;

public class ConsumerErrorHandlingExchange
{

    private const string MainExchange = "main.exchange";
    private const string DlqExchange = "dlq.exchange";
    private const string MainQueue = "main.queue";
    private const string DlqQueue = "dlq.queue";
    private const string RoutingKey = "jobs";

    public async Task ReceiveMessage()
    {
        using var connection = RabbitMqConnectionHelper.CreateConnection();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: DlqExchange,
            type: ExchangeType.Direct,
            durable: false,
            autoDelete: false,
            arguments: null);

        await channel.QueueDeclareAsync(
            queue: DlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.QueueBindAsync(
            queue: DlqQueue,
            exchange: DlqExchange,
            routingKey: RoutingKey);

        await channel.ExchangeDeclareAsync(
            exchange: MainExchange,
            type: ExchangeType.Direct,
            durable: false,
            autoDelete: false,
            arguments: null);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", DlqExchange },
            { "x-dead-letter-routing-key", RoutingKey },
        };

        await channel.QueueDeclareAsync(
            queue: MainQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArgs);

        await channel.QueueBindAsync(
            queue: MainQueue,
            exchange: MainExchange,
            routingKey: RoutingKey);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false);

        Console.WriteLine("ConsumerErrorHandlingExchange is running...");
        Console.WriteLine("Press [Enter] to exit...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($" [x] Received '{message}'");

            try
            {
                Thread.Sleep(1000);

                if (message.Contains("fail", StringComparison.OrdinalIgnoreCase))
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
                }
            }
            catch (Exception)
            {
                Console.WriteLine($" [x] Error processing '{message}' - will be requeued");
                await channel.BasicNackAsync(
                    deliveryTag: e.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: MainQueue,
            autoAck: false,
            consumer: consumer);

        Console.ReadLine();
        Console.WriteLine("Exiting...");
    }
}