using PubSub.Sample.AckQoS.Publisher;
var publisher = new PublisherAckQosQueue();
await publisher.SendMessage();