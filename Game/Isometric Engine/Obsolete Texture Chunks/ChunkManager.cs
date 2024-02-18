using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarbonField
{
    public class ChunkManager(int width, int height, int worldWidth, int worldHeight, GraphicsDevice graphicsDevice, Tile[,] tileMap, ContentManager content)
    {
        private readonly GraphicsDevice graphicsDevice = graphicsDevice;
        const int MaxRenderTargetSize = 640;
        private readonly int width = width, height = height, worldWidth = worldWidth, worldHeight = worldHeight;
        private readonly Tile[,] tileMap = tileMap;
        private SpriteFont tileCoordinateFont;
        private readonly ContentManager content = content;
        private Chunk[,] chunks;
        private Chunk[] visibleChunks;

        //Camera Culling
        private const int CameraMoveThreshold = 50;
        private Rectangle lastVisibleArea;

        public void LoadContent()
        {
            tileCoordinateFont = content.Load<SpriteFont>("Fonts/Arial");
            //CreateCoordinatesRenderTarget();
            InitializeChunks();
        }

        private void InitializeChunks()
        {
            visibleChunks = new Chunk[64];

            int numTargetsX = (int)Math.Ceiling((double)worldWidth / MaxRenderTargetSize);
            int numTargetsY = (int)Math.Ceiling((double)worldHeight / MaxRenderTargetSize);

            chunks = new Chunk[numTargetsX, numTargetsY];

            for (int x = 0; x < numTargetsX; x++)
            {
                for (int y = 0; y < numTargetsY; y++)
                {
                    int offsetX = x * MaxRenderTargetSize;
                    int offsetY = y * MaxRenderTargetSize;
                    int chunkWidth = Math.Min(MaxRenderTargetSize, worldWidth - x * MaxRenderTargetSize);
                    int chunkHeight = Math.Min(MaxRenderTargetSize, worldHeight - y * MaxRenderTargetSize);

                    Rectangle chunkBounds = new Rectangle(offsetX, offsetY, chunkWidth, chunkHeight);

                    // Check if there are any tiles within the chunk bounds
                    if (GetTilesInBounds(chunkBounds).Any())
                    {
                        chunks[x, y] = new Chunk(graphicsDevice, offsetX, offsetY, chunkWidth, chunkHeight);
                    }
                }
            }
        }

        public IEnumerable<Tile> GetTilesInBounds(Rectangle bounds)
        {
            // Calculate the potential range of tile indices considering isometric layout
            int startX = Math.Max(0, bounds.Left / Tile.Width - height); // height is the Y dimension of the grid
            int endX = Math.Min(tileMap.GetLength(0), bounds.Right / Tile.Width + height);

            int startY = Math.Max(0, bounds.Top / Tile.Height - width); // width is the X dimension of the grid
            int endY = Math.Min(tileMap.GetLength(1), bounds.Bottom / Tile.Height + width);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    // Check if the tile index is valid
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        Tile tile = tileMap[x, y];
                        // Calculate the isometric position for each tile
                        Vector2 isoPosition = GetIsometricPosition(x, y);

                        // Check if the tile's isometric position is within the bounds
                        if (isoPosition.X + Tile.Width > bounds.Left &&
                            isoPosition.X < bounds.Right &&
                            isoPosition.Y + Tile.Height > bounds.Top &&
                            isoPosition.Y < bounds.Bottom)
                        {
                            yield return tile;
                        }
                    }
                }
            }
        }

        private Vector2 GetIsometricPosition(int x, int y)
        {
            float halfTileWidth = Tile.Width / 2f;
            float halfTileHeight = Tile.Height / 2f;
            float halfTotalWidth = width * Tile.Width / 2f;

            // Calculate the isometric position based on grid coordinates
            return new Vector2(
                halfTotalWidth + (x * halfTileWidth) - (y * halfTileWidth) - halfTileWidth,
                x * halfTileHeight + y * halfTileHeight
            );
        }



        private bool IsTileWithinBounds(Tile tile, Rectangle bounds)
        {
            // Here you need to check if the tile's isometric coordinates intersect with the chunk's bounds
            // Again, this is a simplified example
            return tile.Position.X + Tile.Width > bounds.Left &&
                   tile.Position.X < bounds.Right &&
                   tile.Position.Y + Tile.Height > bounds.Top &&
                   tile.Position.Y < bounds.Bottom;
        }




        /*public void CreateCoordinatesRenderTarget() OLD COORDINATES CODE
        {
            // Adjust the size of the render target to cover the entire isometric grid
            int renderTargetWidth = (width + height) * Tile.Width / 2;
            int renderTargetHeight = (width + height) * Tile.Height / 2;

            RenderTarget2D coordinatesRenderTarget = new RenderTarget2D(graphicsDevice, renderTargetWidth, renderTargetHeight);
            graphicsDevice.SetRenderTarget(coordinatesRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            var spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();

            // Adjusting position for isometric tiles
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var text = $"[{x},{y}]";
                    var textSize = tileCoordinateFont.MeasureString(text);

                    // Calculate isometric position
                    Vector2 isoPosition = new Vector2(
                        (x - y) * Tile.Width / 2 + renderTargetWidth / 2 - Tile.Width / 2, // Centering horizontally
                        (x + y) * Tile.Height / 2 // Staggering vertically
                    );

                    // Center the text in the tile
                    Vector2 textPosition = new Vector2(
                        isoPosition.X + (Tile.Width - textSize.X) / 2,
                        isoPosition.Y + (Tile.Height - textSize.Y) / 2 - Tile.Height / 8 // Adjust for isometric height
                    );

                    spriteBatch.DrawString(tileCoordinateFont, text, textPosition, Color.White);
                }
            }

            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
        }*/

        public void UpdateTile(Tile updatedTile, Effect blendEffect)
        {
            var affectedChunks = GetAffectedChunks(updatedTile);

            foreach (var (chunkX, chunkY) in affectedChunks)
            {
                var spriteBatch = new SpriteBatch(graphicsDevice);
                chunks[chunkX, chunkY].RedrawTile(updatedTile, spriteBatch, blendEffect);
            }
        }

        private HashSet<(int, int)> GetAffectedChunks(Tile tile)
        {
            var affectedChunks = new HashSet<(int, int)>();
            var bounds = new Rectangle(
                (int)tile.Position.X,
                (int)tile.Position.Y,
                Tile.Width,
                Tile.Height);

            // Add logic to determine affected chunks based on the tile bounds
            affectedChunks.Add(GetChunkIndex(bounds.Left, bounds.Top));
            affectedChunks.Add(GetChunkIndex(bounds.Right, bounds.Bottom));

            return affectedChunks;
        }

        private (int, int) GetChunkIndex(int x, int y)
        {
            int chunkX = Math.Clamp(x / MaxRenderTargetSize, 0, chunks.GetLength(0) - 1);
            int chunkY = Math.Clamp(y / MaxRenderTargetSize, 0, chunks.GetLength(1) - 1);
            return (chunkX, chunkY);
        }

        private void UpdateVisibleChunks(Rectangle cameraViewArea, Effect blendEffect)
        {
            // Determine new visible chunks
            List<Chunk> newVisibleChunks = new List<Chunk>();
            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    if (chunks[x, y] != null && cameraViewArea.Intersects(chunks[x, y].Bounds))
                    {
                        newVisibleChunks.Add(chunks[x, y]);
                    }
                }
            }

            // Dispose render targets for chunks that are no longer visible
            foreach (Chunk chunk in visibleChunks)
            {
                if (chunk != null && !newVisibleChunks.Contains(chunk))
                {
                    chunk.DisposeRenderTarget();
                }
            }

            // Update the visibleChunks array and create render targets for new visible chunks
            for (int i = 0; i < visibleChunks.Length; i++)
            {
                visibleChunks[i] = (i < newVisibleChunks.Count) ? newVisibleChunks[i] : null;
                if (visibleChunks[i] != null)
                {
                    visibleChunks[i].CreateRenderTarget(new SpriteBatch(graphicsDevice), this, blendEffect);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle cameraViewArea, Matrix camTransform, Effect blendEffect)
        {
            // Check for significant camera movement to update visible chunks
            if (!lastVisibleArea.Equals(cameraViewArea))
            {
                UpdateVisibleChunks(cameraViewArea, blendEffect);
                lastVisibleArea = cameraViewArea;
            }

            // Draw visible chunks
            foreach (Chunk chunk in visibleChunks)
            {
                if (chunk != null)
                {
                    chunk.Draw(spriteBatch, cameraViewArea);
                }
            }
        }
    }
}
