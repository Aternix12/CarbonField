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
        private const int MaxTiles = 100; // Maximum tiles per node before splitting

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
                // Add the tile to the appropriate child node
                int index = GetChildIndex(tile.Position);
                if (index != -1)
                {
                    Children[index].AddTile(tile);
                    return;
                }
            }

            Tiles.Add(tile);

            if (Tiles.Count > MaxTiles && Bounds.Width > 1 && Bounds.Height > 1)
            {
                Split(); // Split the node if it exceeds the maximum tiles
                         // Reallocate tiles to children
                int i = 0;
                while (i < Tiles.Count)
                {
                    int index = GetChildIndex(Tiles[i].Position);
                    if (index != -1)
                    {
                        Children[index].AddTile(Tiles[i]);
                        Tiles.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
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

            List<Tile> tiles = new List<Tile>();

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
