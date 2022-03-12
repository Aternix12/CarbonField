using System;
using System.Net.Sockets;
using Bindings;

namespace CarbonField
{
    internal class ClientTCP
    {
        public TcpClient PlayerSocket;
        private static NetworkStream networkStream;
        private ClientHandleData chd;
        private byte[] asyncBuff; //Server to Client
        private bool connecting;
        private bool connected;

        public void ConnectToServer()
        {
            if(PlayerSocket != null)    //Close connection if want to connect again
            {
                if (PlayerSocket.Connected || connected)
                    return;
                PlayerSocket.Close();
                PlayerSocket = null;
            }

            PlayerSocket = new TcpClient();
            chd = new ClientHandleData();
            PlayerSocket.ReceiveBufferSize = 4096;
            PlayerSocket.SendBufferSize = 4096;
            PlayerSocket.NoDelay = false;
            Array.Resize(ref asyncBuff, 8192);
            PlayerSocket.BeginConnect("127.0.0.1", 5555, new AsyncCallback(ConnectCallback), PlayerSocket);
            connecting = true;
        }

        private void ConnectCallback(IAsyncResult ar) //Getting data from server, once connected
        {
            PlayerSocket.EndConnect(ar);
            if(PlayerSocket.Connected == false) //If not connected
            {
                connecting = false;
                connected = false;
                return;
            }
            else
            {   //When we are connected
                PlayerSocket.NoDelay = true;
                networkStream = PlayerSocket.GetStream();   
                networkStream.BeginRead(asyncBuff,0,8192,OnReceive,null); //Starts reading for packages
                connected = true;
                connecting = false;
            }
        }

        private void OnReceive(IAsyncResult ar)    //Recieving Package
        {
            int byteAmt = networkStream.EndRead(ar);    //Once read package, read and then stop
            byte[] myBytes = null;
            Array.Resize(ref myBytes, byteAmt); //Resize array to store data, convert to int to resize
            Buffer.BlockCopy(asyncBuff, 0, myBytes, 0, byteAmt);

            if(byteAmt == 0)    //No package ==BAD==
            {
                //DestroyGame
                return;
            }

            //HandleNetworkPackets
            chd.HandleNetworkMessages(myBytes);
            networkStream.BeginRead(asyncBuff, 0, 8192, OnReceive, null);
        }

        public void SendData(byte[] data)   //Data from client to server
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            networkStream.Write(buffer.ToArray(), 0, buffer.ToArray().Length);
            buffer.Dispose();
        }

        public void SendLogin() //Create a packet to be sent
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddInteger((int)ClientPackets.CLogin);   //Adds identifier
            buffer.AddString("Tristan");
            buffer.AddString("Mitchell");
            SendData(buffer.ToArray()); //Go!
            buffer.Dispose();
        }
    }
}
