using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CarbonField
{
    abstract class Unit : GameObject
    {

        private readonly float _direction;

        protected Unit()
        {
            _direction = 0;
        }

        public abstract override void Update(GameTime gameTime, GraphicsDeviceManager graphics);

        //Unit Draw Needed
    }
}
