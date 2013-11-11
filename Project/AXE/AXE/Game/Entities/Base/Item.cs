using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Base
{
    class Item : Entity
    {
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        public enum State { Idle, Taken };
        public State state;

        public Item(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            initParams();
        }

        public virtual void initParams()
        {
        }

        public override void onCollision(string type, bEngine.bEntity other)
        {
            // Default behaviour, collect on touch
            if (type == "player" && state == State.Idle)
            {
                onCollected();
                (other as Player).onCollectItem(this);
            }
        }

        public virtual void onCollected()
        {
        }

        public virtual void onDisappear()
        {
            world.remove(this);
        }

        public override void onUpdate()
        {
            base.onUpdate();
            spgraphic.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            spgraphic.render(sb, pos);
        }
    }
}
