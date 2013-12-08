using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using bEngine;
using AXE.Game.Utils;

namespace AXE.Game.Screens
{
    /**
     * Common base class for every screen in Axe game
     **/
    public class Screen : bGameState, IReloadable
    {
        public Screen()
            : base()
        { 
        }

        virtual public void reloadContent()
        {
        }
    }
}
