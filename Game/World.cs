using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField.Game
{
    public class World
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ContentManager _content;
        private readonly LightingManager _lightingManager;

        public IsometricManager IsoManager { get; private set; }

        //Viewport Background Testing
        private Texture2D _bgrTexture;

        float previousScrollWheelValue = 0f;


        public World(GraphicsDeviceManager graphics, ContentManager content, CarbonField carbonFieldInstance)
        {
            _graphics = graphics;
            _content = content;
            _lightingManager = new LightingManager(carbonFieldInstance);
        }

        public void Initialize()
        {
            //Camera
            Cam = new CtrCamera(_graphics.GraphicsDevice.Viewport);

            // Initialize the lighting
            _lightingManager.Initialize();

            // Initialize IsometricManager
            IsoManager = new IsometricManager(50, 50);
        }

        public void LoadContent()
        {
            // Load textures, create entities, etc.
            for (int i = 0; i < 5; i++)
            {
                Random r = new();
                int randVal1 = r.Next(0, 1920);
                int randVal2 = r.Next(0, 1080);
                Vector2 p = new(randVal1, randVal2);
                WallBlock ent = new(p);
                EntityManager.Add(ent, _lightingManager);
            }

            //Adding Lights
            _lightingManager.LoadContent(_content);

            //Creating Wall
            _lightingManager.LoadHulls();

            _bgrTexture = _content.Load<Texture2D>("spr_background");

            IsoManager.LoadContent(_content);
            IsoManager.CreateCoordinatesRenderTarget(_graphics.GraphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            // Update entities, check collisions, etc.
            EntityManager.Update(gameTime, _graphics, _lightingManager);

            //Updating View
            Cam.Update(gameTime);

            //Penumbra screen lock
            _lightingManager.Update(gameTime, Cam.GetTransform());
        }

        public CtrCamera Cam { get; set; }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _lightingManager.BeginDraw();
            _graphics.GraphicsDevice.Clear(Color.Black);

            //Background Draw
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            //Isometric Draw
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Cam.GetTransform());
            IsoManager.Draw(spriteBatch);
            spriteBatch.End();

            //Entity Draw
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();

            _lightingManager.Draw(gameTime);
        }

        public void HandleScroll(CarbonField game)
        {
            var currentScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            if (currentScrollWheelValue != previousScrollWheelValue)
            {
                if (currentScrollWheelValue > previousScrollWheelValue)
                {
                    Cam.SetZoom(Cam.GetZoom() + 0.1f);
                }
                else if (currentScrollWheelValue < previousScrollWheelValue)
                {
                    Cam.SetZoom(Cam.GetZoom() - 0.1f);
                }

                previousScrollWheelValue = currentScrollWheelValue;
            }
        }
    }
}

