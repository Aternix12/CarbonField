using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace CarbonField
{
    public class Tile
    {
        public static readonly int Width = 96;
        public static readonly int Height = 48;
        public Dictionary<Terrain, SpriteSheet> spriteSheets;
        public SpriteSheet blendmapSpriteSheet;
        
        public Rectangle _sourceRectangle;
        public Rectangle BoundingBox { get; private set; }
        public Terrain Terrain { get; private set; }
        private Dictionary<Direction, Terrain?> adjacentTerrainTypes;
        private bool hasOverlay;
        private Dictionary<Direction, Rectangle> overlaySource;
        private Dictionary<Direction, Rectangle> blendmapSource;
        private Effect blendEffect;

        private int Elevation;
        public Vector2 IsometricPosition { get; private set; }

        public Vector2 Position { get; private set; }
        public Vector2 Center { get; private set; }
        public Vector2 TopCorner { get; private set; }
        public Vector2 LeftCorner { get; private set; }
        public Vector2 RightCorner { get; private set; }
        public Vector2 BottomCorner { get; private set; }
        private int spriteIndexX;
        private int spriteIndexY;
        private RenderTarget2D _outputTexture;

        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int[] NodePath { get; set; }

        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, SpriteSheet blendmapSpriteSheet, Effect blendEffect, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Position = position;
            Terrain = terrain;
            this.spriteSheets = spriteSheets; //Temporary all spritesheet reference in each tile, will need to be centralized to IsoManager
            this.blendmapSpriteSheet = blendmapSpriteSheet; //This too!
            this.blendEffect = blendEffect; //This too!!!
            string spriteName = $"{terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            _sourceRectangle = spriteSheets[Terrain].GetSprite(spriteName);
            GridX = gridX;
            GridY = gridY;
            this.spriteIndexX = spriteIndexX;
            this.spriteIndexY = spriteIndexY;
            this.Elevation = 0;

            adjacentTerrainTypes = new Dictionary<Direction, Terrain?>
        {
            { Direction.Top, null },
            { Direction.Left, null },
            { Direction.Right, null },
            { Direction.Bottom, null }
        };

            overlaySource = new Dictionary<Direction, Rectangle>
    {
        { Direction.Top, new Rectangle() },
        { Direction.Left, new Rectangle() },
        { Direction.Right, new Rectangle() },
        { Direction.Bottom, new Rectangle() }
    };

            blendmapSource = new Dictionary<Direction, Rectangle>
    {
        { Direction.Top, new Rectangle() },
        { Direction.Left, new Rectangle() },
        { Direction.Right, new Rectangle() },
        { Direction.Bottom, new Rectangle() }
    };

            BoundingBox = new Rectangle((int)position.X, (int)position.Y, Width, Height);
            hasOverlay = false;
        }

        public void DetermineNeighbors(IsometricManager isoManager)
        {
            hasOverlay = false;
            GetNeighborTerrain(isoManager, Direction.Top, 0, -1);
            GetNeighborTerrain(isoManager, Direction.Left, -1, 0);
            GetNeighborTerrain(isoManager, Direction.Right, 1, 0);
            GetNeighborTerrain(isoManager, Direction.Bottom, 0, 1);
        }

        private void GetNeighborTerrain(IsometricManager isoManager, Direction direction, int offsetX, int offsetY)
        {
            var neighborTile = isoManager.GetTileAtGridPosition(GridX + offsetX, GridY + offsetY);
            if (neighborTile != null && neighborTile.Terrain != this.Terrain)
            {
                adjacentTerrainTypes[direction] = neighborTile.Terrain;
                SetOverlay(direction, neighborTile.Terrain);
                hasOverlay = true;
                string blendSpriteName = GetBlendSpriteName(direction);
                blendmapSource[direction] = blendmapSpriteSheet.GetSprite(blendSpriteName);
            }
        }

        private string GetBlendSpriteName(Direction direction)
        {
            // Map each Direction to the index in the blendmap spritesheet
            int spriteIndex = direction switch
            {
                Direction.Top => 0,
                Direction.Right => 1,
                Direction.Bottom => 2,
                Direction.Left => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Invalid direction: {direction}")
            };

            // Construct the sprite name using the terrain type and sprite index
            return $"blend_{spriteIndex}";
        }

        public void SetOverlay(Direction direction, Terrain overlayTerrain)
        {
            string overlaySpriteName = $"{overlayTerrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            Console.WriteLine($"Overlay SpriteName: {overlaySpriteName}");

            if (!spriteSheets.ContainsKey(overlayTerrain))
            {
                Console.WriteLine($"Warning: SpriteSheet not found for terrain {overlayTerrain}");
                return;
            }

            if (!overlaySource.ContainsKey(direction))
            {
                Console.WriteLine($"Warning: overlaySource not initialized for direction {direction}");
                return;
            }

            overlaySource[direction] = spriteSheets[overlayTerrain].GetSprite(overlaySpriteName);
        }

        public string GetNeighborInfo()
        {
            var info = new StringBuilder();
            info.AppendLine($"Top Neighbor: {adjacentTerrainTypes[Direction.Top]?.ToString() ?? "None"}");
            info.AppendLine($"Left Neighbor: {adjacentTerrainTypes[Direction.Left]?.ToString() ?? "None"}");
            info.AppendLine($"Right Neighbor: {adjacentTerrainTypes[Direction.Right]?.ToString() ?? "None"}");
            info.AppendLine($"Bottom Neighbor: {adjacentTerrainTypes[Direction.Bottom]?.ToString() ?? "None"}");
            return info.ToString();
        }

        public void ToggleTerrain(Dictionary<Terrain, SpriteSheet> terrainSpriteSheets, IsometricManager isometricManager)
        {
            Console.WriteLine($"Toggling terrain at [{GridX},{GridY}]");
            Console.WriteLine($"Current terrain: {Terrain}");
            // Toggle the terrain
            Terrain = Terrain == Terrain.Grass ? Terrain.Dirt : Terrain.Grass;
            Console.WriteLine($"New terrain: {Terrain}");

            // Update the sprite name to use the same remainder
            string spriteName = $"{Terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            Console.WriteLine($"Terrain SpriteIndex X: {spriteIndexX}");
            Console.WriteLine($"Terrain SpriteIndex Y: {spriteIndexY}");
            _sourceRectangle = spriteSheets[Terrain].GetSprite(spriteName);

            DetermineNeighbors(isometricManager);
            foreach (var direction in adjacentTerrainTypes.Keys)
            {
                DetermineNeighbors(isometricManager);
            }   
        }

        public bool IsWithinBounds(Rectangle area)
        {
            // Directly compare the boundaries of the bounding boxes
            return BoundingBox.Left < area.Right && BoundingBox.Right > area.Left &&
                   BoundingBox.Top < area.Bottom && BoundingBox.Bottom > area.Top;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 adjustedPosition)
        {
            spriteBatch.Draw(spriteSheets[Terrain].Texture, adjustedPosition, _sourceRectangle, Color.White);
        }

        public void CreateBlendedTexture(GraphicsDevice graphicsDevice)
        {
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, Width*4, Height*4);
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);

            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);

            // Base layer - no shader needed
            spriteBatch.Begin();
            spriteBatch.Draw(spriteSheets[Terrain].Texture, new Rectangle(0, 0, Width, Height), _sourceRectangle, Color.White);
            spriteBatch.End();

            // Overlay layer - using shader
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, blendEffect);
            foreach (var direction in adjacentTerrainTypes.Keys)
            {
                if (adjacentTerrainTypes[direction].HasValue)
                {
                    Terrain adjacentTerrain = adjacentTerrainTypes[direction].Value;
                    Rectangle sourceRect = overlaySource[direction];
                    Rectangle blendMapRect = blendmapSource[direction];
                    Texture2D overlayTexture = CreateTextureFromSourceRect(graphicsDevice, spriteSheets[adjacentTerrain].Texture, sourceRect);
                    Texture2D blendmapTexture = CreateTextureFromSourceRect(graphicsDevice, blendmapSpriteSheet.Texture, blendMapRect);
                    // Set shader parameters

                    blendEffect.Parameters["overlayTexture"].SetValue(overlayTexture);
                    blendEffect.Parameters["blendMap"].SetValue(blendmapTexture);

                    spriteBatch.Draw(spriteSheets[adjacentTerrain].Texture, new Rectangle(0, 0, Width, Height), sourceRect, Color.White);
                }
            }
            spriteBatch.End();

            graphicsDevice.SetRenderTarget(null);
            _outputTexture = renderTarget;

            spriteBatch.Dispose();
        }

        public Texture2D CreateTextureFromSourceRect(GraphicsDevice graphicsDevice, Texture2D originalTexture, Rectangle sourceRect)
        {
            // Create a new RenderTarget
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, sourceRect.Width, sourceRect.Height);

            // Set the new RenderTarget
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);

            // Draw the specific part of the texture
            SpriteBatch spriteBatch = new SpriteBatch(graphicsDevice);
            spriteBatch.Begin();
            spriteBatch.Draw(originalTexture, new Rectangle(0, 0, sourceRect.Width, sourceRect.Height), sourceRect, Color.White);
            spriteBatch.End();

            // Reset the render target to the screen
            graphicsDevice.SetRenderTarget(null);

            // Create a new Texture2D and copy the render target's data into it
            Texture2D croppedTexture = new Texture2D(graphicsDevice, sourceRect.Width, sourceRect.Height);
            Color[] data = new Color[sourceRect.Width * sourceRect.Height];
            renderTarget.GetData(data);
            croppedTexture.SetData(data);

            // Dispose the render target and sprite batch
            renderTarget.Dispose();
            spriteBatch.Dispose();

            return croppedTexture;
        }




        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the base terrain texture
            spriteBatch.Draw(spriteSheets[Terrain].Texture, Position, _sourceRectangle, Color.LightGray, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);

            /*if (hasOverlay)
            {
                foreach (var direction in adjacentTerrainTypes.Keys)
                {
                    if (adjacentTerrainTypes[direction].HasValue)
                    {
                        Terrain adjacentTerrain = adjacentTerrainTypes[direction].Value;
                        Rectangle sourceRect = overlaySource[direction];
                        spriteBatch.Draw(spriteSheets[adjacentTerrain].Texture, Position, sourceRect, Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);
                    }
                }

                // Draw the blendmaps
                foreach (var direction in blendmapSource.Keys)
                {
                    Rectangle blendMapRect = blendmapSource[direction];
                    spriteBatch.Draw(blendmapSpriteSheet.Texture, Position, blendMapRect, Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);
                }
            }*/

            //Draw the rendertarget if it exists
            if (_outputTexture != null)
            {
                spriteBatch.Draw(_outputTexture, Position, null, Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);
            }
        }

    }
}
