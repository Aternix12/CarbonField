using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField

{
    class CtrCamera
    {
        public Matrix transform;
        private Viewport _view;
        private Vector2 _pos;
        private Vector2 _vel;

        public CtrCamera(Viewport newview)
        {
            _view = newview;
            _pos.X = 0;
            _pos.Y = 0;
            _vel.X = 0;
            _vel.Y = 0;
        }

        public void Update(GameTime gametime)
        {
            //Keyboard input for moving view
            _vel.X *= (float)0.75;
            _vel.Y *= (float)0.75;
            if(Keyboard.GetState().IsKeyDown(Keys.D)){
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

            transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0));
                
        }
    }
}
