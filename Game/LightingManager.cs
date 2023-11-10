// LightingManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Penumbra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace CarbonField
{
    public class LightingManager
    {
        private readonly PenumbraComponent _penumbra;

        private Color _bgrCol = new(255, 255, 255, 0f);

        public readonly Light _sun = new TexturedLight();

        //Random Colours
        private readonly Random rnd = new();
        private readonly Color[] Colors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Purple };

        public LightingManager(CarbonField game)
        {
            _penumbra = new PenumbraComponent(game)
            {
                AmbientColor = _bgrCol,
                SpriteBatchTransformEnabled = true
            };
        }

        public void Initialize()
        {
            // Any Penumbra-specific initialization logic can go here.
            _penumbra.Initialize();
        }

        public void LoadContent(ContentManager content)
        {
            // Load any Penumbra-specific content (like textures for lights) here
            // and add lights and hulls to the PenumbraComponent.
            // Assuming that Content is your ContentManager instance.
            for (int i = 0; i < 10; i++)
            {

                Random ran1 = new Random();
                int nextValue1 = ran1.Next(0, 1920);
                Random ran2 = new Random();
                int nextValue2 = ran2.Next(0, 1080);

                Texture2D _tex = content.Load<Texture2D>("src_texturedlight");
                Light _light = new TexturedLight(_tex)
                {
                    Position = new Vector2(nextValue1, nextValue2),
                    //Color = RandomColor(),
                    Scale = new Vector2(800, 400),
                    Color = RandomColor(),
                    Intensity = 2,
                    ShadowType = ShadowType.Illuminated,

                };
                _penumbra.Lights.Add(_light);
            }
        }

        public void LoadHulls()
        {
            // Creating a Hull for example
            // You can create and add Hulls similarly to how you did with lights
            Hull hull = new(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
            {
                Position = new Vector2(100, 100),
                Scale = new Vector2(16)
            };
            AddHull(hull);
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
            //This is the brightness of the game
            _penumbra.AmbientColor = new Color(255, 255, 255, 0.9f);
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

        private Color RandomColor()
        {
            return Colors[rnd.Next(Colors.Length)];
        }
    }
}
