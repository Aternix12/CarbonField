using CarbonField.Game;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CarbonField.Game
{
    public class IsometricManager
    {
        private readonly int width;
        private readonly int height;
        private readonly Tile[,] tileMap;
        private readonly Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;


        public IsometricManager(int width, int height, Dictionary<Terrain, SpriteSheet> spriteSheets)
        {
            this.width = width;
            this.height = height;
            this.terrainSpriteSheets = spriteSheets;
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

                    Terrain type = (x + y) % 2 == 0 ? Terrain.Grass : Terrain.Dirt;
                    //Terrain type = Terrain.Grass;

                    // Correctly passing the terrainSpriteSheets dictionary
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, type, terrainSpriteSheets, spriteIndexX, spriteIndexY);
                }
            }
        }


        public int Width => width;
        public int Height => height;
        public Tile[,] TileMap => tileMap;
    }
}