using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Entities.Bosses;

namespace AXE.Game.Entities.Enemies
{
    class Gargoyle : Enemy, IHazardProvider
    {
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }
        
        // State vars
        bool flipped;
        int fireDelay;

        public Gargoyle(int x, int y, bool flipped)
            : base(x, y)
        {
            this.flipped = flipped;
        }

        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprGargoyleSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 24;
            mask.h = 14;
            mask.offsetx = 0;
            mask.offsety = 1;

            spgraphic = new bSpritemap((game as AxeGame).res.sprGargoyleSheet, 24, 16);
            spgraphic.add(new bAnim("1", new int[] { 0 }));
            spgraphic.play("1");
            spgraphic.flipped = flipped;

            fireDelay = 90;
            timer[0] = fireDelay;
        }

        public override void update()
        {
            base.update();

            spgraphic.update();
        }

        public override void onTimer(int n)
        {
            base.onTimer(n);

            shoot();
            timer[0] = fireDelay;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            spgraphic.render(sb, pos);
        }

        private void shoot()
        {
            int spawnX = facing == Dir.Left ? 0 : _mask.offsetx + _mask.w;
            FireBullet bullet =
                new FireBullet(x + spawnX, y, spgraphic.flipped);
            bullet.setOwner(this);
            world.add(bullet, "hazard");
        }

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
        }

    }
}
