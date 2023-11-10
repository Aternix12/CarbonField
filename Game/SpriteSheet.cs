using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField.Game
{
    public class SpriteSheet
    {
        public Texture2D Texture { get; private set; }
        private Dictionary<string, Rectangle> _spriteMap;

        public SpriteSheet(Texture2D texture)
        {
            Texture = texture;
            _spriteMap = new Dictionary<string, Rectangle>();
        }

        public void AddSprite(string name, int x, int y, int width, int height)
        {
            _spriteMap[name] = new Rectangle(x, y, width, height);
        }

        public void DrawSprite(SpriteBatch spriteBatch, string name, Vector2 position, Color color)
        {
            if (_spriteMap.TryGetValue(name, out Rectangle sourceRectangle))
            {
                spriteBatch.Draw(Texture, position, sourceRectangle, color);
            }
        }
    }

}
