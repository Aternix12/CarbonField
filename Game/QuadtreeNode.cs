using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CarbonField
{
    public class QuadtreeNode
    {
        public Rectangle Bounds { get; private set; }
        public List<Tile> Tiles { get; private set; }
        public QuadtreeNode[] Children { get; private set; }

        private const int MaxTiles = 500;
        private const int SplitMargin = 20;

        public QuadtreeNode(Rectangle bounds)
        {
            Bounds = bounds;
            Tiles = [];
            Children = null;
        }

        public void AddTile(Tile tile)
        {
            // If children exist, use spatial hashing to find the correct child node
            if (Children != null)
            {
                int index = GetChildIndex(tile.Position);
                Children[index].AddTile(tile);
                return;
            }

            Tiles.Add(tile);

            // Defer splitting until the threshold is significantly exceeded
            if (Tiles.Count > MaxTiles + SplitMargin)
            {
                Split();
            }
        }

        private void Split()
        {
            Children = new QuadtreeNode[4];
            int subWidth = Bounds.Width / 2;
            int subHeight = Bounds.Height / 2;

            Children[0] = new QuadtreeNode(new Rectangle(Bounds.Left, Bounds.Top, subWidth, subHeight));
            Children[1] = new QuadtreeNode(new Rectangle(Bounds.Left + subWidth, Bounds.Top, subWidth, subHeight));
            Children[2] = new QuadtreeNode(new Rectangle(Bounds.Left, Bounds.Top + subHeight, subWidth, subHeight));
            Children[3] = new QuadtreeNode(new Rectangle(Bounds.Left + subWidth, Bounds.Top + subHeight, subWidth, subHeight));

            // Re-allocate existing tiles to the new children
            foreach (var tile in Tiles)
            {
                int index = GetChildIndex(tile.Position);
                Children[index].AddTile(tile);
            }
            Tiles.Clear();
        }

        private int GetChildIndex(Vector2 position)
        {
            int index = 0;
            if (position.X > Bounds.Center.X) index |= 1;
            if (position.Y > Bounds.Center.Y) index |= 2;
            return index;
        }

        public IEnumerable<Tile> GetTilesInArea(Rectangle area)
        {
            if (!Bounds.Intersects(area))
            {
                yield break;
            }

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var tile in child.GetTilesInArea(area))
                    {
                        yield return tile;
                    }
                }
            }

            foreach (var tile in Tiles.Where(t => t.IsWithinBounds(area)))
            {
                yield return tile;
            }
        }
    }
}
