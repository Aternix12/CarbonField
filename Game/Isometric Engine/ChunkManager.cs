using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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

                    Rectangle chunkBounds = new Rectangle(offsetX, offsetY, chunkWidth, chunkHeight);

                    // Check if there are any tiles within the chunk bounds
                    if (GetTilesInBounds(chunkBounds).Any())
                    {
                        chunks[x, y] = new Chunk(graphicsDevice, offsetX, offsetY, chunkWidth, chunkHeight);
                        chunks[x, y].PopulateChunk(new SpriteBatch(graphicsDevice), offsetX, offsetY, this);
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

            // Iterate only through chunks that have been initialized
            for (int x = startChunkX; x <= endChunkX; x++)
            {
                for (int y = startChunkY; y <= endChunkY; y++)
                {
                    // Check if the chunk has been initialized before drawing
                    if (chunks[x, y] != null)
                    {
                        chunks[x, y].Draw(spriteBatch, visibleArea);
                    }
                }
            }
        }

    }
}
