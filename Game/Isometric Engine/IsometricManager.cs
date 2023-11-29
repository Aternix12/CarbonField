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
        private SpriteFont tileCoordinateFont;
        private RenderTarget2D coordinatesRenderTarget;
        private RenderTarget2D tileRenderTarget;
        private GraphicsDevice graphicsDevice;
        private ContentManager content;
        private Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;
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
            CalculateWorldBounds();
        }

        private void CalculateWorldBounds()
        {
            WorldTop = new Vector2(worldWidth / 2, 0);
            WorldRight = new Vector2(0, worldHeight / 2);
            WorldBottom = new Vector2(worldWidth / 2, 0);
            WorldLeft = new Vector2(0, worldHeight / 2);
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

            tileCoordinateFont = content.Load<SpriteFont>("Fonts/Arial");


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

                    Terrain type = (x + y) % 2 == 0 ? Terrain.Grass : Terrain.Dirt;

                    // Correctly passing the terrainSpriteSheets dictionary
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, type, terrainSpriteSheets, spriteIndexX, spriteIndexY, x, y);
                }
            }

            foreach (var tile in tileMap)
            {
                tile.DetermineNeighbors(this);
            }

            // Initialize the render target
            InitializeRenderTarget();
            UpdateRenderTarget();
        }

        public void CreateCoordinatesRenderTarget()
        {
            // Adjust the size of the render target to cover the entire isometric grid
            int renderTargetWidth = (width + height) * Tile.Width / 2;
            int renderTargetHeight = (width + height) * Tile.Height / 2;

            coordinatesRenderTarget = new RenderTarget2D(graphicsDevice, renderTargetWidth, renderTargetHeight);
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
        }

        private void InitializeRenderTarget()
        {
            int renderTargetWidth = (width + height) * Tile.Width / 2;
            int renderTargetHeight = (width + height) * Tile.Height / 2;
            tileRenderTarget = new RenderTarget2D(graphicsDevice,
                            renderTargetWidth,
                            renderTargetHeight,
                            false,
                            graphicsDevice.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
        }


        public void UpdateRenderTarget()
        {
            graphicsDevice.SetRenderTarget(tileRenderTarget);
            graphicsDevice.Clear(Color.Transparent); // Clear with a transparent color

            var spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();

            // Draw tiles here
            DrawTilesByTerrain(spriteBatch, Terrain.Grass);
            DrawTilesByTerrain(spriteBatch, Terrain.Dirt);

            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null); // Reset to default render target
        }


        public Tile GetTileAtGridPosition(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return tileMap[x, y];
            }
            return null;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            // Adjust the visible area to ensure it doesn't extend beyond the render target bounds
            visibleArea.X = Math.Max(visibleArea.X, 0);
            visibleArea.Y = Math.Max(visibleArea.Y, 0);
            visibleArea.Width = Math.Min(visibleArea.Width, tileRenderTarget.Width - visibleArea.X);
            visibleArea.Height = Math.Min(visibleArea.Height, tileRenderTarget.Height - visibleArea.Y);

            // Draw only the visible part of the render target
            spriteBatch.Draw(tileRenderTarget, new Vector2(visibleArea.X, visibleArea.Y), visibleArea, Color.White);

            //spriteBatch.Draw(coordinatesRenderTarget, new Vector2(visibleArea.X, visibleArea.Y), visibleArea, Color.White);
        }

        private void DrawTilesByTerrain(SpriteBatch spriteBatch, Terrain terrain)
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
        }
        public Tile[,] TileMap => tileMap;
    }
}