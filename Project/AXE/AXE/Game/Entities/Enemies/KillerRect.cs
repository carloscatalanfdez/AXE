using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities.Enemies
{
    class KillerRect : Entity, IHazard
    {
        public Player.DeathState type;
        public IHazardProvider owner;

        public KillerRect(int x, int y, int w, int h, Player.DeathState type)
            : base(x, y)
        {
            mask.w = w;
            mask.h = h;
            this.type = type;
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

        public void setOwner(IHazardProvider owner)
        {
            this.owner = owner;
        }

        public IHazardProvider getOwner()
        {
            return this.owner;
        }

        public Player.DeathState getType()
        {
            return type;
        }

        public virtual void onHit()
        {
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);
            if (bEngine.bConfig.DEBUG)
                sb.Draw(bEngine.Graphics.bDummyRect.sharedDummyRect(game), mask.rect, color);
        }
    }
}
