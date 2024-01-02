using System;
using System.Runtime;

namespace CarbonField
{
    public static class Program
    {
        private static CarbonField _game;

        public static CarbonField Game { get { return _game; } }

        [STAThread]
        static void Main()
        {
            GCSettings.LatencyMode = GCLatencyMode.Interactive;
            _game = new CarbonField();
            _game.Run();
        }
    }
}
