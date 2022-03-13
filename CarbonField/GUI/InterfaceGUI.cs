using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using GeonBit.UI;

namespace CarbonField
{
    internal class InterfaceGUI
    {
        public static List<Panel> Windows = new List<Panel>();

        public void InitGUI()
        {
            CreateWindow_Login();
            CreateWindow_Register();
        }

        public void CreateWindow(Panel panel)
        {
            Windows.Add(panel);
        }

        public void CreateWindow_Login()
        {
            //Create Entities
            Panel panel = new Panel(new Vector2(800, 600));
            Button btnLogin = new Button("Login");
            TextInput txtUser = new TextInput(false);
            TextInput txtPass = new TextInput(false);
            CheckBox chkUser = new CheckBox("Remember?");
            Header headerUser = new Header("Username", Anchor.TopCenter);
            Header headerPass = new Header("Password", Anchor.AutoCenter);
            Label noAccount = new Label("No Account?", Anchor.AutoCenter);
            UserInterface.Active.AddEntity(panel);

            //Entity Settings
            txtUser.PlaceholderText = "enter username...";
            txtPass.PlaceholderText = "enter password...";

            //Add Entity
            panel.AddChild(headerUser);
            panel.AddChild(txtUser);
            panel.AddChild(headerPass);
            panel.AddChild(txtPass);
            panel.AddChild(chkUser);
            panel.AddChild(btnLogin);
            panel.AddChild(noAccount);

            //On Click
            noAccount.OnClick += (Entity entity) =>
            {
                MenuManager.ChangeMenu(MenuManager.Menu.Register);
            };

            //Create Window
            CreateWindow(panel);

        }

        public void CreateWindow_Register()
        {
            //Create Entities
            Panel panel = new Panel(new Vector2(800, 600));
            Button btnRegister = new Button("Register");
            Button btnBack = new Button("Back");
            TextInput txtUser = new TextInput(false);
            TextInput txtPass = new TextInput(false);
            TextInput txtPassRepeat = new TextInput(false);
            Header headerUser = new Header("Username", Anchor.TopCenter);
            Header headerPass = new Header("Password", Anchor.AutoCenter);
            Header headerPassRepeat = new Header("Repeat Password", Anchor.AutoCenter);
            UserInterface.Active.AddEntity(panel);

            //Entity Settings
            txtUser.PlaceholderText = "enter username...";
            txtPass.PlaceholderText = "enter password...";
            txtPassRepeat.PlaceholderText = "repeat password...";

            //Add Entity
            panel.AddChild(headerUser);
            panel.AddChild(txtUser);
            panel.AddChild(headerPass);
            panel.AddChild(txtPass);
            panel.AddChild(headerPassRepeat);
            panel.AddChild(txtPassRepeat);
            panel.AddChild(btnRegister);
            panel.AddChild(btnBack);

            //On Click
            btnBack.OnClick += (Entity entity) =>
            {
                MenuManager.ChangeMenu(MenuManager.Menu.Login);
            };

            //Create Window
            CreateWindow(panel);
        }
    }
}