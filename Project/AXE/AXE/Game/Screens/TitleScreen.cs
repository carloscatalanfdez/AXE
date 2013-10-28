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
        string message;

        public TitleScreen()
            : base()
        {
        }

        public override void init()
        {
            message = "Axe Throwing Arcade";
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (GameInput.getInstance().pressed(Pad.start))
            {
                Controller.getInstance().onGameStart();
            }
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            sb.DrawString(game.gameFont, message, new Vector2(game.getWidth() / 2 - message.Length / 2 * 8, game.getHeight() / 4), Color.Wheat);
        }
    }
}
