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
        public int insertedCoins;
        // Meta
        public int actualCoins;

        public PlayerData playerAData;
        public PlayerData playerBData;

        public GameData()
        {
            playerAData = new PlayerData(PlayerIndex.One);
            playerBData = new PlayerData(PlayerIndex.Two);
        }

        public void init()
        {
            maxLevels = 1;
            insertedCoins = 0;
            actualCoins = 10;

            // DEBUG
            level = 0;
        }
    }
}
