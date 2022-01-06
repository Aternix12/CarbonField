using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace CarbonField
{
    public class CarbonField : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private CtrCamera _cam;
        //Viewport Backgroun Testing
        private Texture2D _bgrTexture;

        //FPS Counter
        private FrameCounter _frameCounter;
        private SpriteFont _arial;

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            

        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            _cam = new CtrCamera(GraphicsDevice.Viewport);

            _frameCounter = new FrameCounter();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Crearing Wall
            
            for(int i = 0; i < 5; i++) {
                Random r = new Random();
                int nextValue = r.Next(0, 1900);
                Vector2 p = new Vector2(nextValue, 64);
                WallBlock ent = new WallBlock(p);
                EntityManager.Add(ent);
            } 

            _bgrTexture = Content.Load<Texture2D>("spr_background");
        }

        protected override void Update(GameTime gameTime)
        { 
            base.Update(gameTime);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            EntityManager.Update(gameTime, _graphics);

            //Updating View
            _cam.Update(gameTime);
            //Updating FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            base.Update(gameTime);
        }

       

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);//These are the image layers
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);

            //Drawing FPS
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _arial = Content.Load<SpriteFont>("Fonts/Arial");
            _spriteBatch.DrawString(_arial, fps, new Vector2(_cam.pos.X, _cam.pos.Y), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
