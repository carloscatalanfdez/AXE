using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Entities.Bosses;
using AXE.Game.Screens;

namespace AXE.Game.Entities.Enemies
{
    class VGargoyle : Enemy, IHazardProvider
    {
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }
        
        // State vars
        bool flipped;
        int fireDelay;

        public VGargoyle(int x, int y, bool flipped)
            : base(x, y)
        {
            this.flipped = flipped;
        }

        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprVGargoyleSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 14;
            mask.h = 22;
            mask.offsetx = 1;
            mask.offsety = 0;

            spgraphic = new bSpritemap((game as AxeGame).res.sprVGargoyleSheet, 16, 24);
            spgraphic.add(new bAnim("1", new int[] { 0 }));
            spgraphic.play("1");
            spgraphic.vflipped = flipped;

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
            int spawnY = flipped ? 0 : _mask.offsety + _mask.h;
            VFireBullet bullet =
                new VFireBullet(x, y + spawnY, flipped);
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

    class VFireBullet : KillerRect
    {
        bSpritemap sprite;

        // Parameters
        int speed;

        // State vars
        bool flipped;
        bool isBurningOut = false;

        public VFireBullet(int x, int y, bool flipped)
            : base(x, y, 20, 16, Player.DeathState.DeferredBurning)
        {
            this.flipped = flipped;
        }

        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprVFireBulletSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 8;
            mask.h = 15;
            mask.offsetx = 4;
            mask.offsety = 4;

            sprite = new bSpritemap((game as AxeGame).res.sprVFireBulletSheet, 16, 20);
            sprite.add(new bAnim("idle", new int[] { 0, 4, 8, 12 }, 0.2f));
            sprite.add(new bAnim("burnout", new int[] { 1, 5, 9, 13 }, 0.3f, false));
            sprite.play("idle");
            sprite.vflipped = flipped;

            if (flipped)
            {
                y -= sprite.width;
            }

            speed = 5;

            if (y + sprite.height < 0 || y > (world as LevelScreen).height)
                world.remove(this);
        }

        public override void onHit()
        {
            base.onHit();

            isBurningOut = true;
        }

        public override void update()
        {
            base.update();

            if (flipped)
                y -= speed;
            else
                y += speed;

            if (y + sprite.height < 0 ||
                y > (world as LevelScreen).height)
                world.remove(this);

            if (isBurningOut)
            {
                sprite.play("burnout");
                if (sprite.currentAnim.finished)
                {
                    world.remove(this);
                }
            }

            sprite.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            sprite.render(sb, pos);
        }
    }
}
