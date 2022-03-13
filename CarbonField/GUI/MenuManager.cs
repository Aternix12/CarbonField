using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeonBit.UI.Entities;

namespace CarbonField
{
    internal class MenuManager
    {
        public static Menu menu;

        public enum Menu
        {
            Login,
            Register,
            InGame
        }

        public static void ChangeMenu(Menu menu)
        {
            foreach (Panel window in InterfaceGUI.Windows)
            {
                window.Visible = false;
            }

            InterfaceGUI.Windows[(int)menu].Visible = true;
        }
    }
}
