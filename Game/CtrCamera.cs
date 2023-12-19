using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField

{
    public class CtrCamera
    {
        private Matrix transform;
        private Viewport _viewport;
        private Vector2 _pos;
        private Vector2 _vel;
        private float _zoom;
        private float _previousZoom;
        private readonly int worldWidth;
        private readonly int worldHeight;

        public CtrCamera(Viewport newview, int worldWidth, int worldHeight)
        {
            _viewport = newview;
            _pos.X = 4000;
            _pos.Y = 4000;
            _vel.X = 0;
            _vel.Y = 0;
            _zoom = 1f;
            _previousZoom = 1f;
            transform = Matrix.Identity;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle keyboard movement
            _vel *= 0.8f;
            float speed = 400.0f * elapsed;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) { _vel.X += speed; }
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { _vel.Y += speed; }
            if (Keyboard.GetState().IsKeyDown(Keys.A)) { _vel.X -= speed; }
            if (Keyboard.GetState().IsKeyDown(Keys.W)) { _vel.Y -= speed; }
            _pos.X += _vel.X;
            _pos.Y += _vel.Y;

            // Handle zooming
            float newZoom = _zoom; // Replace with your zoom input logic
            if (newZoom != _previousZoom)
            {
                MouseState mouseState = Mouse.GetState();
                Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

                // Check if the mouse is within the viewport
                if (_viewport.Bounds.Contains(mouseState.X, mouseState.Y))
                {
                    // Convert the screen space mouse coordinates to world space BEFORE the zoom change
                    Vector2 worldMousePositionBeforeZoom = Vector2.Transform(mousePosition, Matrix.Invert(transform));

                    // Update the zoom
                    _zoom = newZoom;

                    // Re-calculate the transformation matrix with the new zoom level
                    transform = GetTransform();

                    // Convert the screen space mouse coordinates to world space AFTER the zoom change
                    Vector2 worldMousePositionAfterZoom = Vector2.Transform(mousePosition, Matrix.Invert(transform));

                    // Adjust the camera position
                    Vector2 movement = worldMousePositionBeforeZoom - worldMousePositionAfterZoom;
                    _pos += movement;
                }

                _previousZoom = _zoom;
            }

            AdjustCameraAfterZoom();
            transform = GetTransform();
        }


        public Rectangle GetVisibleArea()
        {
            // Calculate the top-left corner of the visible area in world coordinates
            float left = _pos.X - (_viewport.Width / _zoom / 2);
            float top = _pos.Y - (_viewport.Height / _zoom / 2);

            // Calculate the width and height of the visible area
            int width = (int)(_viewport.Width / _zoom);
            int height = (int)(_viewport.Height / _zoom);

            // Return the visible area as a Rectangle
            return new Rectangle((int)left, (int)top, width, height);
        }



        public (Vector2 topLeft, Vector2 bottomRight) GetVisibleAreaCoordinates()
        {
            var visibleArea = GetVisibleArea();
            Vector2 topLeft = new Vector2(visibleArea.Left, visibleArea.Top);
            Vector2 bottomRight = new Vector2(visibleArea.Right, visibleArea.Bottom);
            return (topLeft, bottomRight);
        }


        public void AdjustCameraAfterZoom()
        {
            // Calculate the visible world area based on the current zoom level
            float visibleWorldWidth = _viewport.Width / _zoom;
            float visibleWorldHeight = _viewport.Height / _zoom;

            // Adjust the camera position to ensure the world area at (-256,-256) is at the edge of the viewport
            _pos.X = Math.Max(_pos.X, visibleWorldWidth / 2 - 256);
            _pos.Y = Math.Max(_pos.Y, visibleWorldHeight / 2 - 256);
            _pos.X = Math.Min(_pos.X, worldWidth + 256 - visibleWorldWidth / 2);
            _pos.Y = Math.Min(_pos.Y, worldHeight + 256 - visibleWorldHeight / 2);
        }


        public Vector2 GetPos()
        { return _pos; }

        public void SetPos(Vector2 value)
        { _pos = value; }

        public float GetZoom()
        { return _zoom; }

        public void SetZoom(float value)
        {
            // You might want to limit the zoom level to a certain range.
            _zoom = MathHelper.Clamp(value, 0.4f, 2f);
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                   Matrix.CreateScale(_zoom, _zoom, 1) *
                   Matrix.CreateTranslation(new Vector3(_viewport.Width * 0.5f, _viewport.Height * 0.5f, 0));
        }

    }
}
