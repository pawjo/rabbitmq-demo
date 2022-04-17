using RabbitMQ.Client;
using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RabbitMQProducer
{
    class Program
    {
        private static string[] _args;

        static void Main(string[] args)
        {
            _args = args;
            string name = readArg("-name");
            string host = readArg("-host", "localhost");
            string user = readArg("-user");
            string password = readArg("-password");
            string strPort = readArg("-port", "5672");
            int port = int.Parse(strPort);
            string exchange = readArg("-exchange", "");
            string queue = readArg("-queue", "");

            using (var connection = GetConnection(host, user, password, port))
            {
                using (var channel = connection.CreateModel())
                {
                    Console.WriteLine("Producer starts...");

                    channel.QueueDeclare(queue: queue,
                        arguments: null,
                        exclusive: false,
                        autoDelete: false,
                        durable: false);

                    while (true)
                    {
                        Console.WriteLine("\nEnter message:");
                        string input = Console.ReadLine();

                        if (input == "exit")
                            End();

                        Console.WriteLine("Sending: " + input);

                        string time = DateTime.Now + ": ";
                        string message = time + input;

                        if (name != null)
                        {
                            message = $"{time} {name} send: {input}";
                        }

                        try
                        {
                            Send(exchange, queue, channel, message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Connection error:");
                            Console.Write(e.Message);
                            End();
                        }
                    }
                }
            }

            Console.WriteLine("End");
            Console.ReadKey();
        }

        private static void Send(string exchange, string queue, IModel channel, string input)
        {
            var message = Encoding.UTF8.GetBytes(input);

            channel.BasicPublish(exchange: exchange,
                routingKey: queue,
                basicProperties: null,
                body: message);
        }

        private static void End()
        {
            Console.WriteLine("\nEnd");
            Console.ReadKey();
            Environment.Exit(0);
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
