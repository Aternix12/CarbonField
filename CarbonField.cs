using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace CarbonField
{
    public class CarbonField : Microsoft.Xna.Framework.Game
    {
        //Version
        public static readonly string Version = "0.2.0";

        //Settings
        private readonly GameSettings _gameSettings;

        public GraphicsDeviceManager Graphics { get; }
        private SpriteBatch _spriteBatch;

        //FPS Counter
        private FrameCounter _frameCounter;
        private string _latestFpsString = "";

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

        //World
        public World CurrentWorld { get; private set; }

        public CarbonField()
        {
            AllocConsole();
            _gameSettings = new GameSettings();
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / 6900);

            client = new Client();
        }

        protected override void Initialize()
        {
            Graphics.PreferredBackBufferWidth = _gameSettings.PreferredBackBufferWidth;
            Graphics.PreferredBackBufferHeight = _gameSettings.PreferredBackBufferHeight;
            Graphics.IsFullScreen = _gameSettings.IsFullScreen;
            Graphics.SynchronizeWithVerticalRetrace = true;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Graphics.ApplyChanges();

            //FPS
            _frameCounter = new FrameCounter();

            //LiteNetLib
            client.Initialise();
            client.StartUpdateLoop();

            //World - Will Eventually be scene management, UserInterface needs to be implemented as part of these scenes
            //Requires a IGameState interface!!!
            CurrentWorld = new World(Graphics, Content, this);
            CurrentWorld.Initialize();

            base.Initialize();

            Console.WriteLine("Initialisation Finished");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Loading Fonts
            _arial = Content.Load<SpriteFont>("Fonts/Arial");

            CurrentWorld.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            UserInterface.Update(this);

            ////Clock
            //Update Time
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < delta; i++)
            {
                _time.Increment();
            }

            //Change Penumbra Alpha
            //_daylight = ((float)Math.Sin((Math.PI/4f*_time.Seconds())-(Math.PI/2f))+1f)/2f; //Keep this for future reference.

            //Updating FPS
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool fpsUpdated = _frameCounter.Update(deltaTime);
            if (fpsUpdated)
            {
                _latestFpsString = _frameCounter.FpsString;
            }

            CurrentWorld.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            CurrentWorld.Draw(_spriteBatch, gameTime);

            //GUI
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_arial, Version, new Vector2(10, 0), Color.White);
            _spriteBatch.DrawString(_arial, _latestFpsString, new Vector2(10, 20), Color.White);
            _spriteBatch.DrawString(_arial, "Entities: " + CurrentWorld._entityManager.EntityCounter.ToString(), new Vector2(10, 40), Color.White);
            _spriteBatch.DrawString(_arial, "X: " + CurrentWorld.Cam.GetPos().X.ToString(), new Vector2(10, 60), Color.White);
            _spriteBatch.DrawString(_arial, "Y: " + CurrentWorld.Cam.GetPos().Y.ToString(), new Vector2(10, 80), Color.White);
            _spriteBatch.DrawString(_arial, "Top Left: " + CurrentWorld.Cam.GetVisibleAreaCoordinates().topLeft.ToString(), new Vector2(10, 100), Color.Red);
            _spriteBatch.DrawString(_arial, "Top Right: " + CurrentWorld.Cam.GetVisibleAreaCoordinates().bottomRight.ToString(), new Vector2(10, 120), Color.Red);
            _spriteBatch.DrawString(_arial, "Viewport: " + Graphics.GraphicsDevice.Viewport.ToString(), new Vector2(10, 140), Color.Blue);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
