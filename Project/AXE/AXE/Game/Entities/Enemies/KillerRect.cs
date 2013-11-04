using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Enemies
{
    class KillerRect : Entity
    {
        public KillerRect(int x, int y, int w, int h)
            : base(x, y)
        {
            mask.w = w;
            mask.h = h;
        }

        public override void init()
        {
            base.init();
            color = Color.OrangeRed;
            color.A = 64;
            attributes.Add("killer");
            mask.x = x;
            mask.y = y;
        }

        public override int graphicWidth()
        {
            return mask.w;
        }

        public override int graphicHeight()
        {
            return mask.h;
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);
            if (bEngine.bConfig.DEBUG)
                sb.Draw(bEngine.Graphics.bDummyRect.sharedDummyRect(game), mask.rect, color);
        }
    }
}
