using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace CarbonField
{
    public class IsometricManager
    {
        public readonly int width;
        private readonly int height;
        public readonly int worldWidth;
        public readonly int worldHeight;
        private readonly Tile[,] tileMap;
        public readonly GraphicsDevice GraphicsDevice;
        private readonly ContentManager content;
        private Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;
        private readonly TerrainManager terrainManager;
        SpriteSheet blendmapSpriteSheet;
        Texture2D pixel;
        private readonly QuadtreeNode quadtreeRoot;
        private Rectangle lastCameraViewArea;
        private readonly Tile[] visibleTileBuffer;
        private int visibleTileCount;
        public float HorizontalOffset { get; private set; }
        private const int CameraMoveThreshold = 50;
        private Effect blendEffect;


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
            this.GraphicsDevice = graphicsDevice;
            this.content = content;
            worldWidth = (width + height) * Tile.Width / 2;
            worldHeight = (width + height) * Tile.Height / 2;
            terrainManager = new TerrainManager();

            pixel = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White
            });
            quadtreeRoot = new QuadtreeNode(new Rectangle(0, 0, worldWidth, worldHeight));
            visibleTileBuffer = new Tile[30000];
            HorizontalOffset = (height - width) * Tile.Width / 2f;
            CalculateWorldBounds();


        }

        private void CalculateWorldBounds()
        {
            // Adjust the world bounds to fit the isometric rectangle
            WorldTop = new Vector2(worldWidth / 2 + HorizontalOffset / 2, 0); // Adjusted Top point
            WorldRight = new Vector2(worldWidth, worldHeight / 2 - HorizontalOffset / 4); // Right center
            WorldBottom = new Vector2(worldWidth / 2 - HorizontalOffset / 2, worldHeight); // Adjusted Bottom point
            WorldLeft = new Vector2(0, worldHeight / 2 + HorizontalOffset / 4); // Left center
            //Verticle offsets are relation between tile width and height
        }


        public void Initialize()
        {
            //Bruh
        }

        public void LoadContent()
        {
            // Load grass terrain spritesheet
            Texture2D grassSheetTexture = content.Load<Texture2D>("sprites/terrain/Grass1/Grass1_atlas");
            SpriteSheet grassSpriteSheet = new(grassSheetTexture);

            // Load dirt terrain spritesheet
            Texture2D dirtSheetTexture = content.Load<Texture2D>("sprites/terrain/Dirt1/Dirt1_atlas");
            SpriteSheet dirtSpriteSheet = new(dirtSheetTexture);

            // Load blendmap texture
            Texture2D blendmapTexture = content.Load<Texture2D>("shaders/blendmaps/terrain_main");
            blendmapSpriteSheet = new SpriteSheet(blendmapTexture);

            blendEffect = content.Load<Effect>("shaders/TerrainBlend");

            // Assuming a 2x2 grid layout for blend images in the blendmap
            int blendImageWidth = 384;
            int blendImageHeight = 192;

            // Add each blend image as a sprite to the sprite sheet
            for (int i = 0; i < 8; i++) // Rows
            {
                int x = i * blendImageWidth;
                int y = 0;
                string blendSpriteName = $"blend_{i}";
                blendmapSpriteSheet.AddSprite(blendSpriteName, x, y, blendImageWidth, blendImageHeight, GraphicsDevice);
            }

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
                    grassSpriteSheet.AddSprite($"grass_{x}_{y}", x * Tile.Width * 4, y * Tile.Height * 4, Tile.Width * 4, Tile.Height * 4, GraphicsDevice);
                    dirtSpriteSheet.AddSprite($"dirt_{x}_{y}", x * Tile.Width * 4, y * Tile.Height * 4, Tile.Width * 4, Tile.Height * 4, GraphicsDevice);
                }
            }

            float halfTileWidth = Tile.Width / 2f;
            float halfTileHeight = Tile.Height / 2f;
            float halfTotalWidth = width * Tile.Width / 2f;


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 isoPosition = new(
            HorizontalOffset + halfTotalWidth + (x * halfTileWidth) - (y * halfTileWidth) - halfTileWidth,
            x * halfTileHeight + y * halfTileHeight
        );

                    // Use TerrainManager to determine the terrain type
                    Terrain terrainType = terrainManager.GetTerrainType(x, y);

                    // Use the determined terrainType instead of random selection
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, terrainType, terrainSpriteSheets, blendmapSpriteSheet, spriteIndexX, spriteIndexY, x, y);
                    quadtreeRoot.AddTile(tileMap[x, y]);
                }
            }

            foreach (var tile in tileMap)
            {
                tile.DetermineNeighbors(this);
            }
        }

        private void UpdateVisibleTiles(Rectangle cameraViewArea)
        {
            // Calculate the movement of the camera since the last update
            int deltaX = Math.Abs(lastCameraViewArea.X - cameraViewArea.X);
            int deltaY = Math.Abs(lastCameraViewArea.Y - cameraViewArea.Y);

            // Check if the camera has moved significantly
            if (deltaX > CameraMoveThreshold || deltaY > CameraMoveThreshold)
            {
                // Expand the camera view area by 100 in each direction
                Rectangle expandedViewArea = new Rectangle(
                    cameraViewArea.X - 100,      // Left
                    cameraViewArea.Y - 100,      // Top
                    cameraViewArea.Width + 200,  // Width
                    cameraViewArea.Height + 200  // Height
                );

                visibleTileCount = 0;
                foreach (var tile in quadtreeRoot.GetTilesInArea(expandedViewArea))  // Use expandedViewArea
                {
                    if (tile.IsWithinBounds(expandedViewArea) && visibleTileCount < visibleTileBuffer.Length)  // Use expandedViewArea
                    {
                        visibleTileBuffer[visibleTileCount++] = tile;
                    }
                }

                // Update the last camera view area after processing
                lastCameraViewArea = cameraViewArea;
            }
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
                pixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                pixel.SetData(new[] { Color.White });
            }

            spriteBatch.Draw(pixel, new Rectangle((int)start.X, (int)start.Y, (int)length, thickness), null, color, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
        }


        public void Draw(SpriteBatch spriteBatch, Rectangle cameraViewArea, Matrix camTransform)
        {
            if (lastCameraViewArea != cameraViewArea)
            {
                UpdateVisibleTiles(cameraViewArea);
            }

            // Scale to fit the size of 96x48
            Vector2 scale = new Vector2(0.25f, 0.25f);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camTransform);
            for (int i = 0; i < visibleTileCount; i++)
            {
                var tile = visibleTileBuffer[i];
                tile.Draw(spriteBatch);
            }

            spriteBatch.End();

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, blendEffect, camTransform);
            for (int i = 0; i < visibleTileCount; i++)
            {

                var tile = visibleTileBuffer[i];
                tile.DrawOverlay(spriteBatch, blendEffect, camTransform);
            }
            //spriteBatch.End();

        }

        public void DrawDiag(SpriteBatch spriteBatch)
        {
            int lineThickness = 2; // Set the desired line thickness

            DrawLine(spriteBatch, WorldTop, WorldRight, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldRight, WorldBottom, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldBottom, WorldLeft, Color.Yellow, lineThickness);
            DrawLine(spriteBatch, WorldLeft, WorldTop, Color.Yellow, lineThickness);

            DrawLine(spriteBatch, Vector2.Zero, new Vector2(worldWidth, 0), Color.Orange, lineThickness);
            DrawLine(spriteBatch, new Vector2(worldWidth, 0), new Vector2(worldWidth, worldHeight), Color.Orange, lineThickness);
            DrawLine(spriteBatch, new Vector2(worldWidth, worldHeight), new Vector2(0, worldHeight), Color.Orange, lineThickness);
            DrawLine(spriteBatch, new Vector2(0, worldHeight), Vector2.Zero, Color.Orange, lineThickness);
        }
    }
}