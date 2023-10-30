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
            //This is just for demonstration! Will fuck FPS
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update the position based on the velocity and deltaTime
            Position += Velocity * deltaTime;

            // Define your boundary values
            float minX = 0;
            float minY = 0;
            float maxX = 1920;
            float maxY = 1080;

            // Check for boundary collision and reverse direction if needed
            if (Position.X <= minX || Position.X >= maxX - _image.Width)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
            }
            if (Position.Y <= minY || Position.Y >= maxY - _image.Height)
            {
                Velocity = new Vector2(Velocity.X, -Velocity.Y);
            }
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
