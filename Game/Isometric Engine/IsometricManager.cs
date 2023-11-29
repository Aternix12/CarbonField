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
        private RenderTarget2D[,] renderTargets;
        private Texture2D[,] textures;
        private int MaxRenderTargetSize = 2000;

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
            PopulateRenderTarget();
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
            Console.WriteLine("Initialising Render Targets");
            Console.WriteLine("-------------------------------");
            int numTargetsX = (int)Math.Ceiling((double)worldWidth / MaxRenderTargetSize);
            int numTargetsY = (int)Math.Ceiling((double)worldHeight / MaxRenderTargetSize);

            renderTargets = new RenderTarget2D[numTargetsX, numTargetsY];
            textures = new Texture2D[numTargetsX, numTargetsY];

            for (int x = 0; x < numTargetsX; x++)
            {
                for (int y = 0; y < numTargetsY; y++)
                {
                    int renderTargetWidth = Math.Min(MaxRenderTargetSize, worldWidth - x * MaxRenderTargetSize);
                    int renderTargetHeight = Math.Min(MaxRenderTargetSize, worldHeight - y * MaxRenderTargetSize);
                    renderTargets[x, y] = new RenderTarget2D(graphicsDevice, renderTargetWidth, renderTargetHeight, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
                }
            }
        }

        public void PopulateRenderTarget()
        {
            Console.WriteLine("Populating Render Targets");
            Console.WriteLine("-------------------------------");
            int numTargetsX = renderTargets.GetLength(0);
            int numTargetsY = renderTargets.GetLength(1);

            for (int x = 0; x < numTargetsX; x++)
            {
                for (int y = 0; y < numTargetsY; y++)
                {
                    RenderTarget2D renderTarget = renderTargets[x, y];
                    graphicsDevice.SetRenderTarget(renderTarget);
                    graphicsDevice.Clear(Color.Transparent);

                    var spriteBatch = new SpriteBatch(graphicsDevice);
                    spriteBatch.Begin();

                    int offsetX = x * MaxRenderTargetSize;
                    int offsetY = y * MaxRenderTargetSize;

                    for (int tileY = 0; tileY < height; tileY++)
                    {
                        for (int tileX = 0; tileX < width; tileX++)
                        {
                            Tile tile = tileMap[tileX, tileY];
                            if (IsTileWithinRenderTarget(tile, offsetX, offsetY, renderTarget.Width, renderTarget.Height))
                            {
                                Vector2 adjustedPosition = new Vector2(tile.Position.X - offsetX, tile.Position.Y - offsetY);
                                tile.Draw(spriteBatch, adjustedPosition);
                            }
                        }
                    }

                    spriteBatch.End();

                    // Create a texture and copy the render target's content into it
                    Texture2D texture = new Texture2D(graphicsDevice, renderTarget.Width, renderTarget.Height);
                    Color[] renderTargetData = new Color[renderTarget.Width * renderTarget.Height];
                    renderTarget.GetData(renderTargetData);
                    texture.SetData(renderTargetData);
                    textures[x, y] = texture;

                    Console.WriteLine($"RenderTarget[{x},{y}] - Offset: ({offsetX}, {offsetY})");

                    // Unload render target to save GPU memory
                    renderTarget.Dispose();
                }
            }

            graphicsDevice.SetRenderTarget(null);
        }

        private void StoreRenderTargetContent()
        {
            for (int x = 0; x < renderTargets.GetLength(0); x++)
            {
                for (int y = 0; y < renderTargets.GetLength(1); y++)
                {
                    if (renderTargets[x, y] != null)
                    {
                        // Create texture and copy content from render target
                        Texture2D texture = new Texture2D(graphicsDevice, renderTargets[x, y].Width, renderTargets[x, y].Height);
                        Color[] renderTargetData = new Color[renderTargets[x, y].Width * renderTargets[x, y].Height];
                        renderTargets[x, y].GetData(renderTargetData);
                        texture.SetData(renderTargetData);
                        textures[x, y] = texture;

                        // Dispose render target to free GPU memory
                        renderTargets[x, y].Dispose();
                        renderTargets[x, y] = null;
                    }
                }
            }
        }

        public void RestoreRenderTargetContent()
        {
            for (int x = 0; x < textures.GetLength(0); x++)
            {
                for (int y = 0; y < textures.GetLength(1); y++)
                {
                    if (textures[x, y] != null)
                    {
                        // Reinitialize render target
                        int width = textures[x, y].Width;
                        int height = textures[x, y].Height;
                        renderTargets[x, y] = new RenderTarget2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                        // Set the render target and draw the texture onto it
                        graphicsDevice.SetRenderTarget(renderTargets[x, y]);
                        graphicsDevice.Clear(Color.Transparent);
                        var spriteBatch = new SpriteBatch(graphicsDevice);
                        spriteBatch.Begin();
                        spriteBatch.Draw(textures[x, y], new Rectangle(0, 0, width, height), Color.White);
                        spriteBatch.End();

                        // Unset the render target
                        graphicsDevice.SetRenderTarget(null);
                    }
                }
            }
        }

        public void UpdateTile(Tile updatedTile)
        {
            for (int x = 0; x < renderTargets.GetLength(0); x++)
            {
                for (int y = 0; y < renderTargets.GetLength(1); y++)
                {
                    int offsetX = x * MaxRenderTargetSize;
                    int offsetY = y * MaxRenderTargetSize;

                    if (IsTileWithinRenderTarget(updatedTile, offsetX, offsetY, MaxRenderTargetSize, MaxRenderTargetSize))
                    {
                        // Recreate the render target
                        renderTargets[x, y] = new RenderTarget2D(graphicsDevice, MaxRenderTargetSize, MaxRenderTargetSize, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                        // Set the render target
                        graphicsDevice.SetRenderTarget(renderTargets[x, y]);
                        graphicsDevice.Clear(Color.Transparent);
                        var spriteBatch = new SpriteBatch(graphicsDevice);
                        spriteBatch.Begin();

                        // Redraw all tiles within this render target
                        for (int tileY = 0; tileY < height; tileY++)
                        {
                            for (int tileX = 0; tileX < width; tileX++)
                            {
                                Tile tile = tileMap[tileX, tileY];
                                if (IsTileWithinRenderTarget(tile, offsetX, offsetY, MaxRenderTargetSize, MaxRenderTargetSize))
                                {
                                    Vector2 adjustedPosition = new Vector2(tile.Position.X - offsetX, tile.Position.Y - offsetY);
                                    tile.Draw(spriteBatch, adjustedPosition);
                                }
                            }
                        }

                        spriteBatch.End();

                        // Copy the updated render target data to the corresponding texture
                        if (textures[x, y] == null)
                        {
                            textures[x, y] = new Texture2D(graphicsDevice, MaxRenderTargetSize, MaxRenderTargetSize);
                        }

                        Color[] renderTargetData = new Color[MaxRenderTargetSize * MaxRenderTargetSize];
                        renderTargets[x, y].GetData(renderTargetData);
                        textures[x, y].SetData(renderTargetData);

                        // Dispose of the render target
                        graphicsDevice.SetRenderTarget(null);
                        renderTargets[x, y].Dispose();
                        renderTargets[x, y] = null;

                        return; // Render target found and updated
                    }
                }
            }
        }




        private void RestoreRenderTargetContent(int rtIndexX, int rtIndexY)
        {
            if (textures[rtIndexX, rtIndexY] != null)
            {
                // Reinitialize render target
                int width = textures[rtIndexX, rtIndexY].Width;
                int height = textures[rtIndexX, rtIndexY].Height;
                renderTargets[rtIndexX, rtIndexY] = new RenderTarget2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                // Set the render target
                graphicsDevice.SetRenderTarget(renderTargets[rtIndexX, rtIndexY]);
                graphicsDevice.Clear(Color.Transparent);

                // Draw the original texture content
                var spriteBatch = new SpriteBatch(graphicsDevice);
                spriteBatch.Begin();
                spriteBatch.Draw(textures[rtIndexX, rtIndexY], new Rectangle(0, 0, width, height), Color.White);
                spriteBatch.End();

                // Unset the render target
                graphicsDevice.SetRenderTarget(null);
            }
        }






        private bool IsTileWithinRenderTarget(Tile tile, int offsetX, int offsetY, int width, int height)
        {
            //NEEDS TO APPLY DRAW TILES BY TERRAIN FOR PERFORMANCE 
            // Check if the tile's position is within the bounds of the render target segment
            // based on its offsetX and offsetY
            return tile.Position.X >= offsetX - width && tile.Position.X < offsetX + width &&
                   tile.Position.Y >= offsetY - height && tile.Position.Y < offsetY + height;
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
            for (int x = 0; x < textures.GetLength(0); x++)
            {
                for (int y = 0; y < textures.GetLength(1); y++)
                {
                    Texture2D texture = textures[x, y];
                    if (texture != null)
                    {
                        // Calculate the world position for the texture
                        int worldPosX = x * MaxRenderTargetSize;
                        int worldPosY = y * MaxRenderTargetSize;

                        // Create a rectangle representing the position and size of the texture
                        Rectangle textureRectangle = new Rectangle(worldPosX, worldPosY, texture.Width, texture.Height);

                        // Check if the texture rectangle intersects with the visible area
                        if (visibleArea.Intersects(textureRectangle))
                        {
                            // Find the intersection area
                            Rectangle intersection = Rectangle.Intersect(visibleArea, textureRectangle);

                            // Adjust the intersection area to the texture's local coordinates
                            Rectangle sourceRectangle = new Rectangle(intersection.X - worldPosX, intersection.Y - worldPosY, intersection.Width, intersection.Height);

                            // Draw only the intersection part of the texture
                            spriteBatch.Draw(texture, intersection, sourceRectangle, Color.White);
                        }
                    }
                }
            }
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