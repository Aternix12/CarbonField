using System;

namespace CarbonField
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new CarbonField())
                game.Run();
        }
    }
}
