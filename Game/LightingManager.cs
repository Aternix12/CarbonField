using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;
using System;

namespace CarbonField
{
    public class LightingManager
    {
        private readonly PenumbraComponent _penumbra;

        private Color _bgrCol = new(255, 255, 255, 0f);

        public readonly Light _sun = new TexturedLight();

        private IsometricManager IsoManager;

        public LightingManager(CarbonField game)
        {
            
            _penumbra = new PenumbraComponent(game)
            {
                AmbientColor = _bgrCol,
                SpriteBatchTransformEnabled = true
            };
        }

        public void Initialize(IsometricManager IsoManager)
        {
            this.IsoManager = IsoManager;
            _penumbra.Initialize();
        }

        public void LoadContent(ContentManager content)
        {
            // Load any Penumbra-specific content (like textures for lights) here
            // and add lights and hulls to the PenumbraComponent.
            // Assuming that Content is your ContentManager instance.
            for (int i = 0; i < 35; i++)
            {
                Texture2D _tex = content.Load<Texture2D>("src_texturedlight");
                Light _light = new TexturedLight(_tex)
                {
                    Position = GenerateRandomPositionWithinDiamond(),
                    Scale = new Vector2(800, 400),
                    Color = Color.White,
                    Intensity = 3,
                    ShadowType = ShadowType.Illuminated,

                };
                _penumbra.Lights.Add(_light);
            }
        }

        private Vector2 GenerateRandomPositionWithinDiamond()
        {
            Random random = new Random();
            Vector2 position;
            do
            {
                float x = random.Next(0, IsoManager.worldWidth);
                float y = random.Next(0, IsoManager.worldHeight);
                position = new Vector2(x, y);
            } while (!IsWithinDiamond(position));

            return position;
        }

        private bool IsWithinDiamond(Vector2 position)
        {
            // Get the center of the diamond
            Vector2 center = new Vector2(IsoManager.worldWidth / 2, IsoManager.worldHeight / 2);

            // Calculate distances from the center
            float dx = Math.Abs(position.X - center.X);
            float dy = Math.Abs(position.Y - center.Y);

            // The diamond's width and height at the center
            float diamondWidth = IsoManager.worldWidth / 2f;
            float diamondHeight = IsoManager.worldHeight / 2f;

            // Check if the point is within the diamond
            // The dividing by 2 is because the diamondWidth and diamondHeight represent full widths and heights
            return (dx / diamondWidth + dy / diamondHeight) <= 1;
        }

        public void LoadHulls()
        {
            // Creating a Hull for example
            // You can create and add Hulls similarly to how you did with lights
/*            Hull hull = new(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
            {
                Position = new Vector2(100, 100),
                Scale = new Vector2(16)
            };
            AddHull(hull);*/
        }

        public void AddLight(Light light)
        {
            Console.WriteLine("Adding light");
            _penumbra.Lights.Add(light);
        }

        public void AddHull(Hull hull)
        {
            _penumbra.Hulls.Add(hull);
        }

        public void Update(GameTime gameTime, Matrix transform)
        {
            //Change Penumbra Alpha
            //_daylight = ((float)Math.Sin((Math.PI/4f*_time.Seconds())-(Math.PI/2f))+1f)/2f; //Keep this for future reference.

            //This is the brightness of the game
            _penumbra.AmbientColor = new Color(255, 255, 255, 0.6f);
            _penumbra.Transform = transform;
        }

        public void BeginDraw()
        {
            _penumbra.BeginDraw();
        }

        public void Draw(GameTime gameTime)
        {
            _penumbra.Draw(gameTime);
        }
    }
}
