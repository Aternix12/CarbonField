using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Penumbra;

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

        //Penumbra
        PenumbraComponent penumbra;

        //Random Colours
        private Random rnd = new Random();
        private Color[] Colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple };

        

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            penumbra = new PenumbraComponent(this);
            
            penumbra.SpriteBatchTransformEnabled = true;
            
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            _cam = new CtrCamera(GraphicsDevice.Viewport);

            _frameCounter = new FrameCounter();

            penumbra.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Crearing Wall
            
            for(int i = 0; i < 15; i++) {
                Random r = new Random();
                int nextValue = r.Next(0, 1900);
                Vector2 p = new Vector2(nextValue, 64);
                WallBlock ent = new WallBlock(p);
                ent.Hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
                {
                    Position = new Vector2(nextValue, 64),
                    Scale = new Vector2(16)
                };
                penumbra.Hulls.Add(ent.Hull);
                EntityManager.Add(ent);
            }

            //Adding Lights
            for (int i = 0; i < 5; i++)
            {
                Random ran1 = new Random();
                int nextValue1 = ran1.Next(0, 1920);
                Random ran2 = new Random();
                int nextValue2 = ran2.Next(0, 1080);

                Light _light = new PointLight
                {
                    Position = new Vector2(nextValue1, nextValue2),
                    Color = RandomColor(),
                    Scale = new Vector2(800),
                    Radius = 500,
                    ShadowType = ShadowType.Occluded
                };
                penumbra.Lights.Add(_light);
            }
            _bgrTexture = Content.Load<Texture2D>("spr_background");
        }

        public Color RandomColor()
        {
            return Colors[rnd.Next(Colors.Length)];
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

            System.Diagnostics.Debug.WriteLine("Test" + penumbra.Transform);
            penumbra.Transform = _cam.transform;

            base.Update(gameTime);
        }

       

        protected override void Draw(GameTime gameTime)
        {
            //Penumbra
            penumbra.BeginDraw();
            GraphicsDevice.Clear(Color.Black);
            

            //Gameplane
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            penumbra.Draw(gameTime);

            //GUI
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _arial = Content.Load<SpriteFont>("Fonts/Arial");
            _spriteBatch.DrawString(_arial, fps, new Vector2(_cam.pos.X, _cam.pos.Y), Color.White);
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
