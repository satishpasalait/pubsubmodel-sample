namespace PubSub.Sample.OrderEventsSystem.OrderEvents;

public record OrderCreated(string OrderId, string CustomerEmail, decimal Amount);
public record OrderShipped(string OrderId, string TrackingNumber, string ShippingProvider);
public record OrderCancelled(string OrderId, string Reason);
public record PaymentReceived(string OrderId, decimal Amount, string PaymentMethod);