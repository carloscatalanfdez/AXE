using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AXE.Game.Utils
{
    class Tools
    {
        public static Random random = new Random();
        public static Color RandomColor 
        { 
            get 
            {
                return new Color(random.Next(256), random.Next(256), random.Next(256));
            }
        }
    }
}
