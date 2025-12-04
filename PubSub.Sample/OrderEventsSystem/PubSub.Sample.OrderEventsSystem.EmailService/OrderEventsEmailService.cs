using System.Text;
using System.Text.Json;
using PubSub.Sample.OrderEventsSystem.Common;
using PubSub.Sample.OrderEventsSystem.OrderEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Sample.OrderEventsSystem.EmailService;

public class OrderEventsEmailService
{
    private const string ExchangeName = "order.events.exchange";
    private const string QueueName = "order.events.email.queue";

    public async Task SendEmail()
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

        var bindingKey = "order.*";

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: bindingKey);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false);

        Console.WriteLine("Email Service is running...");
        Console.WriteLine($"Bound queue [{QueueName}] to exchange [{ExchangeName}] with binding key [{bindingKey}]");
        Console.WriteLine("Press [Enter] to exit...");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            Console.WriteLine($" [x] Received '{message}'");

            try
            {
                switch (e.RoutingKey)
                {
                    case "order.created":
                        var created = JsonSerializer.Deserialize<OrderCreated>(message);
                        if (created != null)
                        {
                            Console.WriteLine($"     -> Sending 'Order Created' email to {created.CustomerEmail} for Order {created.OrderId}");
                        }
                        break;
                    case "order.shipped":
                        var shipped = JsonSerializer.Deserialize<OrderShipped>(message);
                        if (shipped != null)
                        {
                            Console.WriteLine($"     -> Sending 'Order Shipped' email for Order {shipped.OrderId} - Tracking: {shipped.TrackingNumber}");
                        }
                        break;
                    case "order.cancelled":
                        var cancelled = JsonSerializer.Deserialize<OrderCancelled>(message);
                        if (cancelled != null)
                        {
                            Console.WriteLine($"     -> Sending 'Order Cancelled' email for Order {cancelled.OrderId} - Reason: {cancelled.Reason}");
                        }
                        break;
                    case "payment.received":
                        var received = JsonSerializer.Deserialize<PaymentReceived>(message);
                        if (received != null)
                        {
                            Console.WriteLine($"     -> Sending 'Payment Received' email for Order {received.OrderId} - Amount: {received.Amount:C}");
                        }
                        break;
                    default:
                        Console.WriteLine($"     -> Unknown event type: {e.RoutingKey}");
                        break;
                }

                await channel.BasicAckAsync(deliveryTag: e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"     -> Error processing event: {ex.Message}");
                await channel.BasicNackAsync(deliveryTag: e.DeliveryTag, multiple: false, requeue: true);
            }
            Console.WriteLine();
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        Console.ReadLine();
        Console.WriteLine("Exiting...");
    }
}