using System;
using System.Net.Sockets;


namespace CarbonField
{
    internal class ClientTCP
    {
        public TcpClient PlayerSocket;
        public NetworkStream NetworkworkStream;

        public void ConnectToServer()
        {
            PlayerSocket = new TcpClient();
            PlayerSocket.ReceiveBufferSize = 4096;
            PlayerSocket.SendBufferSize = 4096;
            PlayerSocket.NoDelay = false;
            PlayerSocket.BeginConnect("127.0.0.1", 5555, ConnectCallback, PlayerSocket);


        }

        private void ConnectCallback(IAsyncResult ar)
        {
            //Getting data from server
        }
    }
}
