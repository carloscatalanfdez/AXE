using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Contraptions
{
    struct ContraptionRewardData
    {
        // Load elements
        public int rewarderId;
        public int targetId;

        // This data will be passed to the rewarded
        // The target is the entity that will recieve the reward
        public bEntity target;
        // Coordinates of the reward (should be use as an XOR with setTarget)
        public Vector2 targetPos;
        // Value of the reward
        public int value;
    }

    interface IContraption
    {
        // The rewarder will take care of generating/activating the reward
        // It will be able to act on an entity, on some coordinates or whatever
        IRewarder getRewarder();
        void setRewarder(IRewarder rewarder);
        ContraptionRewardData getContraptionRewardData();
        void setContraptionRewardData(ContraptionRewardData rewardData);

        // If we have a rewarder, this method should call onReward()
        void onSolved();
    }
}
