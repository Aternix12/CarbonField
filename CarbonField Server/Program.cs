using CarbonField_Server;
using System;
using System.Threading.Tasks;

namespace CarbonField_Server
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            CarbonFieldServer carbonFieldServer = new();
            carbonFieldServer.Initialize();
            await carbonFieldServer.Run();
        }
    }
}
