using System.Text;
using System.Text.Json;
using PubSub.Sample.OrderEventsSystem.Common;
using PubSub.Sample.OrderEventsSystem.OrderEvents;
using RabbitMQ.Client;

namespace PubSub.Sample.OrderEventsSystem.Publisher;

public class PublisherOrderEvents
{
    private const string ExchangeName = "order.events.exchange";

    public async Task SendMessage()
    {
        using var connection = RabbitMqConnectionHelper.CreateConnection();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);

        Console.WriteLine("=== Order Events Publisher is running ===");
        Console.WriteLine("Choose an event type:");
        Console.WriteLine("1. Order Created");
        Console.WriteLine("2. Order Shipped");
        Console.WriteLine("3. Order Cancelled");
        Console.WriteLine("4. Payment Received");
        Console.WriteLine("0. Exit");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Select [0-4]: ");
            var choice = Console.ReadLine();

            if (!int.TryParse(choice, out var eventType))
            {
                Console.WriteLine("Invalid choice. Please enter a number between 0 and 4.");
                continue;
            }

            if (eventType < 0 || eventType > 4)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 0 and 4.");
                continue;
            }

            if (eventType == 0)
            {
                Console.WriteLine("Exiting...");
                break;
            }

            string routingKey;
            string json;

            switch (eventType)
            {
                case 1:
                    (routingKey, json) = CreateOrderCreated();
                    break;
                case 2:
                    (routingKey, json) = CreateOrderShipped();
                    break;
                case 3:
                    (routingKey, json) = CreateOrderCancelled();
                    break;
                case 4:
                    (routingKey, json) = CreatePaymentReceived();
                    break;
                default:
                    continue;
            }

            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                body: new ReadOnlyMemory<byte>(body));

            Console.WriteLine($" [x] Sent '{json}' with routing key '{routingKey}'");
            Console.WriteLine();
        }
        Console.WriteLine("Exiting...");
    }

    private (string routingKey, string json) CreateOrderCreated()
    {
        Console.Write("OrderId: ");
        var orderId = Console.ReadLine() ?? Guid.NewGuid().ToString("N");

        Console.Write("Amount: ");
        var amountText = Console.ReadLine() ?? "0";
        decimal.TryParse(amountText, out var amount);

        Console.Write("Customer Email: ");
        var email = Console.ReadLine() ?? $"customer-{orderId}@example.com";

        var evt = new OrderCreated(orderId, email, amount);
        var json = JsonSerializer.Serialize(evt);
        return ($"order.created", json);
    }

    private (string routingKey, string json) CreateOrderShipped()
    {
        Console.Write("OrderId: ");
        var orderId = Console.ReadLine() ?? Guid.NewGuid().ToString("N");

        Console.Write("Tracking Number: ");
        var tracking = Console.ReadLine() ?? $"TRACK-{orderId}";

        Console.Write("Shipping Provider: ");
        var provider = Console.ReadLine() ?? "UPS";

        var evt = new OrderShipped(orderId, tracking, provider);
        var json = JsonSerializer.Serialize(evt);
        return ($"order.shipped", json);
    }

    private (string routingKey, string json) CreateOrderCancelled()
    {
        Console.Write("OrderId: ");
        var orderId = Console.ReadLine() ?? Guid.NewGuid().ToString("N");

        Console.Write("Reason: ");
        var reason = Console.ReadLine() ?? "Cancelled";

        var evt = new OrderCancelled(orderId, reason);
        var json = JsonSerializer.Serialize(evt);
        return ($"order.cancelled", json);
    }

    private (string routingKey, string json) CreatePaymentReceived()
    {
        Console.Write("OrderId: ");
        var orderId = Console.ReadLine() ?? Guid.NewGuid().ToString("N");

        Console.Write("Amount: ");
        var amountText = Console.ReadLine() ?? "0";
        decimal.TryParse(amountText, out var amount);

        Console.Write("Payment Method: ");
        var method = Console.ReadLine() ?? "Credit Card";

        var evt = new PaymentReceived(orderId, amount, method);
        var json = JsonSerializer.Serialize(evt);
        return ($"payment.received", json);
    }
}