using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    public class CarbonField : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D logo;
        Vector2 _position = new Vector2(0, 0);
        Vector2 _velocity = new Vector2(100, 100);

        public CarbonField()
        {
            _graphics = new GraphicsDeviceManager(this);
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


            logo = this.Content.Load<Texture2D>("spr_wallblock");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(_position.X <= 0 && _velocity.X < 0)
            {
                _velocity.X *= -1; 
            }
            else if (_position.Y <= 0 && _velocity.Y < 0)
            {
                _velocity.Y *= -1;
            }
            else if (_position.X >= _graphics.GraphicsDevice.Viewport.Width - 32 && _velocity.X > 0)
            {
                _velocity.X *= -1;
            }
            else if (_position.Y >= _graphics.GraphicsDevice.Viewport.Height - 32 && _velocity.Y > 0)
            {
                _velocity.Y *= -1;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();//These are the image layers
            _spriteBatch.Draw(logo, _position, color: Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
