using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CarbonField
{
    public class ChunkManager
    {
        private readonly GraphicsDevice graphicsDevice;
        const int MaxRenderTargetSize = 640;
        private readonly int width, height, worldWidth, worldHeight;
        private readonly Tile[,] tileMap;
        private SpriteFont tileCoordinateFont;
        private readonly ContentManager content;
        private Chunk[,] chunks;

        public ChunkManager(int width, int height, int worldWidth, int worldHeight, GraphicsDevice graphicsDevice, Tile[,] tileMap, ContentManager content)
        {
            this.width = width;
            this.height = height;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            this.graphicsDevice = graphicsDevice;
            this.tileMap = tileMap;
            this.content = content;      
        }

        public void LoadContent()
        {
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

                    chunks[x, y] = new Chunk(graphicsDevice, offsetX, offsetY, chunkWidth, chunkHeight);
                    chunks[x, y].PopulateChunk(tileMap, new SpriteBatch(graphicsDevice), offsetX, offsetY);
                }
            }
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

        public void UpdateTile(Tile updatedTile)
        {
            var affectedChunks = GetAffectedChunks(updatedTile);

            foreach (var (chunkX, chunkY) in affectedChunks)
            {
                var spriteBatch = new SpriteBatch(graphicsDevice);
                chunks[chunkX, chunkY].RedrawTile(updatedTile, spriteBatch);
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

        //Will evnetually need to draw tiles by terrain when creating and updating chunks
        /*        private void DrawTilesByTerrain(SpriteBatch spriteBatch, Terrain terrain)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Tile tile = TileMap[x, y];
                            if (tile.Terrain == terrain)
                            {
                                tile.Draw(spriteBatch);
                            }
                        }
                    }
                }*/

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            // Calculate the range of chunk indices that are within the visible area
            int startChunkX = Math.Max(0, visibleArea.Left / MaxRenderTargetSize);
            int endChunkX = Math.Min(chunks.GetLength(0) - 1, visibleArea.Right / MaxRenderTargetSize);
            int startChunkY = Math.Max(0, visibleArea.Top / MaxRenderTargetSize);
            int endChunkY = Math.Min(chunks.GetLength(1) - 1, visibleArea.Bottom / MaxRenderTargetSize);

            // Iterate only through visible chunks
            for (int x = startChunkX; x <= endChunkX; x++)
            {
                for (int y = startChunkY; y <= endChunkY; y++)
                {
                    chunks[x, y].Draw(spriteBatch, visibleArea);
                }
            }
        }
    }
}
