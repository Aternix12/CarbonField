using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    public class CarbonField : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Vector2 _position = new Vector2(0, 0);
        Vector2 _velocity = new Vector2(100, 100);

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.
            
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Vector2 p;
            p.X = 64;
            p.Y = 64;
            WallBlock ent = new WallBlock(p);
            EntityManager.Add(ent);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        { 
            base.Update(gameTime);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            EntityManager.Update(gameTime, _graphics);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);//These are the image layers
            EntityManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
