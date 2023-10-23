using System;

namespace CarbonField
{
    public static class Program
    {
        private static CarbonField _game;

        public static CarbonField Game { get { return _game; } }

        [STAThread]
        static void Main()
        {
            _game = new CarbonField();
            _game.Run();
        }
    }
}
