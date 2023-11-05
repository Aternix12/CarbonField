using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField.Game
{
    public class Tile
    {
        public static readonly int Width = 64;
        public static readonly int Height = 32;  // Height is half of width

        public Vector2 Position { get; set; }

        public Tile(Vector2 position)
        {
            this.Position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw your tile here
        }
    }
}
