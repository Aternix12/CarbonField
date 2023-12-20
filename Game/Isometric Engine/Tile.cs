using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class Tile
    {
        public static readonly int Width = 64;
        public static readonly int Height = 32;
        public SpriteSheet spriteSheet;
        public Rectangle _sourceRectangle;
        public Terrain Terrain { get; private set; }
        private Dictionary<Direction, Terrain?> adjacentTerrainTypes;
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
        public Tile()
        {
            // Parameterless constructor (leaves the tile in an uninitialized state)
        }

        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Position = position;
            Terrain = terrain;
            this.spriteSheet = spriteSheets[terrain];
            string spriteName = $"{terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            _sourceRectangle = spriteSheet.GetSprite(spriteName);
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
        }

        public void Initialize(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Position = position;
            Terrain = terrain;
            spriteSheet = spriteSheets[terrain];
            string spriteName = $"{terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            _sourceRectangle = spriteSheet.GetSprite(spriteName);
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
        }

        public void DetermineNeighbors(IsometricManager isoManager)
        {
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
            }
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


            // Update the sprite sheet based on the new terrain
            spriteSheet = terrainSpriteSheets[Terrain];

            // Update the sprite name to use the same remainder
            string spriteName = $"{Terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            Console.WriteLine($"Terrain SpriteIndex X: {spriteIndexX}");
            Console.WriteLine($"Terrain SpriteIndex Y: {spriteIndexY}");
            _sourceRectangle = spriteSheet.GetSprite(spriteName);
        }

        public bool IsWithinBounds(int offsetX, int offsetY, int width, int height)
        {
            // Calculate the bounds of the rectangular area
            int leftBound = offsetX;
            int rightBound = offsetX + width;
            int topBound = offsetY;
            int bottomBound = offsetY + height;

            // Check if any part of the tile is within the rectangular area
            bool isWithinHorizontalBounds = Position.X + Width > leftBound && Position.X < rightBound;
            bool isWithinVerticalBounds = Position.Y + Height > topBound && Position.Y < bottomBound;

            return isWithinHorizontalBounds && isWithinVerticalBounds;
        }

        public void Reset()
        {
            // Reset the state of the tile to its initial condition
            Terrain = Terrain.Grass; // or any default value
                                     // Reset other properties as needed
            _sourceRectangle = Rectangle.Empty; // Example
                                                // Reset other relevant fields to their default state
        }

        /*public Texture2D GetBlendMap()
        {
            // Logic to determine and return the appropriate blend map texture based on adjacent terrains
        }*/


        public void Draw(SpriteBatch spriteBatch, Vector2 adjustedPosition)
        {
            spriteBatch.Draw(spriteSheet.Texture, adjustedPosition, _sourceRectangle, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet.Texture, Position, _sourceRectangle, Color.White);
        }
    }
}
