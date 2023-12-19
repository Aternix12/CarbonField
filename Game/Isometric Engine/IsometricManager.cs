using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        /*private readonly ChunkManager chunkManager;*/
        private CtrCamera _camera;
        private QuadtreeNode quadtreeRoot;
        Texture2D pixel;

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
            /*chunkManager = new ChunkManager(width, height, worldWidth, worldHeight, graphicsDevice, tileMap, content);*/
            CalculateWorldBounds();
            pixel = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White
            });
            quadtreeRoot = new QuadtreeNode(new Rectangle(0, 0, worldWidth, worldHeight));
        }

        private void CalculateWorldBounds()
        {
            WorldTop = new Vector2(worldWidth / 2, 0); // Top center
            WorldRight = new Vector2(worldWidth, worldHeight / 2); // Right center
            WorldBottom = new Vector2(worldWidth / 2, worldHeight); // Bottom center
            WorldLeft = new Vector2(0, worldHeight / 2); // Left center
        }

        public void SetCamera(CtrCamera camera)
        {
            _camera = camera;
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
                    quadtreeRoot.AddTile(tileMap[x, y]);

                }
            }

            foreach (var tile in tileMap)
            {
                tile.DetermineNeighbors(this);
            }

            // Initialize the render target
            /*chunkManager.LoadContent();*/
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

        public IEnumerable<Tile> GetVisibleTiles(Rectangle viewBounds)
        {
            return quadtreeRoot.GetTilesInArea(viewBounds);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            foreach (Tile tile in GetVisibleTiles(visibleArea))
            {
                tile.Draw(spriteBatch);
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