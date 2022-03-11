using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bindings;

namespace CarbonFieldServer
{
    internal class General
    {
        private ServerTCP stcp;
        public void InitServer()
        {
            stcp = new ServerTCP();
            for(int i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                ServerTCP.Clients[i] = new Client();
            }
            stcp.InitNetwork();
            Console.WriteLine("Server started!");
        }
    }
}
