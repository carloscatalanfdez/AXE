using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using bEngine;

namespace AXE.Game.Entities
{
    public class Stairs : bEntity
    {
        int w, h;
        public Stairs(int x, int y, int w, int h) : base(x, y)
        {           
            this.w = w;
            this.h = h;
        }

        public override void init()
        {
            base.init();

            mask.x = this.x;
            mask.y = this.y;
            mask.w = this.w;
            mask.h = this.h;
        }
    }
}
