using PubSub.Sample.Fanout.Consumer;
var consumer = new ConsumerFanoutExchange();
await consumer.ReceiveMessage();

