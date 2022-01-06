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
        
        private bool _moving;
        public WallBlock(Vector2 pos)
        {
            _classtype = "WallBlock";
            
            _moving = true;
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
            
            //Gravity
            if(_colliding == false)
            _velocity.Y += 8;

            //Minimum movement
            if (_velocity.X <= 0.5 && _velocity.X >= -0.5)
            {
                _velocity.X = 0;
            }
            if (_velocity.Y <= 0.5 && _velocity.Y >= -0.5)
            {
                _velocity.Y = 0;
            }
            
            

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
                _velocity.Y *= (float)-0.7;
                _velocity.X *= (float)0.95;
            }

            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(_collisionwait == true)
            {
                _collisionwait = false;
                _colliding = false;
            }
        }



        public override void Draw(SpriteBatch spriteBatch)
        {
            if(_collisionwait == false)
            spriteBatch.Draw(_image, _position, Color.White);
            else
            spriteBatch.Draw(_image, _position, Color.Red);
        }


        public override void OnCollide(GameObject obj)
        {
            if (obj._classtype == "WallBlock" && _moving == true)
            {
                OnCollideWall((WallBlock)obj);
                
            }

            
        }

        public Vector2 velocity{
            get {return _velocity ;} 
            set { _velocity = value;}
        }

       

        

        private void OnCollideWall(WallBlock block)
        {
            if (_colliding == false && block._colliding == false)
            {
                //float selfx = (_velocity.X / 2) + block._velocity.X;
                //float selfy = (_velocity.Y / 2) + block._velocity.Y;

                //float otherx = (block._velocity.X / 2) + _velocity.X;
                //float othery = (block._velocity.Y / 2) + _velocity.Y;

                float selfx = _velocity.X;
                float selfy = _velocity.Y;

                _velocity.X = block._velocity.X;
                _velocity.Y = block._velocity.Y;
                block._velocity.X = selfx;
                block._velocity.Y = selfy;

                _colliding = true;
                block._colliding = true;


            }

            

        }

    }
}
