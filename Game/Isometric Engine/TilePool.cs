using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class TilePool
    {
        private Stack<Tile> pool;

        public TilePool(int initialCapacity)
        {
            pool = new Stack<Tile>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                pool.Push(new Tile()); // Create uninitialized Tile objects
            }
        }

        public Tile GetTile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Tile tile;
            if (pool.Count > 0)
            {
                tile = pool.Pop();
                tile.Initialize(position, terrain, spriteSheets, spriteIndexX, spriteIndexY, gridX, gridY); // Initialize the tile
            }
            else
            {
                tile = new Tile(position, terrain, spriteSheets, spriteIndexX, spriteIndexY, gridX, gridY);
            }
            return tile;
        }

        public void ReturnTile(Tile tile)
        {
            tile.Reset();
            pool.Push(tile);
        }
    }

}
