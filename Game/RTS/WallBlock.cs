using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    class WallBlock : GameObject, IHull
    {

        private Vector2 Velocity { get; set; }

        public WallBlock(Vector2 pos)
        {
            _classtype = "WallBlock";
            _image = Program.Game.Content.Load<Texture2D>("spr_wallblock");
            Position = pos;
            _rotation = 0;
            _origin = new Vector2(_image.Width / 2, _image.Height / 2);

            _textureData = new Color[_image.Width * _image.Height];
            _image.GetData(_textureData);
            Children = new List<GameObject>();

            

            Random r = new();
            int nextValue = r.Next(-100, 100);
            Velocity = new Vector2(nextValue, Velocity.Y);

            nextValue = r.Next(-100, 100);
            Velocity = new Vector2(Velocity.X, nextValue);

        }
        public override void Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            
        }



        public override void Draw(SpriteBatch spriteBatch)
        {

            //Updating Hull
            Hull.Position = Position + _origin;


            if (!Collisionwait)
            {
                spriteBatch.Draw(_image, Position, Color.White);
            }
            else
            {
                spriteBatch.Draw(_image, Position, Color.Red);
            }
        }

        public void AddHull(LightingManager lightingManager)
        {
            Hull = new(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
            {
                Position = this.Position,
                Scale = new Vector2(16)
            };
            lightingManager.AddHull(Hull);
        }


        public override void OnCollide(GameObject obj)
        {
            
        }
    }
}
