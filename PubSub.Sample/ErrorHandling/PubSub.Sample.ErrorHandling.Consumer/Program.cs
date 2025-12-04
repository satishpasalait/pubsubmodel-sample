using PubSub.Sample.ErrorHandling.Consumer;

var consumer = new ConsumerErrorHandlingExchange();
await consumer.ReceiveMessage();

