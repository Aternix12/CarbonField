using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    class WallBlock : GameObject
    {
        private Texture2D _image = Program.game.Content.Load<Texture2D>("spr_wallblock");
        private Vector2 _velocity;
        public WallBlock(Vector2 pos)
        {
            _position = pos;
            _velocity.X = 50;
            _velocity.Y = 50;
        }
        public override void Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {

            // TODO: Add your update logic here
            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _velocity.Y += (float)1;

            if (_position.X <= 0 && _velocity.X < 0)
            {
                _velocity.X *= -1;
            }
            else if (_position.Y <= 0 && _velocity.Y < 0)
            {
                _velocity.Y *= -1;
            }
            else if (_position.X >= graphics.GraphicsDevice.Viewport.Width - 32 && _velocity.X > 0)
            {
                _velocity.X *= -1;
            }
            else if (_position.Y >= graphics.GraphicsDevice.Viewport.Height - 32 && _velocity.Y > 0)
            {
                _velocity.Y *= -1;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_image, _position, Color.White);
        }


    }
}
