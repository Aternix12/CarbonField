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

        private Vector2 _velocity;
        private int _collisionwait;


        public WallBlock(Vector2 pos)
        {
            _classtype = "WallBlock";
            _collisionwait = 0;
            _image = Program.game.Content.Load<Texture2D>("spr_wallblock");
            _position = pos;
            _rotation = 0;
            _origin = new Vector2(_image.Width / 2, _image.Height / 2);

            _textureData = new Color[_image.Width * _image.Height];
            _image.GetData(_textureData);
            Children = new List<GameObject>();

            

            Random r = new Random();
            int nextValue = r.Next(-100, 100);
            _velocity.X = nextValue;
            nextValue = r.Next(-100, 100);
            _velocity.Y = nextValue;
        }
        public override void Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            if(_collisionwait != 0)
            _collisionwait -= 1;

            if (_velocity.X <= 0.5 && _velocity.X >= -0.5)
            {
                _velocity.X = 0;
            }
            if (_velocity.Y <= 0.5 && _velocity.Y >= -0.5)
            {
                _velocity.Y = 0;
            }
            // TODO: Add your update logic here
            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _velocity.Y += (float)10;

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
                _velocity.Y *= (float)-0.2;
                _velocity.X *= (float)0.8;
            }
        }



        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_image, _position, Color.White);
        }


        public override void OnCollide(GameObject obj)
        {
            if (obj._classtype == "WallBlock")
            {
                OnCollideWall((WallBlock)obj);
                
            }

            
        }

        public Vector2 velocity{
            get {return _velocity ;} 
            set { _velocity = value;}
        }

        public int collisionwait
        {
            set { _collisionwait = value; }
        }

        

        private void OnCollideWall(WallBlock block)
        {
                //if(Math.Abs(_position.X) - Math.Abs(block.position.X) < 33)
                float oldx = _velocity.X;
                float oldy = _velocity.Y;
                _velocity.X = block.velocity.X;
                _velocity.Y = block.velocity.Y;

                float newx = (float)oldx;
                float newy = (float)oldy;
                block.velocity = new Vector2(newx, newy);
                block.collisionwait = 10;


                _collisionwait = 10;
            


        }

    }
}
