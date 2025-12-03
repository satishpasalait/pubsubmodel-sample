using PubSub.Sample.Durable.Publisher;
var publisher = new PublisherDurableExchange();
await publisher.SendMessage();