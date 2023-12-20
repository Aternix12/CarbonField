using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class TileChunk
    {
        public Tile[,] Tiles;
        public Rectangle Bounds;
        private readonly int chunkWidth;
        private readonly int chunkHeight;

        public TileChunk(int chunkWidth, int chunkHeight)
        {
            Tiles = new Tile[chunkWidth, chunkHeight];
            this.chunkWidth = chunkWidth;
            this.chunkHeight = chunkHeight;

        }

        public void SetTile(int localX, int localY, Tile tile)
        {
            if (localX >= 0 && localX < chunkWidth && localY >= 0 && localY < chunkHeight)
            {
                Tiles[localX, localY] = tile;
            }
        }

        public void CalculateBounds(int startX, int startY, int tileWidth, int tileHeight)
        {
            // Calculate the pixel dimensions of the bounds
            Bounds = new Rectangle(startX, startY, chunkWidth * tileWidth, chunkHeight * tileHeight);
        }
    }
}

