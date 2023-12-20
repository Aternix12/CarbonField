using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CarbonField
{
    public class QuadtreeNode
    {
        public Rectangle Bounds { get; private set; }
        public List<Tile> Tiles { get; private set; }
        public QuadtreeNode[] Children { get; private set; }

        private const int MaxTiles = 10;
        private const int SplitMargin = 5;

        public QuadtreeNode(Rectangle bounds)
        {
            Bounds = bounds;
            Tiles = new List<Tile>();
            Children = null;
        }

        public void AddTile(Tile tile)
        {
            if (Children != null)
            {
                int index = GetChildIndex(tile.Position);
                if (index != -1)
                {
                    Children[index].AddTile(tile);
                    return;
                }
            }

            Tiles.Add(tile);

            // Split if necessary
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
            bool right = position.X > Bounds.Left + Bounds.Width / 2;
            bool bottom = position.Y > Bounds.Top + Bounds.Height / 2;

            if (right)
            {
                return bottom ? 3 : 1;
            }
            else
            {
                return bottom ? 2 : 0;
            }
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

            foreach (var tile in Tiles)
            {
                if (tile.IsWithinBounds(area.Left, area.Top, area.Width, area.Height))
                {
                    yield return tile;
                }
            }
        }
    }
}
