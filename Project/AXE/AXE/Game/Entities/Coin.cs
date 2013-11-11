using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities
{
    class Coin : Item
    {
        public int value;

        public Coin(int x, int y, int value = 1)
            : base(x, y)
        {
            this.value = value;
        }

        public override void initParams()
        {
            spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/coin-sheet"), 16, 17);
            spgraphic.add(new bAnim("idle", new int[] { 0, 1, 2 }, 0.2f));
            spgraphic.play("idle");

            mask.w = 14;
            mask.h = 15;
            mask.offsetx = 1;
            mask.offsety = 1;

            state = State.Idle;

            layer = 11;
        }

        public override void onCollected()
        {
            state = State.Taken;
            timer[0] = 10;
        }

        public override void onTimer(int n)
        {
            if (n == 0 && state == State.Taken)
            {
                onDisappear();
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            if (state == State.Taken)
            {
                pos.Y -= 5;
                graphic.color *= 0.8f;
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
        }
    }
}
