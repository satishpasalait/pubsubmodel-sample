using PubSub.Sample.Common;
using RabbitMQ.Client;
using System.Text;

namespace PubSub.Sample.ErrorHandling.Publisher;

public class PublisherErrorHandlingExchange
{
    private const string MainExchange = "main.exchange";
    private const string DlqExchange = "dlq.exchange";
    private const string MainQueue = "main.queue";
    private const string DlqQueue = "dlq.queue";
    private const string RoutingKey = "jobs";

    public async Task SendMessage()
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

        Console.WriteLine("Error Handling Exchange Publisher is running...");
        Console.WriteLine("Type job messages; ones containing 'fail' will simulate failures in worker.\nEmpty line = exit.\n");

        while (true)
        {
            Console.Write("Job: ");
            var job = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(job))
            {
                Console.WriteLine("Exiting...");
                break;
            }

            var body = Encoding.UTF8.GetBytes(job);

            await channel.BasicPublishAsync(
                exchange: MainExchange,
                routingKey: RoutingKey,
                body: new ReadOnlyMemory<byte>(body));

            Console.WriteLine($" [x] Sent '{job}' to exchange '{MainExchange}'");
        }
    }
}
