using CarbonField.Game;
using Penumbra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField.Game
{
    public class IsometricManager
    {
        Tile[,] tileMap = new Tile[width, height];

        public IsometricManager()
        {
        }

        public void Initialize()
        {
            Tile[,] tileMap = new Tile[width, height];

            // Initialization
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileMap[x, y] = new Tile(new Vector2(x * Tile.Width, y * Tile.Height));
                }
            }

        }
    }
}
