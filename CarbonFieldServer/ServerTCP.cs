using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Bindings;

namespace CarbonFieldServer
{
    internal class ServerTCP //Initialise network allowing player connection
    { 
        public TcpListener ServerSocket;
        public static Client[] Clients = new Client[Constants.MAX_PLAYERS];

        public void InitNetwork()
        {
            Console.WriteLine("Initialising Server Network...");
            ServerSocket = new TcpListener(IPAddress.Any, 5555);
            ServerSocket.Start();
            ServerSocket.BeginAcceptTcpClient(OnClientConnect, null); //Player connects to server

        }

        private void OnClientConnect(IAsyncResult ar)
        {
            TcpClient client = ServerSocket.EndAcceptTcpClient(ar); //End players begin connection
            client.NoDelay = false;
            ServerSocket.BeginAcceptTcpClient(OnClientConnect, null);

            for(int i = 1; i <= Constants.MAX_PLAYERS; i++) //If client is empty add player
            {
                if(Clients[i].Socket == null) //If no player in client class
                {
                    //Then add to client
                    Clients[i].Socket = client;
                    Clients[i].PlayerIndex = i;
                    //For sending messages and data
                    Clients[i].IP = client.Client.RemoteEndPoint.ToString();
                    Clients[i].Start();
                    Console.WriteLine("Connection recieved from " + Clients[i].IP);
                    return;
                }
            }
        }

    }
}
