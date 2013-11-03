using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AXE.Game.Control
{
    /**
     * Stores persistent info of the player (powerups, health)
     */
    class PlayerData
    {
        public PlayerIndex id;

        public PlayerData(PlayerIndex id)
        {
            this.id = id;
        }
    }
}
