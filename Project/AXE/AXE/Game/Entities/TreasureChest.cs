using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Contraptions;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities
{
    class TreasureChest : Entity
    {
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        enum State { Opened, Closed };
        State state;
        string treasure;

        public Lock myLock;

        public TreasureChest(int x, int y, string treasure)
            : base(x, y)
        {
            this.treasure = treasure;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprTreasureChestSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 22;
            mask.h = 12;
            mask.offsetx = 0;
            mask.offsety = 4;

            spgraphic = new bSpritemap((game as AxeGame).res.sprTreasureChestSheet, 22, 16);
            spgraphic.add(new bAnim("closed", new int[] { 0 }));
            spgraphic.add(new bAnim("open", new int[] { 1 }));
            state = State.Closed;
            if (state == State.Closed)
                spgraphic.play("closed");
            else
                spgraphic.play("open");

            myLock = null;
            if (!isOpen())
            {
                myLock = new SmallLock(x + 4, y + 8);
                world.add(myLock, "items");
            }
            layer = 19;
        }

        public override void onUpdate()
        {
            base.onUpdate();
            spgraphic.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            spgraphic.render(sb, pos);
            base.render(dt, sb);
        }

        public void spawnReward()
        {
            Item reward = null;
            switch (treasure)
            {
                case "coin":
                    reward = new Coin(x, y);
                    break;
            }

            if (reward != null)
            {
                int rewardxoffset = 8;
                int rewardyoffset = 8;
                reward.x = x + rewardxoffset - reward.graphicWidth() / 2;
                reward.y = y + rewardyoffset - reward.graphicHeight();
                world.add(reward, "items");
            }
        }

        public bool tryOpen(Player player)
        {
            if (myLock != null)
            {
                open();
                myLock.open();
                spawnReward();
            }

            return true;
        }

        protected void open()
        {
            spgraphic.play("open");
            state = State.Opened;
        }

        protected void close()
        {
            spgraphic.play("closed");
            state = State.Closed;
        }

        public bool isOpen()
        {
            return state == State.Opened;
        }

        public bool isClosed()
        {
            return state == State.Closed;
        }

        public override void onClick()
        {
            if (isClosed())
                tryOpen(null);
        }
    }

    class SmallLock : Lock
    {
        public SmallLock(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            sprite = new bSpritemap(Game.res.sprSmallLockSheet, 7, 7);
            sprite.add(new bAnim("closed", new int[] { 0 }));
            sprite.add(new bAnim("open", new int[] { 1 }));
            sprite.play("closed");

            layer = 18;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprSmallLockSheet;
        }
    }
}
