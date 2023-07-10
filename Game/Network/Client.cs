using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonField
{
    class Client
    {
        //LiteNetLib
        readonly EventBasedNetListener listener;
        readonly NetManager client;

        public Client()
        {
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
        }

        public void Initialise()
        {
            client.Start();
            client.Connect("localhost" , 42069 , "SomeConnectionKey" /* text key or NetDataWriter */);
            listener.NetworkReceiveEvent += (fromPeer, dataReader, channelNumber, deliveryMethod) =>
            {
                Console.WriteLine("We got: {0}", dataReader.GetString(100 /* max length of string */));
                dataReader.Recycle();
            };
        }

        public void Update()
        {
            client.PollEvents();
            Thread.Sleep(15);
        }
    }
}
