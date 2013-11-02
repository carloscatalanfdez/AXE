using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using bEngine;

namespace AXE.Game.Entities
{
    class OneWayPlatform : Entity
    {
        public OneWayPlatform(int x, int y)
            : base(x, y)
        {
            // nothing here ._.
        }

        public override void init()
        {
            base.init();

            mask.w = 16;
            mask.h = 8;

            mask.update(x, y);

            visible = false;
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);
        }
    }
}
