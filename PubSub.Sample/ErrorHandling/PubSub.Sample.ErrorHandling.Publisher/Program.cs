using PubSub.Sample.ErrorHandling.Publisher;

var publisher = new PublisherErrorHandlingExchange();
await publisher.SendMessage();
