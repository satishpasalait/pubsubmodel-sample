using System.Text;
using PubSub.Sample.OrderEventsSystem.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.OrderEventsSystem.AnalyticsService;

public class OrderEventsAnalyticsService
{
    private const string ExchangeName = "order.events.exchange";
    private const string QueueName = "order.events.analytics.queue";

    public async Task AnalyzeEvents()
    {
        using var connection = RabbitMqConnectionHelper.CreateConnection();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var bindingKey = "#";

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: bindingKey);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false);

        Console.WriteLine("Analytics Service is running...");
        Console.WriteLine($"Bound queue [{QueueName}] to exchange [{ExchangeName}] with binding key [{bindingKey}]");
        Console.WriteLine("Press [Enter] to exit...");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [ANALYTICS] {DateTime.UtcNow:o} | RK={e.RoutingKey} | Msg={message}");

            await channel.BasicAckAsync(deliveryTag: e.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        Console.ReadLine();
        Console.WriteLine("Exiting...");
    }
}