using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Numerics;
using System;

namespace CarbonField
{
    public class UserInterface
    {
        static float previousScrollWheelValue = 0f;

        protected UserInterface()
        {

        }

        public static void Update(CarbonField game)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                game.Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                game.Graphics.ToggleFullScreen();
            }

            HandleScroll(game);
        }

        private static void HandleScroll(CarbonField game)
        {
            var currentScrollWheelValue = Mouse.GetState().ScrollWheelValue;

            if (currentScrollWheelValue > previousScrollWheelValue)
            {
                game.Cam.SetZoom(game.Cam.GetZoom() + 0.1f);
            }
            else if (currentScrollWheelValue < previousScrollWheelValue)
            {
                game.Cam.SetZoom(game.Cam.GetZoom() - 0.1f);
            }

            previousScrollWheelValue = currentScrollWheelValue;
        }
    }
}
