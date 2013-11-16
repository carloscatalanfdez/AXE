using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using AXE.Common;
using AXE.Game.Control;

namespace AXE.Game.Screens
{
    class TitleScreen : Screen
    {
        string message, insertCoinStr;
        int timer;
        bool visible;

        public TitleScreen()
            : base()
        {
        }

        public override void init()
        {
            message = "AXE THROWING ARCADE";
            insertCoinStr = "INSERT COIN";
            timer = 15;
            visible = true;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (timer > 0)
                timer--;
            else
            {
                visible = !visible;
                timer = 30;
            }

            if (GameInput.getInstance(PlayerIndex.One).pressed(PadButton.start))
            {
                if (Controller.getInstance().playerStart(PlayerIndex.One))
                    Controller.getInstance().onGameStart();
            }

            if (GameInput.getInstance(PlayerIndex.Two).pressed(PadButton.start))
            {
                if (Controller.getInstance().playerStart(PlayerIndex.Two))
                    Controller.getInstance().onGameStart();
            }
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            sb.DrawString(game.gameFont, message, new Vector2(game.getWidth() / 2 - message.Length / 2 * 8, game.getHeight() / 4), Color.White);


            if (GameData.get().credits > 0)
            {
                insertCoinStr = "PRESS " + (GameData.get().credits > 1 ? "1P OR 2P" : "1P") + " START";
                visible = true;
            }

            if (visible)
                sb.DrawString(game.gameFont, insertCoinStr, new Vector2(game.getWidth() / 2 - insertCoinStr.Length * 8 / 2, 2 * game.getHeight() / 3), Color.White);
            
            String coinsStr = "CREDITS: " + (GameData.get().credits) + " - COINS: " + (GameData.get().coins);
            sb.DrawString(game.gameFont, coinsStr, new Vector2(game.getWidth() / 2 - coinsStr.Length * 8 / 2, game.getHeight() - 8), Color.White);
        }
    }
}
