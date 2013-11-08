using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine;
using AXE.Game.Control;
using AXE.Game.Screens;

namespace AXE.Game.Entities.Contraptions
{
    class FinishLevelContraption : bEntity, IContraption
    {
        IRewarder rewarder;
        ContraptionRewardData contraptionRewardData;

        public FinishLevelContraption()
            : base(0, 0)
        {
        }

        public override void update()
        {
            base.update();

            bool solved = false;
            if ((world as LevelScreen).playerA.state == Player.MovementState.Exit)
            {
                solved = true;
            }
            else if ((world as LevelScreen).playerB != null && (world as LevelScreen).playerB.state == Player.MovementState.Exit)
            {
                solved = true;
            }

            if (solved)
            {
                onSolved();
                world.remove(this);
            }
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
