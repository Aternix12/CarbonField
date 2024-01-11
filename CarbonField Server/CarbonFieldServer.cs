using LiteNetLib;
using System;
using LiteNetLib.Utils;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CarbonField_Server
{
    public class CarbonFieldServer
    {
        readonly EventBasedNetListener listener;
        readonly NetManager server;
        private Task consoleTask;
        private bool shouldExit = false;

        //Console
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public CarbonFieldServer()
        {
            AllocConsole();
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
        }

        public void Initialize()
        {
            server.Start(42069);

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < 10 /* max connections */)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("We got connection: {0}", peer.Address); // Show peer ip
                NetDataWriter writer = new();                               // Create writer class
                writer.Put("Hey client!");                                  // Put some string
                peer.Send(writer, DeliveryMethod.ReliableOrdered);          // Send with reliability
            };

            consoleTask = Task.Run(() =>
            {
                while (!shouldExit)
                {
                    var input = Console.ReadLine();
                    if (input?.ToLower() == "exit")
                    {
                        shouldExit = true;
                    }
                }
            });

            Console.WriteLine("Server Initialisation Finished");
        }

        public async Task Run()
        {
            while (!shouldExit)
            {
                server.PollEvents();
                await Task.Delay(15);
            }

            // Wait for consoleTask to finish
            await consoleTask;

            // Remember to properly stop the server when exiting
            server.Stop();
        }
    }
}
