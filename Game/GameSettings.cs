using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class GameSettings
    {
        public int PreferredBackBufferWidth { get; set; } = 2160;
        public int PreferredBackBufferHeight { get; set; } = 1440;
        public bool IsFullScreen { get; set; } = true;
    }

}
