﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Linq;

namespace CarbonField
{
    public class Chunk
    {
        public Rectangle Bounds { get; private set; }

        private readonly GraphicsDevice graphicsDevice;
        private int x;
        private int y;
        private Vector2 adjustedPosition;
        public int RenderTargetIndex { get; set; } = -1;
        private Tile[] tilesByTerrain;

        public Chunk(GraphicsDevice graphicsDevice, int x, int y, int width, int height, ChunkManager chunkManager)
        {
            this.graphicsDevice = graphicsDevice;
            Bounds = new Rectangle(x, y, width, height);
            this.x = x;
            this.y = y;
            tilesByTerrain = chunkManager.GetTilesInBounds(new Rectangle(x, y, Bounds.Width, Bounds.Height)).ToArray();
        }

        public void CreateRenderTarget(SpriteBatch spriteBatch, ChunkManager chunkManager, Effect blendEffect)
        {

            ConsoleLogger.Log($"Populating Chunk: {x}, {y}", ConsoleColor.Green);
            
            if (chunkManager.GetRenderTargetByIndex(RenderTargetIndex) == null)
            {
                RenderTarget2D newRenderTarget = new RenderTarget2D(graphicsDevice, Bounds.Width, Bounds.Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
                chunkManager.SetRenderTargetAtIndex(RenderTargetIndex, newRenderTarget);
                ConsoleLogger.Log($"Creating new rendertarget! {x}, {y}", ConsoleColor.Yellow);
            }

            RenderTarget2D renderTarget = chunkManager.GetRenderTargetByIndex(RenderTargetIndex);
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Red);

            spriteBatch.Begin();
            foreach (var tile in tilesByTerrain)
            {
                adjustedPosition = new(tile.Position.X - x, tile.Position.Y - y);
                tile.Draw(spriteBatch, adjustedPosition, blendEffect);
            }
            spriteBatch.End();

            foreach (var tile in tilesByTerrain)
            {
                adjustedPosition = new(tile.Position.X - x, tile.Position.Y - y);
                tile.DrawOverlay(spriteBatch, adjustedPosition, blendEffect);
            }

            graphicsDevice.SetRenderTarget(null);
        }

        public void RedrawTile(Tile tile, SpriteBatch spriteBatch, Effect blendEffect, ChunkManager chunkManager)
        {
            // Create a new render target
            RenderTarget2D newRenderTarget = new(graphicsDevice, Bounds.Width, Bounds.Height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            // Set the new render target
            graphicsDevice.SetRenderTarget(newRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            // Draw the existing texture first
            spriteBatch.Begin();
            spriteBatch.Draw(chunkManager.GetRenderTargetByIndex(RenderTargetIndex), Vector2.Zero, Color.White);

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


        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea, ChunkManager chunkManager)
        {

            // Calculate the intersection area
            Rectangle intersection = Rectangle.Intersect(visibleArea, Bounds);

            // Adjust the intersection area to the chunk's local coordinates
            Rectangle sourceRectangle = new Rectangle(intersection.X - Bounds.X, intersection.Y - Bounds.Y, intersection.Width, intersection.Height);
            if (RenderTargetIndex != -1)
            {
                // Draw only the intersection part of the texture
                spriteBatch.Draw(chunkManager.GetRenderTargetByIndex(RenderTargetIndex), intersection, sourceRectangle, Color.White);
            }
        }
    }
}
