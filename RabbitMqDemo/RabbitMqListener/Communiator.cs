using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqListener
{
    internal class RabbitMqListener
    {
        private static IConnection GetConnection(string hostName, string userName, string password)
        {
            ConnectionFactory connectionFactory = new()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = 5672,
                VirtualHost = "/"
            };
            return connectionFactory.CreateConnection();
        }

        public static void Listen()
        {
            string queueName = "akd-queue";
            var rabbitMqConnection = GetConnection("", "", "");
            var rabbitMqChannel = rabbitMqConnection.CreateModel();

            rabbitMqChannel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

          //  rabbitMqChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            int messageCount = Convert.ToInt16(rabbitMqChannel.MessageCount(queueName));
            Console.WriteLine(" Listening to the queue. This channels has {0} messages on the queue", messageCount);

            var consumer = new EventingBasicConsumer(rabbitMqChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.Span);
                Console.WriteLine(" Location received: " + message);
                rabbitMqChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                Thread.Sleep(1000);
            };
            rabbitMqChannel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            Thread.Sleep(1000 * messageCount);
            Console.WriteLine(" Connection closed, no more messages.");
            Console.ReadLine();
        }
    }
}
