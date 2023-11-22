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

        public CtrCamera(Viewport newview)
        {
            _viewport = newview;
            _pos.X = 4000;
            _pos.Y = 4000;
            _vel.X = 0;
            _vel.Y = 0;
            _zoom = 1f;
            _previousZoom = 1f;
            transform = Matrix.Identity;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //This keyboard movement need to be handled by UserInterface.cs
            _vel *= 0.8f;
            float speed = 100.0f * elapsed; // Now speed is per second, not per frame
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                _vel.X += speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                _vel.Y += speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                _vel.X -= speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _vel.Y -= speed;
            }
            _pos.X += _vel.X;
            _pos.Y += _vel.Y;

            if (_zoom != _previousZoom)
            {
                // Get mouse state and position
                MouseState mouseState = Mouse.GetState();
                Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

                // Convert the screen space mouse coordinates to world space
                Vector2 worldMousePosition = Vector2.Transform(mousePosition, Matrix.Invert(transform));

                // Create the scale transform at the world mouse position
                transform = Matrix.CreateTranslation(new Vector3(-worldMousePosition.X, -worldMousePosition.Y, 0)) *
                            Matrix.CreateScale(_zoom) *
                            Matrix.CreateTranslation(new Vector3(worldMousePosition.X, worldMousePosition.Y, 0));
                
            }
            AdjustCameraAfterZoom();
            _previousZoom = _zoom;
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

        public (Vector2 topLeft, Vector2 bottomRight) GetTransformedViewportCorners()
        {
            // Define corners of the viewport
            Vector2 topLeft = new Vector2(0, 0);
            Vector2 bottomRight = new Vector2(_viewport.Width, _viewport.Height);

            // Apply the transformation
            topLeft = Vector2.Transform(topLeft, GetTransform());
            bottomRight = Vector2.Transform(bottomRight, GetTransform());

            return (topLeft, bottomRight);
        }



        public void AdjustCameraAfterZoom()
        {
            // Calculate the visible world area based on the current zoom level
            float visibleWorldWidth = _viewport.Width / _zoom;
            float visibleWorldHeight = _viewport.Height / _zoom;

            // Adjust the camera position to ensure the world area at (0,0) is at the edge of the viewport
            _pos.X = Math.Max(_pos.X, visibleWorldWidth / 2);
            _pos.Y = Math.Max(_pos.Y, visibleWorldHeight / 2);
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
