using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    abstract class Unit : GameObject
    {

        private float _direction;

        public abstract override void Update(GameTime gameTime, GraphicsDeviceManager graphics);

        /* Can be overriden for unit facing direction
        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
        */
    }
}
