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
    class InputScreen : Screen
    {

        public InputScreen()
            : base()
        {
        }

        public override void init()
        {
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (GameInput.getInstance(PlayerIndex.One).pressed(PadButton.start) || GameInput.getInstance(PlayerIndex.Two).pressed(PadButton.start))
                // game.changeWorld(new TitleScreen());
                Controller.getInstance().onMenuStart();
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
        }
    }
}
