using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Enemies;

namespace AXE.Game.Control
{
    class ScoreManager
    {
        public static Dictionary<String, int> scoreMap = new Dictionary<String, int>()
        {
            { typeof(Undead).Name, 10},
            { typeof(CorrosiveSlime).Name, 20},
            { typeof(Dagger).Name, 30},
            { typeof(EvilAxeHolder).Name, 40},
            { typeof(FlameSpirit).Name, 50}
        };

        public static int getScore(object obj)
        {
            String objKey = obj.GetType().Name;
            if (scoreMap.ContainsKey(objKey))
            {
                return scoreMap[objKey];
            }
            else
            {
                return 0;
            }
        }
    }
}
