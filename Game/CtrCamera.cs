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
            _pos.X = 0;
            _pos.Y = 0;
            _vel.X = 0;
            _vel.Y = 0;
            _zoom = 1f;
            _previousZoom = 1f;
            transform = Matrix.Identity;
        }

        public void Update(GameTime gametime)
        {
            //Keyboard input for moving view
            _vel.X *= (float)0.75;
            _vel.Y *= (float)0.75;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                _vel.X += 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                _vel.Y += 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                _vel.X -= 3;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                _vel.Y -= 3;
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
            _previousZoom = _zoom;
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
            _zoom = MathHelper.Clamp(value, 0.1f, 5f);
        }

        public Matrix GetTransform()
        { return transform; }
    }
}
