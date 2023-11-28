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
        private readonly IsometricManager IsoManager; //This will eventually be somehow a variable shared amongst all entities?
        
        public WallBlock(Vector2 pos, IsometricManager isoManager)
        {
            _classtype = "WallBlock";
            _image = Program.Game.Content.Load<Texture2D>("spr_wallblock");
            Position = pos;
            _rotation = 0;
            _origin = new Vector2(_image.Width / 2, _image.Height / 2);
            IsoManager = isoManager;

            _textureData = new Color[_image.Width * _image.Height];
            _image.GetData(_textureData);
            Children = new List<GameObject>();

            

            Random r = new();
            int nextValue = r.Next(-300, 300);
            Velocity = new Vector2(nextValue, Velocity.Y);

            nextValue = r.Next(-300, 300);
            Velocity = new Vector2(Velocity.X, nextValue);

            

        }

        public override void Update(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            //This will eventually need to be handled through ICollision Interface!


            //This is just for demonstration! Will fuck FPS
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update the position based on the velocity and deltaTime
            Position += Velocity * deltaTime;

            // Check for boundary collision and reverse direction if needed
            if (!IsWithinDiamond(Position))
            {
                BounceOffBoundary();
            }
        }

        private bool IsWithinDiamond(Vector2 position)
        {
            Vector2 center = new Vector2(IsoManager.worldWidth / 2, IsoManager.worldHeight / 2);
            Vector2 relativePosition = position - center;
            return (Math.Abs(relativePosition.X) / (IsoManager.worldWidth / 2) +
                    Math.Abs(relativePosition.Y) / (IsoManager.worldHeight / 2)) <= 1;
        }

        private void BounceOffBoundary()
        {
            Vector2 normal = CalculateBoundaryNormal(Position);
            Velocity = Vector2.Reflect(Velocity, normal);
        }

        private Vector2 CalculateBoundaryNormal(Vector2 position)
        {
            Vector2 center = new Vector2(IsoManager.worldWidth / 2, IsoManager.worldHeight / 2);
            Vector2 relativePosition = position - center;

            // Normalizing the relative position
            relativePosition.Normalize();

            // Depending on the quadrant, determine the normal
            if (Math.Abs(relativePosition.X) > Math.Abs(relativePosition.Y))
            {
                return new Vector2(Math.Sign(relativePosition.X), 0);
            }
            else
            {
                return new Vector2(0, Math.Sign(relativePosition.Y));
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
