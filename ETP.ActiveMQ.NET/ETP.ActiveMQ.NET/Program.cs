using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ETP.ActiveMQ.NET
{
    class Program
    {
        private static readonly String ACTIVEMQ_URL = "activemq:tcp://etp.sm-soft.ru:61616";
        private static readonly String TEST_QUEUE_NAME = "TEST.QUEUE";
        private static readonly String TEST_TOPIC_NAME = "TEST.TOPIC";
        private static AutoResetEvent semaphore = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            try
            {
                SendMessage();
                ReadMessage();
                ReadAsyncMessage();
                SendMessage();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} Exception caught.", ex);
            }
        }

        static void SendMessage()
        {
            Uri connectUri = new Uri(ACTIVEMQ_URL);
            Console.WriteLine("Connection URL: " + connectUri);
            IConnectionFactory factory = new NMSConnectionFactory(connectUri);
            using (IConnection connection = factory.CreateConnection())
            {
                using (ISession session = connection.CreateSession())
                {
                    connection.Start();
                    IQueue queue = session.GetQueue(TEST_QUEUE_NAME);
                    using (IMessageProducer producer = session.CreateProducer(queue))
                    {
                        Console.WriteLine("Send message to queue: " + queue);
                        producer.DeliveryMode = MsgDeliveryMode.Persistent;
                        ITextMessage textMessage = session.CreateTextMessage("This queue test message");
                        textMessage.Properties["myProperty"] = "MyPropertyText";
                        textMessage.NMSCorrelationID = "correlationId";
                        producer.Send(textMessage);
                    }

                    ITopic topic = session.GetTopic(TEST_TOPIC_NAME);
                    using (IMessageProducer producer = session.CreateProducer(topic))
                    {
                        Console.WriteLine("Send message to topic: " + topic);
                        producer.DeliveryMode = MsgDeliveryMode.Persistent;
                        ITextMessage textMessage = session.CreateTextMessage("This topic test message");
                        textMessage.Properties["myProperty"] = "MyPropertyText";
                        textMessage.NMSCorrelationID = "correlationId";
                        producer.Send(textMessage);
                    }
                }
            }
        }

        static void ReadMessage()
        {
            Uri connectUri = new Uri(ACTIVEMQ_URL);
            Console.WriteLine("Connection URL: " + connectUri);
            IConnectionFactory factory = new NMSConnectionFactory(connectUri);
            using (IConnection connection = factory.CreateConnection())
            {
                using (ISession session = connection.CreateSession())
                {
                    connection.Start();
                    IQueue queue = session.GetQueue(TEST_QUEUE_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(queue)) //Topic similarly 
                    {
                        Console.WriteLine("Read message from queue: " + queue);
                        ITextMessage message = consumer.Receive() as ITextMessage;
                        if (message == null)
                        {
                            Console.WriteLine("No message received!");
                        }
                        else
                        {
                            Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                            Console.WriteLine("Received message with text: " + message.Text);
                        }
                    }

                }
            }
        }

        static void ReadAsyncMessage()
        {
            Uri connectUri = new Uri(ACTIVEMQ_URL);
            Console.WriteLine("Connection URL: " + connectUri);
            IConnectionFactory factory = new NMSConnectionFactory(connectUri);
            using (IConnection connection = factory.CreateConnection())
            {
                using (ISession session = connection.CreateSession())
                {
                    connection.Start();
                    ITopic topic = session.GetTopic(TEST_TOPIC_NAME);
                    using (IMessageConsumer consumer = session.CreateConsumer(topic)) //Topic similarly 
                    {
                        Console.WriteLine("Read message from topic: " + topic);
                        consumer.Listener += Consumer_Listener;
                        SendMessage();
                    }

                }
            }
        }

        private static void Consumer_Listener(IMessage message)
        {
            ITextMessage textMessage = message as ITextMessage;
            if (message == null)
            {
                Console.WriteLine("Message is null");
            }
            else
            {
                Console.WriteLine("Received message with ID:   " + textMessage.NMSMessageId);
                Console.WriteLine("Received message with text: " + textMessage.Text);
            }
            //semaphore.Set();
        }
    }
}
