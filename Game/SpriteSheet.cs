using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class SpriteSheet
    {
        public Texture2D Texture { get; private set; }
        private readonly Dictionary<string, (Rectangle, Texture2D)> _spriteMap;

        public SpriteSheet(Texture2D texture)
        {
            Texture = texture;
            _spriteMap = new Dictionary<string, (Rectangle, Texture2D)>();
        }

        public void AddSprite(string name, int x, int y, int width, int height, GraphicsDevice graphicsDevice)
        {
            Rectangle sourceRect = new Rectangle(x, y, width, height);
            Texture2D spriteTexture = CreateTextureFromRect(graphicsDevice, Texture, sourceRect);
            _spriteMap[name] = (sourceRect, spriteTexture);
        }

        public Rectangle GetSprite(string name)
        {
            if (_spriteMap.TryGetValue(name, out var spriteData))
            {
                return spriteData.Item1; // Return the Rectangle
            }
            throw new ArgumentException($"Sprite not found: {name}", nameof(name));
        }

        public Texture2D GetSpriteTexture(string name)
        {
            if (_spriteMap.TryGetValue(name, out var spriteData))
            {
                return spriteData.Item2; // Return the Texture2D
            }
            throw new ArgumentException($"Sprite not found: {name}", nameof(name));
        }

        private Texture2D CreateTextureFromRect(GraphicsDevice graphicsDevice, Texture2D originalTexture, Rectangle sourceRect)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, sourceRect.Width, sourceRect.Height);
            // Set the new RenderTarget
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);

            // Draw the specific part of the texture
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();
            spriteBatch.Draw(originalTexture, new Rectangle(0, 0, sourceRect.Width, sourceRect.Height), sourceRect, Color.White);
            spriteBatch.End();

            // Reset the render target to the screen
            graphicsDevice.SetRenderTargets(null);

            // Create a new Texture2D and copy the render target's data into it
            Texture2D croppedTexture = new Texture2D(graphicsDevice, sourceRect.Width, sourceRect.Height);
            Color[] data = new Color[sourceRect.Width * sourceRect.Height];
            renderTarget.GetData(data);
            croppedTexture.SetData(data);

            return croppedTexture;
        }
    }

}
