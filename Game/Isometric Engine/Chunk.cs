using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CarbonField
{
    public class Chunk
    {
        public RenderTarget2D RenderTarget { get; private set; }
        public Texture2D Texture { get; private set; }
        public Rectangle Bounds { get; private set; }

        private GraphicsDevice graphicsDevice;

        public Chunk(GraphicsDevice graphicsDevice, int x, int y, int width, int height)
        {
            this.graphicsDevice = graphicsDevice;
            Bounds = new Rectangle(x, y, width, height);
            RenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            Texture = new Texture2D(graphicsDevice, width, height);
        }

        public void PopulateChunk(Tile[,] tileMap, SpriteBatch spriteBatch, int offsetX, int offsetY)
        {
            Console.WriteLine($"Populating Chunk at Offset ({offsetX}, {offsetY}) with Size ({Bounds.Width}, {Bounds.Height})");

            graphicsDevice.SetRenderTarget(RenderTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            for (int tileY = 0; tileY < tileMap.GetLength(1); tileY++)
            {
                for (int tileX = 0; tileX < tileMap.GetLength(0); tileX++)
                {
                    Tile tile = tileMap[tileX, tileY];
                    if (IsTileWithinBounds(tile, offsetX, offsetY))
                    {
                        Vector2 adjustedPosition = new Vector2(tile.Position.X - offsetX, tile.Position.Y - offsetY);
                        tile.Draw(spriteBatch, adjustedPosition);
                    }
                }
            }

            spriteBatch.End();
            Color[] renderTargetData = new Color[RenderTarget.Width * RenderTarget.Height];
            RenderTarget.GetData(renderTargetData);
            Texture.SetData(renderTargetData);

            graphicsDevice.SetRenderTarget(null);
            RenderTarget.Dispose();
        }

        private bool IsTileWithinBounds(Tile tile, int offsetX, int offsetY)
        {
            int marginX = Tile.Width / 2;  // Half of the tile's width
            int marginY = Tile.Height / 2; // Half of the tile's height

            return (tile.Position.X + Tile.Width > offsetX - marginX) &&
                   (tile.Position.X < offsetX + Bounds.Width + marginX) &&
                   (tile.Position.Y + Tile.Height > offsetY - marginY) &&
                   (tile.Position.Y < offsetY + Bounds.Height + marginY);
        }



        public void RedrawTile(Tile tile, SpriteBatch spriteBatch)
        {
            // Create a new render target
            RenderTarget2D newRenderTarget = new RenderTarget2D(graphicsDevice, Bounds.Width, Bounds.Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            // Set the new render target
            graphicsDevice.SetRenderTarget(newRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            // Draw the existing texture first
            spriteBatch.Begin();
            spriteBatch.Draw(Texture, Vector2.Zero, Color.White);

            // Then draw the updated tile
            Vector2 adjustedPosition = new Vector2(tile.Position.X - Bounds.X, tile.Position.Y - Bounds.Y);
            tile.Draw(spriteBatch, adjustedPosition);
            spriteBatch.End();

            // Update the texture with the new render target content
            Color[] renderTargetData = new Color[Bounds.Width * Bounds.Height];
            newRenderTarget.GetData(renderTargetData);
            Texture.SetData(renderTargetData);

            // Clean up
            graphicsDevice.SetRenderTarget(null);
            newRenderTarget.Dispose();
        }


        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {

            // Calculate the intersection area
            Rectangle intersection = Rectangle.Intersect(visibleArea, Bounds);

            // Adjust the intersection area to the chunk's local coordinates
            Rectangle sourceRectangle = new Rectangle(intersection.X - Bounds.X, intersection.Y - Bounds.Y, intersection.Width, intersection.Height);

            // Draw only the intersection part of the texture
            spriteBatch.Draw(Texture, intersection, sourceRectangle, Color.White);
        }
    }
}
