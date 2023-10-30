using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Numerics;
using System;

namespace CarbonField
{
    public class UserInterface
    { 

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

            game.HandleScroll();
        }
    }
}
