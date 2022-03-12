using System.Threading;
using System;

namespace CarbonFieldServer
{
    class Program
    {
        private static Thread consoleThread;
        private static General general;

        
        static void Main(string[] args)
        {
            general = new General();          
            consoleThread = new Thread(new ThreadStart(ConsoleThread));
            consoleThread.Start();
            general.InitServer();
        }

        static void ConsoleThread()
        {
            Console.ReadLine();
        }
    }
}