using RabbitMQ.Client;
using System.Text;

namespace RabbitMqPublisher
{
    internal class Communiator
    {
        private static IConnection GetConnection(string hostName, string userName, string password)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = hostName;
            connectionFactory.UserName = userName;
            connectionFactory.Password = password;
            connectionFactory.Port = 5672;
            connectionFactory.VirtualHost = "/";
            return connectionFactory.CreateConnection();
        }
        public static void Send(string queue, string data)
        {
            using (IConnection connection = GetConnection("", "", ""))
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue, false, false, false, null);
                    channel.BasicPublish(string.Empty, queue, null, Encoding.UTF8.GetBytes(data));
                }
            }
        }
    }
}
