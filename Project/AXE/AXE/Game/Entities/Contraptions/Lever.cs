using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AXE.Game.Entities.Contraptions
{
    class Lever : Entity, IActivable, IContraption
    {
        public IRewarder rewarder;
        public ContraptionRewardData contraptionRewardData;
        public Entity agent;

        bSpritemap smgraphic;
        string state;

        public Lever(int x, int y) : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            smgraphic = new bSpritemap((game as AxeGame).res.sprLeverSheet, 16, 16);
            smgraphic.add(new bAnim("left", new int[] { 0 }));
            smgraphic.add(new bAnim("right", new int[] { 4 }));
            smgraphic.add(new bAnim("left-to-right", new int[] { 0, 1, 2, 3, 4 }, 0.4f, false));
            smgraphic.add(new bAnim("right-to-left", new int[] { 4, 3, 2, 1, 0 }, 0.4f, false));
            smgraphic.play("left");
            state = "left";

            mask.w = 16;
            mask.h = 16;
        }

        public override void update()
        {
            base.update();

            state = smgraphic.currentAnim.name;
            if (state == "left-to-right" || state == "right-to-left")
            {
                if (smgraphic.currentAnim.finished)
                {
                    onSolved();

                    notifyAgent();

                    if (state == "left-to-right")
                        smgraphic.play("right");
                    else
                        smgraphic.play("left");
                }
            }

            smgraphic.update();
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            smgraphic.render(sb, pos);
        }

        /* IContraption implementation */
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
            this.contraptionRewardData = rewardData;
        }

        // If we have a rewarder, this method should call onReward()
        public void onSolved()
        {
            if (rewarder != null)
                rewarder.onReward(this);
        }

        /* IActivable implementation */
        public bool activate(Entity agent)
        {
            this.agent = agent;

            if (state == "left")
            {
                smgraphic.play("left-to-right");
            }
            else if (state == "right")
            {
                smgraphic.play("right-to-left");
            }
            else
                return false;

            return true;
        }

        public void notifyAgent()
        {
            if (agent != null)
                agent.onActivationEndNotification();
        }
    }
}
