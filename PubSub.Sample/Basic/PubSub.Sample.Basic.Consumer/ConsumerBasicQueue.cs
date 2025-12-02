using System.Text;
using PubSub.Sample.Common;
using RabbitMQ.Client;

namespace PubSub.Sample.Basic.Consumer
{
    public class ConsumerBasicQueue
    {
        private const string QueueName = "basic.queue";

        public async Task ReceiveMessage()
        {
            using var connection = RabbitMqConnectionHelper.CreateConnection();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: QueueName, 
                durable: false, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            Console.WriteLine("Basic Queue Consumer is running...");
            Console.WriteLine("Waiting for messages. Press [Enter] to exit...");

            var cancellationTokenSource = new CancellationTokenSource();
            
            _ = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await channel.BasicGetAsync(QueueName, autoAck: true);
                    if (result != null)
                    {
                        var body = result.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($" [x] Received '{message}'");
                    }
                    else
                    {
                        await Task.Delay(100, cancellationTokenSource.Token);
                    }
                }
            }, cancellationTokenSource.Token);

            Console.ReadLine();
            cancellationTokenSource.Cancel();
            Console.WriteLine("Exiting...");
        }
    }
}