using PubSub.Sample.OrderEventsSystem.Publisher;
var publisher = new PublisherOrderEvents();
await publisher.SendMessage();
