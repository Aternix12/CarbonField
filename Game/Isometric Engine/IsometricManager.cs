using CarbonField.Game;
using Microsoft.Xna.Framework;

namespace CarbonField.Game
{
    public class IsometricManager
    {
        private readonly int width;
        private readonly int height;
        private readonly Tile[,] tileMap;
        private readonly SpriteSheet tileSpriteSheet;


        public IsometricManager(int width, int height, SpriteSheet spriteSheet)
        {
            this.width = width;
            this.height = height;
            this.tileSpriteSheet = spriteSheet;
            this.tileMap = new Tile[width, height];
        }

        public void Initialize()
        {
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

                    string tileName = $"tile_{x % 10}_{y % 10}"; // Example naming convention
                    tileMap[x, y] = new Tile(isoPosition, tileName, tileSpriteSheet);
                }
            }
        }

        public int Width => width;
        public int Height => height;
        public Tile[,] TileMap => tileMap;
    }
}