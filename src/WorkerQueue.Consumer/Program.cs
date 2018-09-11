using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace WorkerQueue.Consumer
{
    internal class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;

        private const string QueueName = "WorkerQueue_Queue";

        private static void Main(string[] args)
        {
            Receive();
        }

        public static void Receive()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using (_connection = _factory.CreateConnection())
            {
                using (IModel channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(QueueName, true, false, false, null);
                    channel.BasicQos(0, 1, false);

                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        Payment message = (Payment)ea.Body.DeSerialize(typeof(Payment));
                        Console.WriteLine($"----- Payment Processed {message.CardNumber} : {message.AmountToPay}");
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    channel.BasicConsume(QueueName, false, consumer);
                }
            }
        }
    }
}
