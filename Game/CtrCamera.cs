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
        private float _zoomVel;
        private readonly int worldWidth;
        private readonly int worldHeight;

        public CtrCamera(Viewport newview, int worldWidth, int worldHeight)
        {
            _viewport = newview;
            _pos.X = worldWidth / 2 - _viewport.Width / 2;
            _pos.Y = worldHeight / 2 - _viewport.Height / 2;
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

            // 1. Calculate WASD Movement
            _vel *= 0.6f;
            float speed = 1600.0f * elapsed;
            Vector2 wasdMovement = new Vector2(
                (Keyboard.GetState().IsKeyDown(Keys.D) ? speed : 0) - (Keyboard.GetState().IsKeyDown(Keys.A) ? speed : 0),
                (Keyboard.GetState().IsKeyDown(Keys.S) ? speed : 0) - (Keyboard.GetState().IsKeyDown(Keys.W) ? speed : 0)
            );

            // 2. Calculate Zoom Effect (independently)
            _zoomVel *= 0.85f; // Apply friction to zoom velocity
            _zoom += _zoomVel * elapsed; // Update zoom
            _zoom = MathHelper.Clamp(_zoom, 0.4f, 2f); // Clamp zoom

            Vector2 zoomEffect = Vector2.Zero;
            if (_zoom != _previousZoom)
            {
                MouseState mouseState = Mouse.GetState();
                if (_viewport.Bounds.Contains(mouseState.X, mouseState.Y))
                {
                    Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                    Vector2 worldMousePositionBeforeZoom = Vector2.Transform(mousePosition, Matrix.Invert(transform));
                    transform = GetTransform();
                    Vector2 worldMousePositionAfterZoom = Vector2.Transform(mousePosition, Matrix.Invert(transform));
                    zoomEffect = worldMousePositionBeforeZoom - worldMousePositionAfterZoom;
                }
            }

            // 3. Combine Both Movements
            _pos += wasdMovement + zoomEffect;

            _previousZoom = _zoom;
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

        public void UpdateZoomVelocity(float zoomDelta)
        {
            _zoomVel += zoomDelta;
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
