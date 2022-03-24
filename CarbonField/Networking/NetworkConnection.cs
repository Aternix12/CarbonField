using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Bindings;

namespace CarbonField
{
    internal class NetworkConnection
    {
        private NetClient _client;
        public bool Start()
        {
            var loginInformation = new NetworkLoginInformation()
            {
                Name = "GdayChamp"
            };

            _client = new NetClient(new NetPeerConfiguration("NetworkGame"));

            _client.Start();
            var outmsg = _client.CreateMessage();
            outmsg.Write((byte)PacketType.Login);
            _client.Connect("localhost", 9981, outmsg);
            return EstablishInfo();
        }

        private bool EstablishInfo()
        {
            var time = DateTime.Now;
            NetIncomingMessage inc;
            while (true)
            {
                if(DateTime.Now.Subtract(time).Seconds > 15) //Timeout
                {
                    return false;
                }

                if ((inc = _client.ReadMessage()) == null) continue;

                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        var data = inc.ReadByte();
                        if(data == (byte)PacketType.Login)
                        {
                            var accepted = inc.ReadBoolean();
                            if(accepted)
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                }
            }
        }
    }
}
