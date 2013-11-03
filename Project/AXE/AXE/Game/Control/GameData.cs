using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Control;
using Microsoft.Xna.Framework;

namespace AXE.Game.Control
{
    class GameData
    {
        public static GameData get()
        {
            return Controller.getInstance().data;
        }

        // Declare here game data
        public int level;
        public int maxLevels;

        public PlayerData playerAData;
        public PlayerData playerBData;

        public GameData()
        {
            playerAData = new PlayerData(PlayerIndex.One);
            playerBData = new PlayerData(PlayerIndex.Two);
        }

        public void init()
        {
            maxLevels = int.MaxValue;

            // DEBUG
            level = maxLevels;
        }
    }
}
