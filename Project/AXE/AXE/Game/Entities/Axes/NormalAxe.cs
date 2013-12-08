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
    class NormalAxe : Axe
    {
        public NormalAxe(int x, int y, IWeaponHolder holder)
            : base(x, y, holder)
        {
            // create stuff
        }

        override public void loadIdleMask()
        {
            _mask.w = 12;
            _mask.h = 13;
            _mask.offsetx = 4;
            _mask.offsety = 4;
        }

        override public void loadFlyMask()
        {
            if (facing == Player.Dir.Left)
            {
                _mask.w = 1;
                _mask.h = 13;
                _mask.offsetx = 4;
                _mask.offsety = 4;
            }
            else
            {
                _mask.w = 1;
                _mask.h = 13;
                _mask.offsetx = 15;
                _mask.offsety = 4;
            }
        }

        protected override void initParams()
        {
            loadIdleMask();
            attributes.Add("axe");
            current_hspeed = current_vspeed = 0;
            gravity = 0.8f;

            layer = 10;

            wrapCount = 0;
            wrapLimit = 1;

            type = PlayerData.Weapons.Axe;
        }

        protected override void initGraphic()
        {
            spgraphic = new bSpritemap((game as AxeGame).res.sprAxeSheet, 20, 20);
            loadAnims();
        }

        public override int graphicWidth()
        {
            return 20;
        }

        /* IWeapon implementation */
        public override Vector2 getGrabPosition()
        {
            return new Vector2(8, 10);
        }

        public override void onHitSolid(bEntity entity)
        {
            if (justLaunched)
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
                    onStuck(entity);
                }
            }
            else
            {
                // Stuck on others
                onStuck(entity);
            }
        }
    }
}
