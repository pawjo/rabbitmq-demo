using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQConsumer
{
    class Program
    {
        private static string[] _args;
        static void Main(string[] args)
        {
            _args = args;
            string host = readArg("-host", "localhost");
            string user = readArg("-user");
            string password = readArg("-password");
            string strPort = readArg("-port", "5672");
            int port = int.Parse(strPort);
            string queue = readArg("-queue", "");
            var declareQueue = readArg("-declare-queue");

            using (var connection = GetConnection(host, user, password, port))
            {
                using (var channel = connection.CreateModel())
                {
                    Console.WriteLine("Consumer starts...");

                    if (declareQueue != null && declareQueue == "true")
                    {
                        channel.QueueDeclare(queue: queue,
                            arguments: null,
                            exclusive: false,
                            autoDelete: false,
                            durable: false);
                    }

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, eventArgs) =>
                    {
                        var message = eventArgs.Body.ToArray();
                        Console.WriteLine("Message: " + Encoding.UTF8.GetString(message));
                    };

                    try
                    {
                        while (true)
                        {
                            channel.BasicConsume(queue: queue,
                                autoAck: true,
                                consumer: consumer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Connection error:");
                        Console.Write(e.Message);
                        Console.ReadKey();
                    }
                }
            }
        }

        private static string readArg(string key, string defaultValue = null)
        {
            int length = _args.Length;

            if (length >= 2)
            {
                length--;
                for (int i = 0; i < length; i++)
                {
                    if (_args[i] == key)
                        return _args[i + 1];
                }
            }

            return defaultValue;
        }

        private static IConnection GetConnection(string host, string user, string password, int port)
            => new ConnectionFactory()
            {
                HostName = host,
                UserName = user,
                Password = password,
                Port = port,
            }
            .CreateConnection();
    }
}
