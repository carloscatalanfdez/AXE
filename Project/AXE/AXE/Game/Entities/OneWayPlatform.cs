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
        int width;

        public OneWayPlatform(int x, int y, int w)
            : base(x, y)
        {
            // nothing here ._.
            width = w;
        }

        public override void init()
        {
            base.init();

            mask.w = width;
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
