using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE.Common;

namespace AXE.Game.Screens
{
    class LogoScreen : Screen
    {
        bStamp logo;
        GameInput input;

        public LogoScreen()
            : base()
        {
        }

        public override void init()
        {
            input = GameInput.getInstance();
            logo = new bStamp(game.Content.Load<Texture2D>("Assets/badladns_banner"));
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (input.pressed(Pad.start))
                game.changeWorld(new TitleScreen());
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            logo.render(sb, game.getWidth() / 2 - logo.width / 2, game.getHeight() / 2 - logo.height / 2);
        }
    }
}
