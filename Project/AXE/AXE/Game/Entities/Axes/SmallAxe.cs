using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using AXE.Game.Control;

namespace AXE.Game.Entities.Axes
{
    class SmallAxe : Axe
    {
        const int FADEOUT_TIMER = 2;
        int fadeoutTime;
        
        Vector2 handPos;

        public SmallAxe(int x, int y, IWeaponHolder holder, Dir direction, Vector2 handPos)
            : base(x, y, holder)
        {
            // create stuff
            facing = direction;
            this.handPos = handPos;
        }

        public override void init()
        {
            base.init();

            wrappable = false;
            onThrow(10, facing, handPos);
        }

        override public void loadIdleMask()
        {
            if (idleMask == null)
            {
                idleMask = new bMask(x, y, 10, 10, 3, 3);
                idleMask.game = game;
            }

            _mask = idleMask;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprWeaponsSheet;
            initSoundEffects();
        }

        protected override void initParams()
        {
            loadIdleMask();
            attributes.Add("axe");
            current_hspeed = current_vspeed = 0;
            gravity = 0.8f;

            layer = 10;

            wrapCount = 0;
            wrapLimit = 0;

            type = PlayerData.Weapons.Small;

            fadeoutTime = 7;
        }

        protected override void initGraphic()
        {
            spgraphic = new bSpritemap((game as AxeGame).res.sprWeaponsSheet, 16, 16);
            loadAnims();
        }

        protected override void loadAnims()
        {
            spgraphic.add(new bAnim("grabbed", new int[] { 4 }));
            spgraphic.add(new bAnim("cw-rotation", new int[] { 5, 6, 7, 4 }, 0.7f));
            spgraphic.add(new bAnim("ccw-rotation", new int[] { 9, 10, 11, 8 }, 0.7f));
            spgraphic.add(new bAnim("idle", new int[] { 9 }));

            // Knife
            /*spgraphic.add(new bAnim("grabbed", new int[] { 0 }));
            spgraphic.add(new bAnim("cw-rotation", new int[] { 0 }, 0.7f));
            spgraphic.add(new bAnim("ccw-rotation", new int[] { 1 }, 0.7f));
            spgraphic.add(new bAnim("idle", new int[] { 0 }));*/
        }

        public override int graphicWidth()
        {
            return 16;
        }

        /* IWeapon implementation */
        public override Vector2 getGrabPosition()
        {
            return new Vector2(6, 8);
        }

        public override void onHitSolid(bEntity entity)
        {
            if (justLaunched && entity is Player)
                return;

            if (entity != null && (entity is Entity))
            {
                if ((entity as Entity).onHit(this))
                {
                    onHit(entity as Entity);
                    onBounce(false);
                }
                else
                {
                    onBounce(true);
                }
            }
            else
            {
                // Stuck on others
                onBounce(true);
            }
        }

        public override void onTimer(int n)
        {
            base.onTimer(n);

            if (n == FADEOUT_TIMER)
            {
                visible = false;
                world.remove(this);
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            if (timer[FADEOUT_TIMER] >= 0)
            {
               visible = !visible;
            }
        }

        public override void onBounce(bool playYourSound = true)
        {
            base.onBounce(playYourSound);

            setTimer(FADEOUT_TIMER, fadeoutTime, fadeoutTime+3);
        }
    }
}
