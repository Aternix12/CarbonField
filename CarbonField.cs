using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        public static readonly string Version = "0.1.0";

        //Settings
        private readonly GameSettings _gameSettings;

        public GraphicsDeviceManager Graphics { get; }
        private SpriteBatch _spriteBatch;

        //Viewport Background Testing
        private Texture2D _bgrTexture;

        //FPS Counter
        private FrameCounter _frameCounter;
        private string _latestFpsString = "";

        //Penumbra
        private readonly LightingManager _lightingManager;

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

            _lightingManager = new LightingManager(this);

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
            Cam = new CtrCamera(GraphicsDevice.Viewport);

            //FPS
            _frameCounter = new FrameCounter();

            //LightingManager      
            _lightingManager.Initialize();

            //LiteNetLib
            client.Initialise();
            client.StartUpdateLoop();

            Console.WriteLine("Initialisation Finished");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Creating Wall
            for (int i = 0; i < 100; i++)
            {
                Random r = new();
                int randVal1 = r.Next(0, 1920);
                int randVal2 = r.Next(0, 1080);
                Vector2 p = new(randVal1, randVal2);
                WallBlock ent = new(p);
                EntityManager.Add(ent, _lightingManager);
            }

            //Loading Fonts
            _arial = Content.Load<SpriteFont>("Fonts/Arial");

            //Adding Lights
            _lightingManager.LoadContent(Content);

            //Creating Wall
            _lightingManager.LoadHulls();

            _bgrTexture = Content.Load<Texture2D>("spr_background");
        }



        public CtrCamera Cam { get; set; }

        protected override void Update(GameTime gameTime)
        {
            UserInterface.Update(this);

            EntityManager.Update(gameTime, Graphics, _lightingManager);

            ////Clock
            //Update Time
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < delta; i++)
            {
                _time.Increment();
            }

            //Change Penumbra Alpha
            //_daylight = ((float)Math.Sin((Math.PI/4f*_time.Seconds())-(Math.PI/2f))+1f)/2f; //Keep this for future reference.

            //Updating View
            Cam.Update(gameTime);

            //Updating FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool fpsUpdated = _frameCounter.Update(deltaTime);
            if (fpsUpdated)
            {
                _latestFpsString = _frameCounter.FpsString;
            }

            //Penumbra screen lock
            _lightingManager.Update(gameTime, Cam.GetTransform());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //Penumbra
            _lightingManager.BeginDraw();
            GraphicsDevice.Clear(Color.Black);

            //Gameplane
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Cam.GetTransform());
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            _lightingManager.Draw(gameTime);

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
