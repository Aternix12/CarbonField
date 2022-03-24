using System;
using System.Collections.Generic;
using Bindings;

namespace CarbonFieldServer
{
    internal class ServerHandleData
    {
        private delegate void Packet_(int index, byte[] data);
        #nullable enable
        private static Dictionary<int, Packet_>? Packets;

        public void InitMessages()
        {
            Console.WriteLine("Init Network Packets");
            Packets = new Dictionary<int, Packet_>();
            Packets.Add((int)ClientPackets.CLogin, HandleLogin); //When we recieve, what do we want to do? Assigning function
        }

        public void HandleNetworkMessages(int index, byte[] data)
        {
            int packetNum;
            PacketBuffer buffer = new PacketBuffer();

            buffer.AddBytes(data);
            packetNum = buffer.GetInteger();
            buffer.Dispose();

            if (Packets.TryGetValue(packetNum, out Packet_ Packet))  //Checks dictionary and see if assigned to data type?
            {
                Packet.Invoke(index, data);
            }
        }

        private void HandleLogin(int index, byte[] data)
        {
            Console.WriteLine("Got Network Message");
        }
    }
}
