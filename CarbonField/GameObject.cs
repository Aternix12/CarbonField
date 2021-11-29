using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonField
{
	abstract class GameObject
	{
		protected Texture2D _image;

		protected Vector2 _position;
		protected Vector2 _origin;
        protected float _rotation;
        protected Color[] _textureData;
        public bool IsExpired;
        public List<GameObject> Children { get; set; }

        public Vector2 Size
		{
			get
			{
				return _image == null ? Vector2.Zero : new Vector2(_image.Width, _image.Height);
			}
		}

		public abstract void Update(GameTime gameTime, GraphicsDeviceManager graphics);

        public abstract void OnCollide(GameObject obj);

		public virtual void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(_image, _position, Color.White);
		}

        

        private Matrix Transform
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-_origin, 0)) *
                 Matrix.CreateRotationZ(_rotation) *
                 Matrix.CreateTranslation(new Vector3(_position, 0));
            }
        }

        private Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)_position.X - (int)_origin.X, (int)_position.Y - (int)_origin.Y, _image.Width, _image.Height);
            }
        }

        public virtual bool Intersects(GameObject obj)
        {
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            var transformAToB = this.Transform * Matrix.Invert(obj.Transform);

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            var stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            var stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Calculate the top left corner of A in B's local space
            // This variable will be reused to keep track of the start of each row
            var yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            for (int yA = 0; yA < this.Rectangle.Height; yA++)
            {
                // Start at the beginning of the row
                var posInB = yPosInB;

                for (int xA = 0; xA < this.Rectangle.Width; xA++)
                {
                    // Round to the nearest pixel
                    var xB = (int)Math.Round(posInB.X);
                    var yB = (int)Math.Round(posInB.Y);

                    if (0 <= xB && xB < obj.Rectangle.Width &&
                        0 <= yB && yB < obj.Rectangle.Height)
                    {
                        // Get the colors of the overlapping pixels
                        var colourA = this._textureData[xA + yA * this.Rectangle.Width];
                        var colourB = obj._textureData[xB + yB * obj.Rectangle.Width];

                        // If both pixel are not completely transparent
                        if (colourA.A != 0 && colourB.A != 0)
                        {
                            return true;
                        }
                    }

                    // Move to the next pixel in the row
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // No intersection found
            return false;
        }
    }
}
