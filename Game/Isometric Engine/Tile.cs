using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
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
        public Dictionary<Terrain, SpriteSheet> SpriteSheets { get; private set; }
        public SpriteSheet BlendmapSpriteSheet { get; private set; }

        public Rectangle SourceRectangle { get; private set; }
        public Rectangle BoundingBox { get; private set; }
        public Terrain Terrain { get; private set; }
        private readonly Dictionary<Direction, Terrain?> adjacentTerrainTypes;
        private bool hasOverlay;
        private readonly Dictionary<Direction, String> overlaySource;
        private readonly Dictionary<Direction, String> blendmapSource;

        private readonly int Elevation;
        public Vector2 IsometricPosition { get; private set; }

        public Vector2 Position { get; private set; }
        public Vector2 Center { get; private set; }
        public Vector2 TopCorner { get; private set; }
        public Vector2 LeftCorner { get; private set; }
        public Vector2 RightCorner { get; private set; }
        public Vector2 BottomCorner { get; private set; }
        private readonly int spriteIndexX;
        private readonly int spriteIndexY;
        private RenderTarget2D _outputTexture;

        public int GridX { get; private set; }
        public int GridY { get; private set; }
        readonly List<Texture2D> overlayTextures = [];
        readonly List<Texture2D> blendmapTextures = [];

        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, SpriteSheet blendmapSpriteSheet, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Position = position;
            Terrain = terrain;
            this.SpriteSheets = spriteSheets; //Temporary all spritesheet reference in each tile, will need to be centralized to IsoManager
            this.BlendmapSpriteSheet = blendmapSpriteSheet; //This too!
            string spriteName = $"{terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            SourceRectangle = spriteSheets[Terrain].GetSprite(spriteName);
            GridX = gridX;
            GridY = gridY;
            this.spriteIndexX = spriteIndexX;
            this.spriteIndexY = spriteIndexY;
            this.Elevation = 0;

            adjacentTerrainTypes = new Dictionary<Direction, Terrain?>
                {
                    { Direction.Top, null },
                    { Direction.TopRight, null },
                    { Direction.Right, null },
                    { Direction.BottomRight, null },
                    { Direction.Bottom, null },
                    { Direction.BottomLeft, null },
                    { Direction.Left, null },
                    { Direction.TopLeft, null }
                };

            overlaySource = new Dictionary<Direction, string>
            {
                    { Direction.Top, null },
                    { Direction.TopRight, null },
                    { Direction.Right, null },
                    { Direction.BottomRight, null },
                    { Direction.Bottom, null },
                    { Direction.BottomLeft, null },
                    { Direction.Left, null },
                    { Direction.TopLeft, null }
                };

            blendmapSource = new Dictionary<Direction, string>
            {
                    { Direction.Top, null },
                    { Direction.TopRight, null },
                    { Direction.Right, null },
                    { Direction.BottomRight, null },
                    { Direction.Bottom, null },
                    { Direction.BottomLeft, null },
                    { Direction.Left, null },
                    { Direction.TopLeft, null }
                };

            BoundingBox = new Rectangle((int)position.X, (int)position.Y, Width, Height);
            hasOverlay = false;
        }

        public void DetermineNeighbors(IsometricManager isoManager)
        {
            hasOverlay = false;
            overlayTextures.Clear();
            blendmapTextures.Clear();
            if (_outputTexture != null)
            {
                _outputTexture.Dispose();
                _outputTexture = null;
            }

            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                overlaySource[direction] = null;
                blendmapSource[direction] = null;
            }

            GetNeighborTerrain(isoManager, Direction.Top, 0, -1);
            GetNeighborTerrain(isoManager, Direction.TopRight, 1, -1);
            GetNeighborTerrain(isoManager, Direction.Right, 1, 0);
            GetNeighborTerrain(isoManager, Direction.BottomRight, 1, 1);
            GetNeighborTerrain(isoManager, Direction.Bottom, 0, 1);
            GetNeighborTerrain(isoManager, Direction.BottomLeft, -1, 1);
            GetNeighborTerrain(isoManager, Direction.Left, -1, 0);
            GetNeighborTerrain(isoManager, Direction.TopLeft, -1, -1);

            if (hasOverlay)
            {
                CreateBlendedTexture(isoManager.GraphicsDevice);
            }
        }

        private void GetNeighborTerrain(IsometricManager isoManager, Direction direction, int offsetX, int offsetY)
        {
            var neighborTile = isoManager.GetTileAtGridPosition(GridX + offsetX, GridY + offsetY);
            if (neighborTile != null)
            {
                adjacentTerrainTypes[direction] = neighborTile.Terrain;

                // Determine if the neighbor's terrain has higher precedence
                if (GetTerrainPrecedence(neighborTile.Terrain) > GetTerrainPrecedence(Terrain))
                {
                    if (neighborTile.Terrain == Terrain.Grass)
                    {
                        Console.WriteLine("What the fuck");
                    }
                    SetOverlay(direction, neighborTile.Terrain);
                    hasOverlay = true;
                    //Apply appropriate blendsource to that respective direction
                    //This will eventually be terrain specific i believe
                    string blendSpriteName = GetBlendSpriteName(direction);
                    blendmapSource[direction] = blendSpriteName;
                }
            }
        }

        private string GetBlendSpriteName(Direction direction)
        {
            // Map each Direction to the index in the blendmap spritesheet
            int spriteIndex = direction switch
            {
                Direction.Top => 0,
                Direction.TopRight => 1,
                Direction.Right => 2,
                Direction.BottomRight => 3,
                Direction.Bottom => 4,
                Direction.BottomLeft => 5,
                Direction.Left => 6,
                Direction.TopLeft => 7,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Invalid direction: {direction}")
            };

            // Construct the sprite name using the terrain type and sprite index
            return $"blend_{spriteIndex}";
        }

        public void SetOverlay(Direction direction, Terrain overlayTerrain)
        {
            string overlaySpriteName = $"{overlayTerrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";

            overlaySource[direction] = overlaySpriteName;
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
            SourceRectangle = SpriteSheets[Terrain].GetSprite(spriteName);

            DetermineNeighbors(isometricManager);
            UpdateAdjacentTiles(isometricManager);
        }
        private void UpdateAdjacentTiles(IsometricManager isometricManager)
        {
            //This will need to isolate only the calling tiles direction
            UpdateNeighbor(isometricManager, 0, -1);
            UpdateNeighbor(isometricManager, 1, -1);
            UpdateNeighbor(isometricManager, -1, -1);
            UpdateNeighbor(isometricManager, -1, 1);
            UpdateNeighbor(isometricManager, 1, 1);
            UpdateNeighbor(isometricManager, -1, 0);
            UpdateNeighbor(isometricManager, 1, 0);
            UpdateNeighbor(isometricManager, 0, 1);
        }

        private void UpdateNeighbor(IsometricManager isoManager, int offsetX, int offsetY)
        {
            Tile neighborTile = isoManager.GetTileAtGridPosition(GridX + offsetX, GridY + offsetY);
            if (neighborTile != null)
            {
                neighborTile.DetermineNeighbors(isoManager);
            }
        }

        public bool IsWithinBounds(Rectangle area)
        {
            // Directly compare the boundaries of the bounding boxes
            return BoundingBox.Left < area.Right && BoundingBox.Right > area.Left &&
                   BoundingBox.Top < area.Bottom && BoundingBox.Bottom > area.Top;
        }



        public int GetTerrainPrecedence(Terrain terrain)
        {
            return terrain switch
            {
                Terrain.Grass => 1,
                Terrain.Dirt => 2,
                // Add other terrain types here
                _ => int.MaxValue // Unknown or least precedence
            };
        }


        public void CreateBlendedTexture(GraphicsDevice graphicsDevice)
        {
            foreach (var kvp in overlaySource)
            {

                foreach (var direction in overlaySource.Keys.Where(k => overlaySource[k] != null))
                {
                    if (adjacentTerrainTypes.TryGetValue(direction, out Terrain? value) && value.HasValue)
                    {

                        Terrain adjacentTerrain = value.Value;
                        // Retrieve stored textures instead of creating new ones
                        Texture2D overlayTexture = SpriteSheets[adjacentTerrain].GetSpriteTexture(overlaySource[direction]);
                        Texture2D blendmapTexture = BlendmapSpriteSheet.GetSpriteTexture(blendmapSource[direction]);

                        overlayTextures.Add(overlayTexture);
                        blendmapTextures.Add(blendmapTexture);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSheets[Terrain].Texture, Position, SourceRectangle, Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 adjustedPosition, Effect blendEffect)
        {
            Vector2 scale = new(0.25f, 0.25f);
            spriteBatch.Begin();
            spriteBatch.Draw(SpriteSheets[Terrain].Texture, adjustedPosition, SourceRectangle, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.End();
            for (int i = 0; i < overlayTextures.Count; i++)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, blendEffect, null);
                blendEffect.Parameters["overlayTexture"].SetValue(overlayTextures[i]);
                blendEffect.Parameters["blendMap"].SetValue(blendmapTextures[i]);
                blendEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(overlayTextures[i], adjustedPosition, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.End();
            }
        }

        public void DrawOverlay(SpriteBatch spriteBatch, Effect blendEffect, Matrix camTransform)
        {
            Vector2 scale = new Vector2(0.25f, 0.25f);
            for (int i = 0; i < overlayTextures.Count; i++)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, blendEffect, camTransform);
                blendEffect.Parameters["overlayTexture"].SetValue(overlayTextures[i]);
                blendEffect.Parameters["blendMap"].SetValue(blendmapTextures[i]);
                blendEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(overlayTextures[i], Position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                spriteBatch.End();
            }

        }
    }
}
