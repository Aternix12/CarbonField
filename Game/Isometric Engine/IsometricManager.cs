using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarbonField
{
    public class IsometricManager
    {
        public readonly int width;
        private readonly int height;
        public readonly int worldWidth;
        public readonly int worldHeight;
        private readonly Tile[,] tileMap;
        private readonly GraphicsDevice graphicsDevice;
        private readonly ContentManager content;
        private Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;
        private readonly TerrainManager terrainManager;
        private TileChunk[,] chunkMap;
        private readonly int chunkWidth = 100;
        private readonly int chunkHeight = 100;
        Texture2D pixel;
        private List<Tile> visibleTilesCache = new List<Tile>();


        public Dictionary<Terrain, SpriteSheet> TerrainSpriteSheets => terrainSpriteSheets;

        public Vector2 WorldTop { get; private set; }
        public Vector2 WorldRight { get; private set; }
        public Vector2 WorldBottom { get; private set; }
        public Vector2 WorldLeft { get; private set; }


        public IsometricManager(int width, int height, GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.width = width;
            this.height = height;
            tileMap = new Tile[width, height];
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            worldWidth = (width + height) * Tile.Width / 2;
            worldHeight = (width + height) * Tile.Height / 2;
            terrainManager = new TerrainManager();
            CalculateWorldBounds();
            pixel = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White
            });
        }



        private void CalculateWorldBounds()
        {
            WorldTop = new Vector2(worldWidth / 2, 0); // Top center
            WorldRight = new Vector2(worldWidth, worldHeight / 2); // Right center
            WorldBottom = new Vector2(worldWidth / 2, worldHeight); // Bottom center
            WorldLeft = new Vector2(0, worldHeight / 2); // Left center
        }


        public void Initialize()
        {
            //Bruh
        }

        public void LoadContent()
        {
            // Load grass terrain spritesheet
            Texture2D grassSheetTexture = content.Load<Texture2D>("sprites/terrain/grass_terrain");
            SpriteSheet grassSpriteSheet = new(grassSheetTexture);

            // Load dirt terrain spritesheet
            Texture2D dirtSheetTexture = content.Load<Texture2D>("sprites/terrain/dirt_terrain");
            SpriteSheet dirtSpriteSheet = new(dirtSheetTexture);

            terrainSpriteSheets = new()
            {
                { Terrain.Grass, grassSpriteSheet },
                { Terrain.Dirt, dirtSpriteSheet }
            };

            // Populate the SpriteSheet with tiles (assuming 10x10 grid of 64x32 tiles)
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    grassSpriteSheet.AddSprite($"grass_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                    dirtSpriteSheet.AddSprite($"dirt_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                }
            }

            float halfTileWidth = Tile.Width / 2f;
            float halfTileHeight = Tile.Height / 2f;
            float halfTotalWidth = width * Tile.Width / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the isometric position
                    Vector2 isoPosition = new Vector2(
                        halfTotalWidth + (x * halfTileWidth) - (y * halfTileWidth) - halfTileWidth,
                        x * halfTileHeight + y * halfTileHeight
                    );

                    // Use TerrainManager to determine the terrain type
                    Terrain terrainType = terrainManager.GetTerrainType(x, y);

                    // Use the determined terrainType instead of random selection
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, terrainType, terrainSpriteSheets, spriteIndexX, spriteIndexY, x, y);
                }
            }

            foreach (var tile in tileMap)
            {
                tile.DetermineNeighbors(this);
            }

            InitializeChunks(chunkWidth, chunkHeight);
        }

        public void InitializeChunks(int chunkWidth, int chunkHeight)
        {
            int numChunksX = (int)Math.Ceiling((double)worldWidth / (chunkWidth * Tile.Width));
            int numChunksY = (int)Math.Ceiling((double)worldHeight / (chunkHeight * Tile.Height));

            chunkMap = new TileChunk[numChunksX, numChunksY];

            for (int x = 0; x < numChunksX; x++)
            {
                for (int y = 0; y < numChunksY; y++)
                {
                    var chunk = new TileChunk(chunkWidth, chunkHeight);
                    int startX = x * chunkWidth * Tile.Width;
                    int startY = y * chunkHeight * Tile.Height;
                    chunk.CalculateBounds(startX, startY, Tile.Width, Tile.Height);

                    foreach (Tile tile in tileMap)
                    {
                        if (IsTileWithinChunkBounds(tile, chunk.Bounds))
                        {
                            // Use tile's global coordinates to determine its position within the chunk
                            int tileX = (int)((tile.Position.X - startX) / Tile.Width);
                            int tileY = (int)((tile.Position.Y - startY) / Tile.Height);
                            chunk.SetTile(tileX, tileY, tile);
                        }
                    }

                    chunkMap[x, y] = chunk;
                }
            }
        }




        private bool IsTileWithinChunkBounds(Tile tile, Rectangle chunkBounds)
        {
            // Calculate the tile's position in the world
            Vector2 worldPosition = tile.Position; // Make sure this is the global position

            // Check if the tile's global position falls within the chunk's bounds
            return worldPosition.X >= chunkBounds.Left && worldPosition.X < chunkBounds.Right &&
                   worldPosition.Y >= chunkBounds.Top && worldPosition.Y < chunkBounds.Bottom;
        }



        public Tile GetTileAtGridPosition(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return tileMap[x, y];
            }
            return null;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            // Create a 1x1 white pixel texture if not already created
            if (pixel == null)
            {
                pixel = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
                pixel.SetData(new[] { Color.White });
            }

            spriteBatch.Draw(pixel, new Rectangle((int)start.X, (int)start.Y, (int)length, thickness), null, color, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
        }




        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            foreach (var chunk in chunkMap)
            {
                if (visibleArea.Intersects(chunk.Bounds)) // Check if the chunk is in the visible area
                {
                    foreach (var tile in chunk.Tiles)
                    {
                        if (tile != null)
                        {
                            tile.Draw(spriteBatch);
                        }
                    }
                }
            }
        }

        public void DrawDiag(SpriteBatch spriteBatch)
        {
            int lineThickness = 2; // Set the desired line thickness

            DrawLine(spriteBatch, WorldTop, WorldRight, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldRight, WorldBottom, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldBottom, WorldLeft, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldLeft, WorldTop, Color.Yellow, lineThickness);
        }


        public Tile[,] TileMap => tileMap;
    }
}