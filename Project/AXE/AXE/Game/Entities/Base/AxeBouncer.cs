using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Axes;
using bEngine;
using bEngine.Graphics;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Base
{
    class AxeBouncer : Enemy
    {
        public AxeBouncer(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            _mask.w = 16;
            _mask.h = 16;
            _mask.offsetx = 0;
            _mask.offsety = 0;
        }

        public override AxeHitResponse onAxeHit(Axe other)
        {
            return AxeHitResponse.generateRedirectResponseWithSpeed(-other.current_hspeed*0.4f, -(float) Math.Abs(other.current_hspeed*0.8f));
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);


            sb.Draw(bDummyRect.sharedDummyRect(game), mask.rect, Color.Plum);
        }
    }
}
