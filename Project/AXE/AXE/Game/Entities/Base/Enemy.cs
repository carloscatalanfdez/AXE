using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Contraptions;
using bEngine;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Base
{
    class Enemy : Entity, IContraption
    {
        public IRewarder rewarder;
        public ContraptionRewardData contraptionRewardData;

        public Enemy(int x, int y)
            : base(x, y)
        {
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
