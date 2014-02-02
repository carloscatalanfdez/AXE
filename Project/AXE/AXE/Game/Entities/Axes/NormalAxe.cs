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
            if (idleMask == null)
            {
                idleMask = new bMask(x, y, 12, 13, 4, 4);
                idleMask.game = game;
            }

            _mask = idleMask;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprAxeSheet;
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
    }
}
