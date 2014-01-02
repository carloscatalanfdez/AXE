using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Contraptions;
using bEngine;
using Microsoft.Xna.Framework;
using AXE.Game.Utils;
using AXE.Game.Screens;
using AXE.Game.Entities.Enemies;
using AXE.Game.Control;

namespace AXE.Game.Entities.Base
{
    class Enemy : Entity, IContraption
    {

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

        protected bool isPlayerOnSight(Dir dir, bool vertical, String []categories, bMask watchMask)
        {
            return isPlayerOnSight(dir, vertical, categories, watchMask, null);
        }

        /**
         * Check if the player is on the line of sight specified by the watchMask
         * usingThisVarToWrap is the bMaskList that will contained the wrapped watchMask.
         * If null, one will be created (beware of performance)
         * Dir is used to check if the instance can move horizontally in that direction towards
         * the player without colliding with any of the specified categories. Only used for horizontal movement, 
         * when vertical we suppose it's be up-down.
         * If vertical is true, vertical movement is considered
         */
        protected bool isPlayerOnSight(Dir dir, bool vertical, String []categories, bMask watchMask, bMaskList usingThisVarToWrap)
        {
            // VERY IMPORTANT
            // When holding the mask, we need to hold the original _mask, since
            // mask itself is a property and will return a hacked wrapped mask sometimes
            bMask holdMyMaskPlease = _mask;
            bMask wrappedmask = generateWrappedMask(watchMask, usingThisVarToWrap);
            mask = wrappedmask;

            bEntity spottedEntity = instancePlace(x, y, "player", null, alivePlayerCondition);
            mask = holdMyMaskPlease; // thank you!

            if (spottedEntity != null)
            {
                // Nothing stopping me from hitting you?
                Vector2 oldPos = pos;
                // Check with moveToContact, but move in steps of mask.h to improve performance (we don't need more accuracy anyways)
                float xDestination;
                float yDestination;
                if (vertical)
                {
                    xDestination = mask.x;
                    if (spottedEntity.mask.y - mask.y > 0)
                        yDestination = spottedEntity.mask.y - mask.h;
                    else
                        yDestination = spottedEntity.mask.y + mask.h;
                }
                else
                {
                    yDestination = mask.y;
                    if (dir == Dir.Right)
                    {
                        if (spottedEntity.mask.x < mask.x)  // spottedEntity wrapped t the left!
                            xDestination = (world as LevelScreen).width + spottedEntity.mask.x - mask.w;
                        else
                            xDestination = spottedEntity.mask.x - mask.w;
                    }
                    else if (dir == Dir.Left)
                    {
                        if (spottedEntity.mask.x > mask.x)  // spottedEntity wrapped to the right!
                            xDestination = -(world as LevelScreen).width + spottedEntity.mask.x - spottedEntity.mask.w;
                        else
                            xDestination = spottedEntity.mask.x + mask.w;
                    }
                    else
                    {
                        xDestination = 0;
                    }
                }


                Vector2 remnantOneWay = moveToContact(new Vector2(xDestination, yDestination), categories, new Vector2(1, mask.h));
                // Restore values
                pos = oldPos;
                if (vertical)
                {
                    return remnantOneWay.Y == 0;
                }
                else
                {
                    // Yeah, let's go
                    return remnantOneWay.X == 0;
                }

            }

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

        public virtual void onDeath(Entity killer)
        {
            Controller.getInstance().applyScore(killer, this);
        }

        public override void onCollision(string type, bEntity other)
        {
            base.onCollision(type, other);
            if (other is FlameSpiritBullet)
            {
                Entity killer = (other as Entity).getKillOwner();
                onDeath(killer);
            }
        }
            
    }
}
