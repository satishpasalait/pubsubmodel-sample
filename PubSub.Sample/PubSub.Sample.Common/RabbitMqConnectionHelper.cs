using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PubSub.Sample.Common
{
    public class RabbitMqConnectionHelper
    {
        public static IConnection CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest",
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(10),
                    SocketReadTimeout = TimeSpan.FromSeconds(10),
                    SocketWriteTimeout = TimeSpan.FromSeconds(10),
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            }
            catch (BrokerUnreachableException ex)
            {
                throw new InvalidOperationException(
                    "Unable to connect to RabbitMQ server. " +
                    "Please ensure RabbitMQ is running and accessible at localhost:5672. " +
                    "You can check if RabbitMQ is running by visiting http://localhost:15672 (Management UI). " +
                    "To start RabbitMQ, run: rabbitmq-server (on Linux/Mac) or start the RabbitMQ service (on Windows).",
                    ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create RabbitMQ connection: {ex.Message}. " +
                    "Please check your RabbitMQ server configuration and network connectivity.",
                    ex);
            }
        }
    }
}
