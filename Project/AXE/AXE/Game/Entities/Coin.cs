using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities
{
    class Coin : Entity
    {
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        public enum State { Idle, Taken };
        public State state;
        public int value;

        public Coin(int x, int y, int value = 1)
            : base(x, y)
        {
            this.value = value;
        }

        public override void init()
        {
            base.init();

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

        public override void onCollision(string type, bEngine.bEntity other)
        {
            if (type == "player" && state == State.Idle)
            {
                onCollected();
                (other as Player).onCollectCoin();
            }
        }

        public void onCollected()
        {
            state = State.Taken;
            timer[0] = 10;
        }

        public override void onTimer(int n)
        {
            if (n == 0 && state == State.Taken)
            {
                world.remove(this);
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();
            spgraphic.update();

            if (state == State.Taken)
            {
                pos.Y -= 5;
                graphic.alpha -= Math.Max(0, graphic.alpha - 0.1f);
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            spgraphic.render(sb, pos);
        }
    }
}
