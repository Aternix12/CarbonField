using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Rectangle _sourceRectangle;
        public Rectangle BoundingBox { get; private set; }
        public Terrain Terrain { get; private set; }
        private Dictionary<Direction, Terrain?> adjacentTerrainTypes;
        private bool hasOverlay;
        private Dictionary<Direction, Rectangle> overlaySource;

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
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int[] NodePath { get; set; }

        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Position = position;
            Terrain = terrain;
            this.spriteSheets = spriteSheets; //Temporary all spritesheet reference in each tile, will need to be centralized to IsoManager
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
            }
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

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the base terrain texture
            spriteBatch.Draw(spriteSheets[Terrain].Texture, Position, _sourceRectangle, Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f), SpriteEffects.None, 0f);

            if (hasOverlay)
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
            }
        }

    }
}
