using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using IBM.WMQ;

namespace etp
{
    class SimpleClientAutoReconnectPut
    {
        private String queueManagerName = "GU01QM";
        private String connectionNameList = "etp3.sm-soft.ru(2424),etp4.sm-soft.ru(2424)";
        /// <summary>
        /// The reconnect option. Referred from IBM.WMQ.MQC.
        /// 
        /// 0 - MQC.WMQ_CLIENT_RECONNECT_DISABLED
        /// 1 - MQC.WMQ_CLIENT_RECONNECT 
        /// 2 - MQC.WMQ_CLIENT_RECONNECT_Q_MGR (default value)
        /// 3 - MQC.WMQ_CLIENT_RECONNECT_AS_DEF
        /// </summary>
        private int reconnectOption = 2; //(default value);
        private String channelName = "ERP.GIN.SVRCONN";
        private String destination = "ERP.GIN.MSG_OUT";
        /// <summary>
        /// Name of the Queue.
        /// </summary>
        /// <summary>
        /// Variables
        ///</summary>
        private MQQueueManager queueManager;
        private MQQueue queue;

        private Hashtable properties;
        private MQMessage message;
        private MQPutMessageOptions putMessageOptions;



        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Start of SimpleClientAutoReconnectPut Application\n");
            try
            {
                SimpleClientAutoReconnectPut SimpleClientAutoReconnectPut = new SimpleClientAutoReconnectPut();
                SimpleClientAutoReconnectPut.PutMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught: {0}", ex);
                Console.WriteLine("Sample execution FAILED!");
                Console.ReadLine();
            }

            Console.WriteLine("\nEnd of SimpleClientAutoReconnectPut Application\n");
        }

        /// <summary>
        /// Put messages
        /// </summary>
        void PutMessage()
        {
            try
            {
                // create connection
                Console.Write("Connecting to queue manager.. ");

                // mq properties
                properties = new Hashtable();
                properties.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                properties.Add(MQC.CONNECT_OPTIONS_PROPERTY, MQC.MQCNO_RECONNECT);
                properties.Add(MQC.CONNECTION_NAME_PROPERTY, connectionNameList);
                properties.Add(MQC.CHANNEL_PROPERTY, channelName);
               // properties.Add(MQC.USER_ID_PROPERTY, "gin" );
               // properties.Add(MQC.PASSWORD_PROPERTY, "gin");

                // display all details
                Console.WriteLine("MQ Parameters");
                Console.Write("1) destinationURI = ");
                Console.WriteLine(destination);
                Console.WriteLine("2) connectionNameList = " + connectionNameList);
                Console.WriteLine("3) reconnectOption = " + reconnectOption);
                Console.WriteLine("4) channel = " + channelName);
                Console.WriteLine("5) queueManagerName = " + queueManagerName);

                queueManager = new MQQueueManager(queueManagerName, properties);

                Console.Write("Accessing queue " + destination + ".. ");
                queue = queueManager.AccessQueue(destination,  MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);
                // create PutMessageOptions object
                putMessageOptions = new MQPutMessageOptions();

                // Message data
                String data = "Тест " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now);

                message = new MQMessage();
                message.CharacterSet = 1208;
                message.WriteUTF(data);

                queue.Put(message,putMessageOptions);

                message.ClearMessage();

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

            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine(e.StackTrace);
                Console.ReadLine();
            }
        }

    }
}
