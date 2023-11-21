﻿using CarbonField.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CarbonField.Game
{
    public class IsometricManager
    {
        private readonly int width;
        private readonly int height;
        private readonly Tile[,] tileMap;
        private SpriteFont tileCoordinateFont;
        private RenderTarget2D coordinatesRenderTarget;
        private RenderTarget2D tileRenderTarget;
        private Vector2 renderTargetPosition;
        private GraphicsDevice graphicsDevice;
        private ContentManager content;
        private Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;
        public Dictionary<Terrain, SpriteSheet> TerrainSpriteSheets => terrainSpriteSheets;

        public IsometricManager(int width, int height, GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.width = width;
            this.height = height;
            tileMap = new Tile[width, height];
            this.graphicsDevice = graphicsDevice;
            this.content = content;
        }

        public void Initialize()
        {

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

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the isometric position
                    Vector2 isoPosition = new Vector2(
                        x * halfTileWidth - y * halfTileWidth,
                        x * halfTileHeight + y * halfTileHeight
                    );

                    Terrain type = (x + y) % 2 == 0 ? Terrain.Grass : Terrain.Dirt;

                    // Correctly passing the terrainSpriteSheets dictionary
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, type, terrainSpriteSheets, spriteIndexX, spriteIndexY, x, y);
                }
            }

            renderTargetPosition = new Vector2(
               -width * Tile.Width / 2, // Half the width of a single column of tiles
               0 // No vertical offset needed
           );

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
                        (x - y) * Tile.Width / 2 + renderTargetWidth / 2, // Centering horizontally
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
            tileRenderTarget = new RenderTarget2D(graphicsDevice,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                graphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
        }

        public void UpdateRenderTarget()
        {
            graphicsDevice.SetRenderTarget(tileRenderTarget);
            graphicsDevice.Clear(Color.Transparent);

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

        public void Draw(SpriteBatch spriteBatch)
        {
            /*DrawTilesByTerrain(spriteBatch, Terrain.Grass);
            DrawTilesByTerrain(spriteBatch, Terrain.Dirt);*/

            //spriteBatch.Draw(coordinatesRenderTarget, renderTargetPosition, Color.White);
            spriteBatch.Draw(tileRenderTarget, Vector2.Zero, Color.White);


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