using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField.Game
{
    public class Tile
    {
        public static readonly int Width = 64;
        public static readonly int Height = 32;
        private SpriteSheet spriteSheet;
        private Rectangle _sourceRectangle;
        public Terrain Terrain { get; private set; }
        private readonly Dictionary<Direction, Terrain?> adjacentTerrainTypes;

        public Vector2 Position { get; private set; }
        public Vector2 Center { get; private set; }
        public Vector2 TopCorner { get; private set; }
        public Vector2 LeftCorner { get; private set; }
        public Vector2 RightCorner { get; private set; }
        public Vector2 BottomCorner { get; private set; }
        private readonly int spriteIndexX;
        private readonly int spriteIndexY;
        public int GridX { get; private set; }
        public int GridY { get; private set; }


        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
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

            adjacentTerrainTypes = new Dictionary<Direction, Terrain?>
        {
            { Direction.Top, null },
            { Direction.Left, null },
            { Direction.Right, null },
            { Direction.Bottom, null }
        };

            CalculateCorners();
        }

        private void CalculateCorners()
        {
            // Assuming the position is the top-left corner of the tile
            float halfWidth = Width / 2f;
            float halfHeight = Height / 2f;

            Center = new Vector2(Position.X + halfWidth, Position.Y + halfHeight);
            TopCorner = new Vector2(Center.X, Position.Y);
            LeftCorner = new Vector2(Position.X, Center.Y);
            RightCorner = new Vector2(Position.X + Width, Center.Y);
            BottomCorner = new Vector2(Center.X, Position.Y + Height);
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

        public void ToggleTerrain(Dictionary<Terrain, SpriteSheet> terrainSpriteSheets)
        {
            Console.WriteLine($"Toggling terrain at [{GridX},{GridY}]");
            Console.WriteLine($"Current terrain: {Terrain}");
            // Toggle the terrain
            Terrain = Terrain == Terrain.Grass ? Terrain.Dirt : Terrain.Grass;
            Console.WriteLine($"New terrain: {Terrain}");


            // Update the sprite sheet based on the new terrain
            spriteSheet = terrainSpriteSheets[Terrain];

            // Update the sprite name to use the same remainder
            Console.WriteLine($"Index X: {spriteIndexX}");
            Console.WriteLine($"Index Y: {spriteIndexY}");
            string spriteName = $"{Terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            Console.WriteLine($"Index X: {spriteIndexX}");
            Console.WriteLine($"Index Y: {spriteIndexY}");
            _sourceRectangle = spriteSheet.GetSprite(spriteName);
        }



        /*public Texture2D GetBlendMap()
        {
            // Logic to determine and return the appropriate blend map texture based on adjacent terrains
        }*/

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet.Texture, Position, _sourceRectangle, Color.White);
        }
    }
}
