using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXE.Game.Control
{
    /**
     * Stores persistent info of the player (powerups, health)
     */
    class PlayerData
    {
        public int id;

        public PlayerData(int id)
        {
            this.id = id;
        }
    }
}
