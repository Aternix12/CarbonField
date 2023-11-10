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

        private SpriteSheet grassSpriteSheet;
        private SpriteSheet dirtSpriteSheet;
        private Effect terrainBlendEffect;
        private Texture2D grassTexture;
        private Texture2D dirtTexture;

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

            // Load grass terrain spritesheet
            Texture2D grassSheetTexture = _content.Load<Texture2D>("sprites/terrain/grass_terrain");
            grassSpriteSheet = new SpriteSheet(grassSheetTexture);

            // Load dirt terrain spritesheet
            Texture2D dirtSheetTexture = _content.Load<Texture2D>("sprites/terrain/dirt_terrain");
            dirtSpriteSheet = new SpriteSheet(dirtSheetTexture);

            terrainBlendEffect = _content.Load<Effect>("shaders/TerrainBlend");
            grassTexture = grassSpriteSheet.Texture;
            dirtTexture = dirtSpriteSheet.Texture;


            // Populate the SpriteSheet with tiles (assuming 10x10 grid of 64x32 tiles)
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    grassSpriteSheet.AddSprite($"grass_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                    dirtSpriteSheet.AddSprite($"dirt_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                }
            }

            // Initialize IsometricManager
            IsoManager = new IsometricManager(5, 5, new Dictionary<Terrain, SpriteSheet>
            {
                { Terrain.Grass, grassSpriteSheet },
                { Terrain.Dirt, dirtSpriteSheet }
            });

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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());

            // Set shader parameters
            terrainBlendEffect.Parameters["grassTexture"].SetValue(grassTexture);
            terrainBlendEffect.Parameters["dirtTexture"].SetValue(dirtTexture);

            // Apply shader
            spriteBatch.End();


            _lightingManager.BeginDraw();
            _graphics.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, terrainBlendEffect, Cam.GetTransform());
            DrawTilesByTerrain(spriteBatch, Terrain.Grass);
            DrawTilesByTerrain(spriteBatch, Terrain.Dirt);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            EntityManager.Draw(spriteBatch);
            spriteBatch.End();

            _lightingManager.Draw(gameTime);
        }

        private void DrawTilesByTerrain(SpriteBatch spriteBatch, Terrain terrain)
        {
            for (int y = 0; y < IsoManager.Height; y++)
            {
                for (int x = 0; x < IsoManager.Width; x++)
                {
                    Tile tile = IsoManager.TileMap[x, y];
                    if (tile.Terrain == terrain)
                    {
                        tile.Draw(spriteBatch);
                    }
                }
            }
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

