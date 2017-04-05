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
        // Sample channel name
        // TODO: change for customer
        private String channelName = "CLNT.SAMPLE.SVRCONN";
        /// Name of the Queue.
        /// TODO: change for customer
        private String destination = "SAMPLE.STATUS_OUT";

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
                properties.Add(MQC.CONNECT_OPTIONS_PROPERTY, reconnectOption);
                properties.Add(MQC.CONNECTION_NAME_PROPERTY, connectionNameList);
                properties.Add(MQC.CHANNEL_PROPERTY, channelName);
               // properties.Add(MQC.USER_ID_PROPERTY, "sample" );
               // properties.Add(MQC.PASSWORD_PROPERTY, "sample");

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
                putMessageOptions.Options = MQC.MQPMO_NEW_MSG_ID;

                // Message data
                String data = "Тест " + string.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);

                message = new MQMessage();
                message.MessageId = MQC.MQMI_NONE;
                message.Format = MQC.MQFMT_STRING;
                message.CharacterSet = MQC.CODESET_UTF;
                byte[] utf16 = Encoding.Unicode.GetBytes(data);
                byte[] utf8 = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16);

                message.Write(utf8);

                // Set property if your need
                message.SetStringProperty("TestProperty","Тестовое значение");

                queue.Put(message,putMessageOptions);
                message.ClearMessage();
                // message.MessageId = MQC.MQMI_NONE;


                // Send second message 
                data = "Тест " + string.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);
                utf16 = Encoding.Unicode.GetBytes(data);
                utf8 = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16);

                message.Write(utf8);
                queue.Put(message, putMessageOptions);
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
