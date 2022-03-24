using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace CarbonFieldServer
{
    class Client //Instance of Player Connection
    {
        public int PlayerIndex;
        public string IP;
        public TcpClient Socket; //Allows connection to server
        private ServerHandleData shd;
        public NetworkStream NetworkStream; //Sends and gets data
        public bool Closing = false;
        public byte[] readBuff; //Where data will be 'saved'

        public Client()
        {
            shd = null;
            IP = "000.000.000.000";
            Socket = null;
        }
        
        public void Start()
        {
            shd = new ServerHandleData();
            Socket.SendBufferSize = 4096;
            Socket.ReceiveBufferSize = 4096;
            NetworkStream = Socket.GetStream();
            Array.Resize(ref readBuff, Socket.ReceiveBufferSize);
            NetworkStream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);

        }

        private void OnReceiveData(IAsyncResult ar) //Asynchronous Client
        {
            try
            {
                int readBytes = NetworkStream.EndRead(ar);
                if(readBytes <= 0)
                {
                    //Closes client for another player
                    CloseSocket(PlayerIndex);
                    return;// Disconnect the player 
                }

                byte[] newBytes = null;
                Array.Resize(ref newBytes, readBytes);
                Buffer.BlockCopy(readBuff, 0, newBytes, 0, readBytes); //Copies specific number of bytes starting at an offset, read Microsoft documentation

                ///Handle Data
                shd.HandleNetworkMessages(PlayerIndex, newBytes);
                NetworkStream.BeginRead(readBuff, 0, Socket.ReceiveBufferSize, OnReceiveData, null);

            }
            catch (Exception ex)
            {
                CloseSocket(PlayerIndex);
            }
        }

        private void CloseSocket(int index)
        {
            Console.WriteLine("Connection from " + IP + " has been terminated.");
            Socket.Close();
            Socket = null;
        }

    }
}
