using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Linq;

namespace CarbonField
{
    public class Chunk
    {
        public RenderTarget2D RenderTarget { get; private set; }
        public Rectangle Bounds { get; private set; }

        private readonly GraphicsDevice graphicsDevice;
        private int x;
        private int y;

        public Chunk(GraphicsDevice graphicsDevice, int x, int y, int width, int height)
        {
            this.graphicsDevice = graphicsDevice;
            Bounds = new Rectangle(x, y, width, height);
            this.x = x;
            this.y = y;
            RenderTarget = new RenderTarget2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
        }

        public void CreateRenderTarget(SpriteBatch spriteBatch, ChunkManager chunkManager, Effect blendEffect)
        {
            if (RenderTarget == null)
            {
                RenderTarget = new RenderTarget2D(graphicsDevice, Bounds.Width, Bounds.Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                graphicsDevice.SetRenderTarget(RenderTarget);
                graphicsDevice.Clear(Color.Transparent);

                
                // Group tiles by terrain type
                var tilesByTerrain = chunkManager.GetTilesInBounds(new Rectangle(x, y, Bounds.Width, Bounds.Height))
                .GroupBy(tile => tile.Terrain)
                                                  .ToDictionary(group => group.Key, group => group.ToList());

                // Draw each group of tiles
                foreach (var terrainGroup in tilesByTerrain)
                {
                    foreach (Tile tile in terrainGroup.Value)
                    {
                        Vector2 adjustedPosition = new(tile.Position.X - x, tile.Position.Y - y);
                        tile.Draw(spriteBatch, adjustedPosition, blendEffect);
                    }
                }

                graphicsDevice.SetRenderTarget(null);
            }
        }

        public void DisposeRenderTarget()
        {
            RenderTarget?.Dispose();
            RenderTarget = null;
        }


        public void RedrawTile(Tile tile, SpriteBatch spriteBatch, Effect blendEffect)
        {
            // Create a new render target
            RenderTarget2D newRenderTarget = new RenderTarget2D(graphicsDevice, Bounds.Width, Bounds.Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

            // Set the new render target
            graphicsDevice.SetRenderTarget(newRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            // Draw the existing texture first
            spriteBatch.Begin();
            spriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);

            // Then draw the updated tile
            Vector2 adjustedPosition = new Vector2(tile.Position.X - Bounds.X, tile.Position.Y - Bounds.Y);
            tile.Draw(spriteBatch, adjustedPosition, blendEffect);
            spriteBatch.End();

            // Update the texture with the new render target content
            Color[] renderTargetData = new Color[Bounds.Width * Bounds.Height];
            newRenderTarget.GetData(renderTargetData);

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

            if (RenderTarget != null)
            {
                // Draw only the intersection part of the texture
                spriteBatch.Draw(RenderTarget, intersection, sourceRectangle, Color.White);
            }
        }
    }
}
