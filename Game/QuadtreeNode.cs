using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
    public class QuadtreeNode
    {
        public Rectangle Bounds { get; private set; }
        public List<Tile> Tiles { get; private set; }
        public QuadtreeNode[] Children { get; private set; }
        private TilePool tilePool;

        private const int MaxTiles = 50000; // Maximum tiles per node before splitting
        private const int MinTiles = 15000; // Minimum tiles before considering a merge
        private const int SplitMargin = 20000; // Additional margin before actual split
        private const int MergeMargin = 20000;  // Additional margin before actual merge

        public QuadtreeNode(Rectangle bounds, TilePool pool)
        {
            Bounds = bounds;
            Tiles = new List<Tile>();
            Children = null;
            tilePool = pool;
        }

        public void AddTile(Vector2 position, Terrain terrainType, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            Tile tile = tilePool.GetTile(position, terrainType, spriteSheets, spriteIndexX, spriteIndexY, gridX, gridY);

            if (Children != null)
            {
                int index = GetChildIndex(tile.Position);
                if (index != -1)
                {
                    Children[index].AddTile(position, terrainType, spriteSheets, spriteIndexX, spriteIndexY, gridX, gridY);
                    return;
                }
            }

            Tiles.Add(tile);

            Tiles.Add(tile);

            // Check if we need to split, but with an additional margin
            if (Tiles.Count > MaxTiles + SplitMargin && Bounds.Width > 1 && Bounds.Height > 1)
            {
                Split();
            }
        }

        private void TryMerge()
        {
            // Check if this node has children and if the total tile count is below the merge threshold
            if (Children == null || TotalTileCount() >= (MinTiles - MergeMargin) * 4)
            {
                return;
            }

            // Consolidate all tiles from children nodes
            foreach (var child in Children)
            {
                Tiles.AddRange(child.Tiles);
                child.Tiles.Clear();
                child.Children = null;
            }

            // Remove the children nodes as they are now empty
            Children = null;
        }

        private int TotalTileCount()
        {
            int count = Tiles.Count;
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    count += child.Tiles.Count;
                }
            }
            return count;
        }



        private void Split()
        {
            Children = new QuadtreeNode[4];
            int subWidth = Bounds.Width / 2;
            int subHeight = Bounds.Height / 2;
            Children[0] = new QuadtreeNode(new Rectangle(Bounds.Left, Bounds.Top, subWidth, subHeight), tilePool);
            Children[1] = new QuadtreeNode(new Rectangle(Bounds.Left + subWidth, Bounds.Top, subWidth, subHeight), tilePool);
            Children[2] = new QuadtreeNode(new Rectangle(Bounds.Left, Bounds.Top + subHeight, subWidth, subHeight), tilePool);
            Children[3] = new QuadtreeNode(new Rectangle(Bounds.Left + subWidth, Bounds.Top + subHeight, subWidth, subHeight), tilePool);
        }

        private int GetChildIndex(Vector2 position)
        {
            bool left = position.X > Bounds.Left + Bounds.Width / 2;
            bool top = position.Y > Bounds.Top + Bounds.Height / 2;

            if (left)
            {
                if (top)
                    return 3;
                else
                    return 1;
            }
            else
            {
                if (top)
                    return 2;
                else
                    return 0;
            }
        }

        public IEnumerable<Tile> GetTilesInArea(Rectangle area)
        {
            if (!Bounds.Intersects(area))
            {
                return Enumerable.Empty<Tile>();
            }

            List<Tile> tiles = new();

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    tiles.AddRange(child.GetTilesInArea(area));
                }
            }

            tiles.AddRange(Tiles.Where(tile => tile.IsWithinBounds(area.Left, area.Top, area.Width, area.Height)));
            return tiles;
        }
    }
}
