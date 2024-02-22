using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private Chunk[] previousVisibleChunks = new Chunk[64];
        private Chunk[] newVisibleChunks = new Chunk[64];
        private Chunk[] visibleChunks = new Chunk[64];
        private int previousVisibleChunkCount = 0;
        private int newVisibleChunkCount = 0;
        private readonly RenderTarget2D[] renderTargets = new RenderTarget2D[64];
        private List<int> availableRenderTargetIndexes = Enumerable.Range(0, 64).ToList();
        private SpriteBatch spriteBatch;

        //Camera Culling
        private const int CameraMoveThreshold = 50;
        private Rectangle lastVisibleArea;
        private Rectangle expandedViewArea;

        public void LoadContent()
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            tileCoordinateFont = content.Load<SpriteFont>("Fonts/Arial");
            //CreateCoordinatesRenderTarget();
            InitializeChunks();
        }

        private void InitializeChunks()
        {
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
                chunks[chunkX, chunkY].RedrawTile(updatedTile, spriteBatch, blendEffect, this);
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
            expandedViewArea = new Rectangle(
                cameraViewArea.X - 100,
                cameraViewArea.Y - 100,
                cameraViewArea.Width + 200,
                cameraViewArea.Height + 200
            );

            List<Chunk> previouslyVisibleChunksList = previousVisibleChunks.Where(c => c != null).ToList();
            List<Chunk> newlyVisibleChunksList = [];

            // Reset the counter
            newVisibleChunkCount = 0;


            // Assuming each chunk has a fixed size (MaxRenderTargetSize), and the starting point of the chunks grid is (0,0).
            int startXIndex = Math.Max(0, expandedViewArea.Left / MaxRenderTargetSize);
            int endXIndex = Math.Min(chunks.GetLength(0), (expandedViewArea.Right / MaxRenderTargetSize) + 1); // +1 to include the edge chunk

            int startYIndex = Math.Max(0, expandedViewArea.Top / MaxRenderTargetSize);
            int endYIndex = Math.Min(chunks.GetLength(1), (expandedViewArea.Bottom / MaxRenderTargetSize) + 1);


            // Identify new visible chunks
            for (int x = startXIndex; x < endXIndex; x++)
            {
                for (int y = startYIndex; y < endYIndex; y++)
                {
                    if (chunks[x, y] != null)
                    {
                        newlyVisibleChunksList.Add(chunks[x, y]);
                        if (!previouslyVisibleChunksList.Contains(chunks[x, y]))
                        {
                            // This is a newly visible chunk, assign a render target index if available
                            if (availableRenderTargetIndexes.Count > 0)
                            {
                                int index = availableRenderTargetIndexes[0];
                                availableRenderTargetIndexes.RemoveAt(0);
                                chunks[x, y].RenderTargetIndex = index;
                                // Assuming CreateRenderTarget initializes the render target
                                chunks[x, y].CreateRenderTarget(spriteBatch, this, blendEffect);
                            }
                        }
                        newVisibleChunks[newVisibleChunkCount++] = chunks[x, y];
                    }
                }
            }

            // Identify chunks that are no longer visible
            foreach (Chunk oldChunk in previouslyVisibleChunksList)
            {
                if (!newlyVisibleChunksList.Contains(oldChunk) && oldChunk.RenderTargetIndex != -1)
                {
                    availableRenderTargetIndexes.Add(oldChunk.RenderTargetIndex);
                    oldChunk.RenderTargetIndex = -1; // Mark as no longer having a render target
                }
            }

            // Update the visibleChunks array for the next update
            Array.Clear(visibleChunks, 0, visibleChunks.Length);
            Array.Copy(newVisibleChunks, visibleChunks, newVisibleChunkCount);

            // Prepare previousVisibleChunks for the next cycle
            Array.Clear(previousVisibleChunks, 0, previousVisibleChunks.Length);
            Array.Copy(newVisibleChunks, previousVisibleChunks, newVisibleChunkCount);
        }


        public RenderTarget2D GetRenderTargetByIndex(int index)
        {
            if (index < 0 || index >= renderTargets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return renderTargets[index];
        }

        public void SetRenderTargetAtIndex(int index, RenderTarget2D renderTarget)
        {
            if (index >= 0 && index < renderTargets.Length)
            {
                renderTargets[index] = renderTarget;
            }
            else
            {
                ConsoleLogger.Log($"Index {index} is out of bounds for renderTargets array.", ConsoleColor.Red);
            }
        }


        public void Draw(SpriteBatch spriteBatch, Rectangle cameraViewArea, Effect blendEffect)
        {
            // Calculate the movement of the camera since the last update
            int deltaX = Math.Abs(lastVisibleArea.X - cameraViewArea.X);
            int deltaY = Math.Abs(lastVisibleArea.Y - cameraViewArea.Y);

            // Check if the camera has moved significantly
            if (deltaX > CameraMoveThreshold || deltaY > CameraMoveThreshold)
            {
                UpdateVisibleChunks(cameraViewArea, blendEffect);
                lastVisibleArea = cameraViewArea;
            }

            // Draw visible chunks
            foreach (Chunk chunk in visibleChunks)
            {
                if (chunk != null)
                {
                    chunk.Draw(spriteBatch, cameraViewArea, this);
                }
            }
        }
    }
}
