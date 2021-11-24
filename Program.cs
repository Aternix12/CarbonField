using System;

namespace CarbonField
{
    public static class Program
    {
        public static CarbonField game;
        [STAThread]
        static void Main()
        {
            game = new CarbonField();
            game.Run();
        }
    }
}
