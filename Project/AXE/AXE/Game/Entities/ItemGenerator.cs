using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Contraptions;
using bEngine;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities
{
    class ItemGenerator : bEntity, IRewarder
    {
        public enum Type { COINS };
        public Type type;

        public ItemGenerator(string type)
            : this(ItemGenerator.getTypeFromString(type))
        {
        }

        public ItemGenerator(Type type) : base(0,0)
        {
            this.type = type;
        }

        public void onReward(IContraption contraption)
        {
            // Get location of the reward
            Vector2 rewardPos = new Vector2();
            ContraptionRewardData rewardData = contraption.getContraptionRewardData();
            bEntity entity = rewardData.target;
            if (entity != null)
            {
                rewardPos.X = entity.pos.X;
                rewardPos.Y = entity.pos.Y - 20;
            }
            else
            {
                rewardPos = rewardData.targetPos;
            }

            // Generate it
            switch (type)
            {
                default:
                case Type.COINS:
                    Coin coin = new Coin((int) rewardPos.X, (int) rewardPos.Y, rewardData.value);
                    world.add(coin, "coins");
                    break;
            }
        }

        public static Type getTypeFromString(string type)
        {
            switch (type)
            {
                default:
                case "coins":
                    return Type.COINS;
            }
        }
    }
}
