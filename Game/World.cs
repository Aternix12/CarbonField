using System;
using System.Collections.Generic;
using System.Linq;
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

        private SpriteSheet tileSpriteSheet;

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

            // Load the spritesheet texture
            Texture2D tileSheetTexture = _content.Load<Texture2D>("sprites/terrain/grasstest_terrain");

            // Initialize the SpriteSheet object
            tileSpriteSheet = new SpriteSheet(tileSheetTexture);

            // Populate the SpriteSheet with tiles (assuming 10x10 grid of 64x32 tiles)
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    string tileName = $"tile_{x}_{y}";
                    tileSpriteSheet.AddSprite(tileName, x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                }
            }

            // Initialize IsometricManager
            IsoManager = new IsometricManager(40, 40, tileSpriteSheet);
            IsoManager.Initialize();
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
            //Penumbra
            _lightingManager.BeginDraw();
            _graphics.GraphicsDevice.Clear(Color.Black);

            //Gameplane
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Cam.GetTransform());
            spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            for (int y = 0; y < IsoManager.Height; y++)
            {
                for (int x = 0; x < IsoManager.Width; x++)
                {
                    IsoManager.TileMap[x, y].Draw(spriteBatch);
                }
            }
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

