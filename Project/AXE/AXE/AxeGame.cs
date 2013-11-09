using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using bEngine;
using AXE.Game;
using AXE.Game.Screens;
using AXE.Game.Control;

namespace AXE
{
    /// This is the main type for your game
    public class AxeGame : bGame
    {
        Screen screen;

        protected override void initSettings()
        {
            base.initSettings();

            horizontalZoom = 3;
            verticalZoom = 3;

            width = 320;
            height = 256;
        }

        protected override void Initialize()
        {
            Controller.getInstance().setGame(this);
            Controller.getInstance().onMenuStart();
            /*changeWorld(new LogoScreen());*/
            base.Initialize();
        }

        public override void update(GameTime gameTime)
        {
            Common.GameInput.getInstance(PlayerIndex.One).update();
            Common.GameInput.getInstance(PlayerIndex.Two).update();

            if (Common.GameInput.getInstance(PlayerIndex.One).pressed(Common.PadButton.coin))
            {
                if (GameData.get().coins > 0)
                {
                    GameData.get().credits++;
                    GameData.get().coins--;
                    GameData.saveGame();
                }
            }

            if (Common.GameInput.getInstance(PlayerIndex.One).pressed(Common.PadButton.debug))
                bConfig.DEBUG = !bConfig.DEBUG;
            else if (input.pressed(Keys.D0))
                GameData.get().coins++;

            base.update(gameTime);
        }

        public void changeWorld(Screen screen)
        {
            this.screen = screen;
            base.changeWorld(screen);
        }
    }
}
