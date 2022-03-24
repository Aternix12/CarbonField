using System;
using System.Collections.Generic;
using Bindings;

namespace CarbonField
{
    internal class ClientHandleData
    {
        public PacketBuffer buffer = new PacketBuffer();
        private delegate void Packet_(byte[] data);
        private static Dictionary<int, Packet_> Packets;

        public void InitMessages()
        {
            Packets = new Dictionary<int, Packet_>();
            
        }

        public void HandleNetworkMessages(byte[] data)
        {
            int packetNum;
            PacketBuffer buffer = new PacketBuffer();

            buffer.AddBytes(data);  
            packetNum = buffer.GetInteger();
            buffer.Dispose();

            if(Packets.TryGetValue(packetNum, out Packet_ Packet))  //Checks dictionary and see if assigned to data type?
            {
                Packet.Invoke(data);
            }
        }
    }
}
