﻿using Microsoft.Xna.Framework;
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
        private readonly SpriteSheet spriteSheet;
        private readonly Rectangle _sourceRectangle;
        public Terrain Terrain { get; private set; }
        private readonly Dictionary<Direction, Terrain?> adjacentTerrainTypes;

        public Vector2 Position { get; private set; }
        public Vector2 Center { get; private set; }
        public Vector2 TopCorner { get; private set; }
        public Vector2 LeftCorner { get; private set; }
        public Vector2 RightCorner { get; private set; }
        public Vector2 BottomCorner { get; private set; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }


        public Tile(Vector2 position, Terrain terrain, Dictionary<Terrain, SpriteSheet> spriteSheets, int spriteIndexX, int spriteIndexY, int gridX, int gridY)
        {
            this.Position = position;
            this.Terrain = terrain;
            this.spriteSheet = spriteSheets[terrain];
            string spriteName = $"{terrain.ToString().ToLower()}_{spriteIndexX}_{spriteIndexY}";
            _sourceRectangle = spriteSheet.GetSprite(spriteName);
            GridX = gridX;
            GridY = gridY;

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
