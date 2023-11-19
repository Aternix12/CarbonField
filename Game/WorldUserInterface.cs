﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CarbonField.Game;
using System;

namespace CarbonField.Game
{
    public class WorldUserInterface
    {
        private readonly World _world;
        float previousScrollWheelValue = 0f;

        public WorldUserInterface(World world)
        {
            _world = world;
        }

        public void Update(GameTime gameTime)
        {
            HandleMouseInput();
            HandleScroll();
        }

        private void HandleMouseInput()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 clickPosition = new Vector2(mouseState.X, mouseState.Y);
                HandleTileClick(clickPosition);
            }
        }

        private void HandleTileClick(Vector2 screenPosition)
        {
            // Convert screen position to isometric grid coordinates
            Point gridPosition = ScreenToTile(screenPosition);
            Tile clickedTile = _world.IsoManager.GetTileAtGridPosition(gridPosition.X, gridPosition.Y);
            if (clickedTile != null)
            {
                // Logic to display tile information
                ShowTileInfo(clickedTile);
            }
        }

        private Point ScreenToTile(Vector2 screenPosition)
        {
            // Adjust the screen position based on the camera's transform
            screenPosition = Vector2.Transform(screenPosition, Matrix.Invert(_world.Cam.GetTransform()));

            // Account for the offset where the top of the diamond starts (half the tile width)
            float correctedX = screenPosition.X - Tile.Width / 2.0f;
            float correctedY = screenPosition.Y;

            // Calculate the isometric grid coordinates
            int tileX = (int)Math.Floor((correctedX / Tile.Width) + (correctedY / Tile.Height));
            int tileY = (int)Math.Floor((correctedY / Tile.Height) - (correctedX / Tile.Width));

            return new Point(tileX, tileY);
        }

        private static void ShowTileInfo(Tile tile)
        {
            string tileInfo = $"Clicked Tile: [{tile.GridX},{tile.GridY}]\n";
            tileInfo += tile.GetNeighborInfo();
            Console.WriteLine(tileInfo);
        }

        public void HandleScroll() //Needs to take into account gametime
        {
            var currentScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            if (currentScrollWheelValue != previousScrollWheelValue)
            {
                if (currentScrollWheelValue > previousScrollWheelValue)
                {
                    _world.Cam.SetZoom(_world.Cam.GetZoom() + 0.1f);
                }
                else if (currentScrollWheelValue < previousScrollWheelValue)
                {
                    _world.Cam.SetZoom(_world.Cam.GetZoom() - 0.1f);
                }

                previousScrollWheelValue = currentScrollWheelValue;
            }
        }
    }
}


