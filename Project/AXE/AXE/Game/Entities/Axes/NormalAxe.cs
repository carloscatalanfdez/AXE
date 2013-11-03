using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Axes
{
    class NormalAxe : Axe
    {
        public NormalAxe(int x, int y, IWeaponHolder holder)
            : base(x, y, holder)
        {
            // create stuff
        }

        protected override void initParams()
        {
            mask = new bMask(0, 0, 16, 16);
            mask.game = game;
            attributes.Add("axe");
            current_hspeed = current_vspeed = 0;
            gravity = 0.8f;
            graphic.color = Color.Yellow;

            layer = 10;
        }

        protected override void initGraphic()
        {
            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/axe-sheet"), 20, 20);
            loadAnims();
        }
    }
}
