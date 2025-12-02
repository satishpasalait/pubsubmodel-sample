using PubSub.Sample.Fanout.Publisher;

var publisher = new PublisherFanoutExchange();
await publisher.SendMessage();