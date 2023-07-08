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

        //Viewport Background Testing
        private Texture2D _bgrTexture;

        //FPS Counter
        private FrameCounter _frameCounter;
        private SpriteFont _arial;

        //Penumbra
        PenumbraComponent penumbra;
        public Color bgrCol = new(255, 255, 255, 0f);
        public Light _sun = new TexturedLight();


        //Random Colours
        private Random rnd = new Random();
        private Color[] Colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple };

        //Clock
        private Clock _time = new Clock();

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            penumbra = new PenumbraComponent(this);
            penumbra.AmbientColor = bgrCol;
            penumbra.SpriteBatchTransformEnabled = true;
        }

        protected override void Initialize()
        {

            base.Initialize();

            _graphics.PreferredBackBufferWidth = 2160;
            _graphics.PreferredBackBufferHeight = 1440;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            //Camera
            _cam = new CtrCamera(GraphicsDevice.Viewport);

            //FPS
            _frameCounter = new FrameCounter();

            //Penumbra
            penumbra.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Creating Wall

            for (int i = 0; i < 5; i++)
            {
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
            for (int i = 0; i < 3; i++)
            {

                Random ran1 = new Random();
                int nextValue1 = ran1.Next(0, 1920);
                Random ran2 = new Random();
                int nextValue2 = ran2.Next(0, 1080);

                Texture2D _tex = Content.Load<Texture2D>("src_texturedlight");
                Light _light = new TexturedLight(_tex)
                {
                    Position = new Vector2(nextValue1, nextValue2),
                    //Color = RandomColor(),
                    Scale = new Vector2(800, 400),
                    Color = Color.White,
                    Intensity = 2,
                    ShadowType = ShadowType.Illuminated,

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
            //TODO: Later for controller input
            base.Update(gameTime);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            EntityManager.Update(gameTime, _graphics);

            ////Clock
            //Update Time
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < delta; i++)
            {
                _time.Increment();
            }

            //Change Penumbra Alpha
            //_daylight = ((float)Math.Sin((Math.PI/4f*_time.Seconds())-(Math.PI/2f))+1f)/2f;
            penumbra.AmbientColor = new Color(255, 255, 255, 0.8f);

            //Updating View
            _cam.Update(gameTime);

            //Updating FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            //Penumbra screen lock
            penumbra.Transform = _cam.transform;

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {


            ////Penumbra
            penumbra.BeginDraw();

            GraphicsDevice.Clear(Color.Black);

            ////Gameplane

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();


            penumbra.Draw(gameTime);

            ////GUI 
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);
            //FPS
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _arial = Content.Load<SpriteFont>("Fonts/Arial");
            _spriteBatch.DrawString(_arial, fps, new Vector2(_cam.pos.X, _cam.pos.Y), Color.White);
            _spriteBatch.DrawString(_arial, "Entities: " + EntityManager._entityCounter.ToString(), new Vector2(_cam.pos.X, _cam.pos.Y + 40), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
