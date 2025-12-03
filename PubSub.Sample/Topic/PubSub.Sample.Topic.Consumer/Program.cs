using PubSub.Sample.Topic.Consumer;
var consumer = new ConsumerTopicExchange();
await consumer.ReceiveMessage(args[0]);
