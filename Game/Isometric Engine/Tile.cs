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
        private readonly string spriteName;
        private readonly SpriteSheet spriteSheet;


        public Vector2 Position { get; set; }

        public Tile(Vector2 position, string spriteName, SpriteSheet spriteSheet)
        {
            this.Position = position;
            this.spriteName = spriteName;
            this.spriteSheet = spriteSheet;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteSheet.DrawSprite(spriteBatch, spriteName, Position, Color.White);
        }
    }
}
