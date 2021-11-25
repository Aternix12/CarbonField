using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    public class CarbonField : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private CtrCamera _cam;
        //Viewport Backgroun Testing
        private Texture2D _bgrTexture;
        private Vector2 _bgrPos;

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            _cam = new CtrCamera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Crearing Wall
            Vector2 p = new Vector2(64, 64);
            WallBlock ent = new WallBlock(p);
            EntityManager.Add(ent);

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
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, _cam.transform);//These are the image layers
            _spriteBatch.Draw(_bgrTexture, new Vector2(0, 0), Color.White);
            EntityManager.Draw(_spriteBatch);
            
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
