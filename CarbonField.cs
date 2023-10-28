using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Penumbra;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using System.Runtime.InteropServices;
using CarbonField.Game;

namespace CarbonField
{
    public class CarbonField : Microsoft.Xna.Framework.Game
    {
        //Version
        public static readonly string Version = "1.0.0";

        //Settings
        private readonly GameSettings _gameSettings;

        public GraphicsDeviceManager Graphics { get; }
        private SpriteBatch _spriteBatch;

        private CtrCamera _cam;

        //Viewport Background Testing
        private Texture2D _bgrTexture;

        //FPS Counter
        private FrameCounter _frameCounter;
        private string _latestFpsString = "";

        //Penumbra
        readonly PenumbraComponent penumbra;
        private Color _bgrCol = new(255, 255, 255, 0f);

        public Color BgrCol { get; set; }

        public readonly Light _sun = new TexturedLight();


        //Random Colours
        private readonly Random rnd = new();
        private readonly Color[] Colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple };

        //Clock
        private readonly Clock _time = new();

        //Console
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        //Networking
        readonly Client client;

        //Fonts
        SpriteFont _arial;

        public CarbonField()
        {
            AllocConsole(); 
            _gameSettings = new GameSettings();
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / 6900);

            PenumbraComponent penumbraComponent = new(this)
            {
                AmbientColor = _bgrCol,
                SpriteBatchTransformEnabled = true
            };
            penumbra = penumbraComponent;

            client = new Client();
        }

        protected override void Initialize()
        {

            base.Initialize();

            Graphics.PreferredBackBufferWidth = _gameSettings.PreferredBackBufferWidth;
            Graphics.PreferredBackBufferHeight = _gameSettings.PreferredBackBufferHeight;
            Graphics.IsFullScreen = _gameSettings.IsFullScreen;
            Graphics.SynchronizeWithVerticalRetrace = false;
            Graphics.ApplyChanges();

            //Camera
            _cam = new CtrCamera(GraphicsDevice.Viewport);

            //FPS
            _frameCounter = new FrameCounter();

            //Penumbra
            penumbra.Initialize();

            //LiteNetLib
            client.Initialise();

            Console.WriteLine("Initialisation Finished");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Creating Wall

            for (int i = 0; i < 1; i++)
            {
                Random r = new();
                int nextValue = r.Next(0, 1900);
                Vector2 p = new (nextValue, 64);
                WallBlock ent = new(p)
                {
                    Hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
                    {
                        Position = new Vector2(nextValue, 64),
                        Scale = new Vector2(16)
                    }
                };
                penumbra.Hulls.Add(ent.Hull);
                EntityManager.Add(ent);
            }

            //Loading Fonts
            _arial = Content.Load<SpriteFont>("Fonts/Arial");

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

        public CtrCamera Cam
        {
            get { return _cam; }
            set { _cam = value; }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UserInterface.Update(this);

            EntityManager.Update(gameTime, Graphics);

            ////Clock
            //Update Time
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < delta; i++)
            {
                _time.Increment();
            }

            //Change Penumbra Alpha
            //_daylight = ((float)Math.Sin((Math.PI/4f*_time.Seconds())-(Math.PI/2f))+1f)/2f; //Keep this for future reference.
            penumbra.AmbientColor = new Color(255, 255, 255, 0.8f);

            //Updating View
            _cam.Update(gameTime);

            //Updating FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool fpsUpdated = _frameCounter.Update(deltaTime);
            if (fpsUpdated)
            {
                _latestFpsString = _frameCounter.FpsString;
            }

            //Penumbra screen lock
            penumbra.Transform = _cam.GetTransform();

            //LitenetLib
            //client.Update();

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //Penumbra
            //penumbra.BeginDraw();
            GraphicsDevice.Clear(Color.Black);

            //Gameplane
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.GetTransform());
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            //penumbra.Draw(gameTime);

            //GUI
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_arial, Version, new Vector2(10, 0), Color.White);
            _spriteBatch.DrawString(_arial, _latestFpsString, new Vector2(10, 20), Color.White);
            _spriteBatch.DrawString(_arial, "Entities: " + EntityManager.EntityCounter.ToString(), new Vector2(10, 40), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
