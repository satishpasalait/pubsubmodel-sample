using PubSub.Sample.Durable.Consumer;
var consumer = new ConsumerDurableExchange();
await consumer.ReceiveMessage();

