using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CarbonField
{
    public class World
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly ContentManager _content;
        private readonly LightingManager _lightingManager;
        private readonly WorldUserInterface _worldUI;
        public readonly EntityManager _entityManager;
        Texture2D pixel;
        

        public IsometricManager IsoManager { get; private set; }

        //Viewport Background Testing
        private Texture2D _bgrTexture;

        public World(GraphicsDeviceManager graphics, ContentManager content, CarbonField carbonFieldInstance)
        {
            _graphics = graphics;
            _content = content;
            _lightingManager = new LightingManager(carbonFieldInstance);
            _worldUI = new WorldUserInterface(this);
            _entityManager = new EntityManager();
            pixel = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pixel.SetData(new[] { Color.White
            });
        }

        public void Initialize()
        {
            // Initialize IsometricManager
            IsoManager = new IsometricManager(200, 200, _graphics.GraphicsDevice, _content);

            // Initialize the lighting
            _lightingManager.Initialize(IsoManager);

            //Camera
            Cam = new CtrCamera(_graphics.GraphicsDevice.Viewport, IsoManager.worldWidth, IsoManager.worldHeight);
            IsoManager.SetCamera(Cam);
        }

        public void LoadContent()
        {
            // Load textures, create entities, etc.
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = GenerateRandomPositionWithinDiamond();
                WallBlock ent = new(pos, IsoManager);
                _entityManager.Add(ent, _lightingManager);
            }

            //Adding Lights
            _lightingManager.LoadContent(_content);

            //Creating Wall
            _lightingManager.LoadHulls();

            _bgrTexture = _content.Load<Texture2D>("spr_background");

            IsoManager.LoadContent();
        }

        private Vector2 GenerateRandomPositionWithinDiamond()
        {
            Random random = new();
            Vector2 position;
            do
            {
                float x = random.Next(0, IsoManager.worldWidth);
                float y = random.Next(0, IsoManager.worldHeight);
                position = new Vector2(x, y);
            } while (!IsWithinDiamond(position));

            return position;
        }

        private bool IsWithinDiamond(Vector2 position)
        {
            // Get the center of the diamond
            Vector2 center = new(IsoManager.worldWidth / 2, IsoManager.worldHeight / 2);

            // Calculate distances from the center
            float dx = Math.Abs(position.X - center.X);
            float dy = Math.Abs(position.Y - center.Y);

            // The diamond's width and height at the center
            float diamondWidth = IsoManager.worldWidth / 2f;
            float diamondHeight = IsoManager.worldHeight / 2f;

            // Check if the point is within the diamond
            // The dividing by 2 is because the diamondWidth and diamondHeight represent full widths and heights
            return (dx / diamondWidth + dy / diamondHeight) <= 1;
        }

        public void Update(GameTime gameTime)
        {
            _worldUI.Update(gameTime);

            // Update entities, check collisions, etc.
            _entityManager.Update(gameTime, _graphics, _lightingManager);

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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, /*SamplerState.PointClamp*/null, null, null, null, Cam.GetTransform());
            IsoManager.Draw(spriteBatch, Cam.GetVisibleArea());
            
            spriteBatch.End();

            //Entity Draw
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            _entityManager.Draw(spriteBatch);
            spriteBatch.End();        

            _lightingManager.Draw(gameTime);

            //World Diagnostics
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Cam.GetTransform());
            DrawRectangle(spriteBatch, Cam.GetVisibleArea(), Color.Red, 2);
            IsoManager.DrawDiag(spriteBatch);
            spriteBatch.End();
        }
        void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color); // Top
            spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Bottom, rect.Width, thickness), color); // Bottom
            spriteBatch.Draw(pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color); // Left
            spriteBatch.Draw(pixel, new Rectangle(rect.Right, rect.Top, thickness, rect.Height), color); // Right
        }
    }
}

