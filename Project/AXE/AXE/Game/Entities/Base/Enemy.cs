using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Contraptions;
using bEngine;
using Microsoft.Xna.Framework;
using AXE.Game.Utils;

namespace AXE.Game.Entities.Base
{
    class Enemy : Entity, IContraption
    {
        public const string ATTR_SOLID = "solid";

        public IRewarder rewarder;
        public ContraptionRewardData contraptionRewardData;

        public Random random;

        public Enemy(int x, int y)
            : base(x, y)
        {
            // Rendering layer
            layer = 1;
            random = Tools.random;
        }

        protected bool alivePlayerCondition(bEntity me, bEntity other)
        {
            if (other is Player)
                return (other as Player).canDie();
            else
                return false;
        }

        /**
         * ICONTRAPTION METHODS
         */
        public IRewarder getRewarder()
        {
            return rewarder;
        }
        public void setRewarder(IRewarder rewarder)
        {
            this.rewarder = rewarder;
        }
        public ContraptionRewardData getContraptionRewardData()
        {
            return contraptionRewardData;
        }
        public void setContraptionRewardData(ContraptionRewardData rewardData)
        {
            contraptionRewardData = rewardData;
        }

        public virtual void onSolved()
        {
            if (rewarder != null)
            {
                rewarder.onReward(this);
            }
        }
    }
}
