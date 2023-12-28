using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CarbonField
{
    public class WorldUserInterface
    {
        private readonly World _world;
        float previousScrollWheelValue = 0f;
        private MouseState previousMouseState;

        public WorldUserInterface(World world)
        {
            _world = world;
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.V))
            {
                _world.Cam.ResetZoom();
            }

            HandleMouseInput();
            HandleScroll();
        }

        private void HandleMouseInput()
        {
            MouseState currentMouseState = Mouse.GetState();

            if (previousMouseState.LeftButton == ButtonState.Pressed &&
                currentMouseState.LeftButton == ButtonState.Released)
            {
                Vector2 clickPosition = new(currentMouseState.X, currentMouseState.Y);
                HandleTileClick(clickPosition);
            }

            previousMouseState = currentMouseState;
        }

        private void HandleTileClick(Vector2 screenPosition)
        {
            // Convert screen position to isometric grid coordinates
            Point gridPosition = ScreenToTile(screenPosition);
            Tile clickedTile = _world.IsoManager.GetTileAtGridPosition(gridPosition.X, gridPosition.Y);

            if (clickedTile != null)
            {
                // Toggle the terrain of the clicked tile
                clickedTile.ToggleTerrain(_world.IsoManager.TerrainSpriteSheets, _world.IsoManager);

                // Optionally, update the neighbors of the clicked tile
                //clickedTile.DetermineNeighbors(_world.IsoManager);

                // Logic to display tile information (optional)
                ShowTileInfo(clickedTile);
            }
        }


        private Point ScreenToTile(Vector2 screenPosition)
        {
            // Adjust the screen position based on the camera's transform
            screenPosition = Vector2.Transform(screenPosition, Matrix.Invert(_world.Cam.GetTransform()));

            // Calculate the half total width of all tiles // Why does this only use width? Weird
            float halfTotalWidth = _world.IsoManager.width * Tile.Width / 2f;

            // Adjust the X position of the screen based on the new tile drawing logic
            float correctedX = screenPosition.X - halfTotalWidth - _world.IsoManager.HorizontalOffset;
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

        public void HandleScroll()
        {
            var currentScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            if (currentScrollWheelValue != previousScrollWheelValue)
            {
                // Calculate the change in zoom based on the scroll wheel
                float zoomDelta = (currentScrollWheelValue - previousScrollWheelValue) * 0.03f; // Adjust multiplier as needed

                // Update the zoom velocity in the camera
                _world.Cam.UpdateZoomVelocity(zoomDelta);

                previousScrollWheelValue = currentScrollWheelValue;
            }
        }

    }
}


