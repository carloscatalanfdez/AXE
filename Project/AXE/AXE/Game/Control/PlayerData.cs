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
    public class PlayerData
    {
        public PlayerIndex id;
        // Coins collected on this session
        public int collectedCoins;

        public PlayerData(PlayerIndex id)
        {
            this.id = id;
            this.collectedCoins = 0;
        }
    }
}
