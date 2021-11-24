using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
	abstract class GameObject
	{
		protected Texture2D _image;

		protected Vector2 _position;
		public bool IsExpired;

		public Vector2 Size
		{
			get
			{
				return _image == null ? Vector2.Zero : new Vector2(_image.Width, _image.Height);
			}
		}

		public abstract void Update(GameTime gameTime, GraphicsDeviceManager graphics);

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_image, _position, Color.White);
		}
	}
}
