using System;
using System.Collections;
using System.Threading;
using System.Transactions;

using IBM.WMQ;
using System.Text;

namespace etp
{
    class SimpleClientAutoReconnectGet
    {
        private String queueManagerName = "GU01QM";
        private String connectionNameList = "etp3.sm-soft.ru(2424),etp4.sm-soft.ru(2424)";
        // Sample channel name
        // TODO: change for customer
        private String channelName = "CLNT.SAMPLE.SVRCONN";
        /// Name of the Queue.
        /// TODO: change for customer
        private String destination = "SAMPLE.APPLICATION_INC";

        /// The reconnect option. Referred from IBM.WMQ.MQC.
        /// 
        /// 0 - MQC.WQCNO_CLIENT_RECONNECT_DISABLED
        /// 1 - MQC.WQCNO_CLIENT_RECONNECT 
        /// 2 - MQC.WQCNO_CLIENT_RECONNECT_Q_MGR (default value)
        /// 3 - MQC.WQCNO_CLIENT_RECONNECT_AS_DEF
         private int reconnectOption = MQC.MQCNO_RECONNECT_Q_MGR; //(default value);
        /// Variables
        private MQQueueManager queueManager;
        private MQQueue queue;
        private MQQueue errorQueue;

        private Hashtable properties;
        private MQMessage message;
        private MQGetMessageOptions getMessageOptions;
        private MQPutMessageOptions putMessageOptions;

        // Message data
        byte[] data;
        String str;

        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Start of SimpleClientAutoReconnectGet Application\n");
            try
            {
                SimpleClientAutoReconnectGet SimpleClientAutoReconnectGet = new SimpleClientAutoReconnectGet();
                SimpleClientAutoReconnectGet.GetMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex);
                Console.WriteLine("Sample execution FAILED!");
                Console.ReadLine();
            }

            Console.WriteLine("\nEnd of SimpleClientAutoReconnectGet Application\n");
        }

        /// <summary>
        /// Get messages
        /// </summary>
        void GetMessages()
        {
            try
            {
               // TransactionScope ts;

                // create connection
                Console.Write("Connecting to queue manager.. ");

                // mq properties
                properties = new Hashtable();
                properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                properties.Add(MQC.CONNECT_OPTIONS_PROPERTY, reconnectOption);
                properties.Add(MQC.CONNECTION_NAME_PROPERTY, connectionNameList);
                properties.Add(MQC.CHANNEL_PROPERTY, channelName);

                // display all details
                Console.WriteLine("MQ Parameters");
                Console.Write("1) destinationURI = ");
                Console.WriteLine(destination);
                Console.WriteLine("2) connectionNameList = " + connectionNameList);
                Console.WriteLine("3) reconnectOption = " + reconnectOption);
                Console.WriteLine("4) channel = " + channelName);
                Console.WriteLine("5) queueManagerName = " + queueManagerName);

                queueManager = new MQQueueManager(queueManagerName, properties);

                Console.WriteLine("Accessing queue " + destination + ".. ");
                queue = queueManager.AccessQueue(destination, MQC.MQOO_INPUT_AS_Q_DEF + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE);
                Console.WriteLine("Current queue depth = " +  queue.CurrentDepth);

                //queue.BackoutRequeueName
                errorQueue = queueManager.AccessQueue(queue.BackoutRequeueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);

                //create GetMessageOptions object
                getMessageOptions = new MQGetMessageOptions();
                getMessageOptions.Options += MQC.MQGMO_WAIT + MQC.MQGMO_SYNCPOINT;
                getMessageOptions.WaitInterval = 5000;  // 20 seconds wait


                // create PutMessageOptions object
                // putMessageOptions = new MQPutMessageOptions();

                
                // getting messages continuously
                int i = 0;
                while (true)
                {
                 //   ts = new TransactionScope();
                //    using (ts)
                 //   {
                    
                    try
                    {

                        // creating a message object
                        message = new MQMessage();
                        queue.Get(message, getMessageOptions);

                        i++;

                        data = message.ReadBytes(message.MessageLength);
                        str = Encoding.UTF8.GetString(data);
                        // Get message properties sample
                        // String ApplicationId = message.GetStringProperty("ApplicationId");
                        DateTime date = message.PutDateTime;
                        Console.WriteLine("Message " + i + " msgId " +  BitConverter.ToString(message.MQMD.MsgId).Replace("-","") + "  got = " + data + " date = " + date /*+ " ApplicationId = " + ApplicationId*/);

                        // Sample of error check
                        if (str.StartsWith("ошибка"))
                        {
                            errorQueue.Put(message/*, putMessageOptions*/);
                            Console.WriteLine("Message " + i + " msgId " + BitConverter.ToString(message.MQMD.MsgId).Replace("-", "") + " put to backout queue " + errorQueue.Name);
                        }

                        if (str.StartsWith("откат"))
                        {
                            throw new Exception("Откат сообщения");
                        }


                        message.ClearMessage();
                        queueManager.Commit();
                       // ts.Complete();             
                    }

                    catch (MQException mqe)
                    {
                        if (mqe.ReasonCode == 2033)
                        {
                            Console.WriteLine("No message available " + DateTime.Now);
                          //  ts.Complete();             
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message);
                            Console.WriteLine("QManager backout");
                            queueManager.Backout();
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("User exception caught " + e.Message);
                        queueManager.Backout();
                    //    ts.Dispose();
                        break;

                    }

                  //  }
                 //   ts.Dispose();
                }

                // closing destination
                Console.Write("Closing queue " + destination + ".. ");
                queue.Close();
                Console.WriteLine("done");

                // Disconnecting queue manager
                Console.Write("Disconnecting queue manager.. ");
                queueManager.Disconnect();
                Console.WriteLine("done");
                Console.ReadLine();
            }

            catch (MQException mqe)
            {
                Console.WriteLine("");
                Console.WriteLine("MQException caught: {0} - {1}", mqe.ReasonCode, mqe.Message);
                Console.WriteLine(mqe.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
