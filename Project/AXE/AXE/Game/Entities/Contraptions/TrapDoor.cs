using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using bEngine;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Contraptions
{
    class TrapDoor : Entity, IContraption, IRewarder
    {
        public IRewarder rewarder;
        public ContraptionRewardData contraptionRewardData;

        // Remember the width when we nuke the mask to fake the trap door opening
        // This way we know what width should we have when we close it and bring back the mask
        protected int prevWidth;
        protected bMask isAnyoneOnTopMask;

        protected bool _isOpen = false;
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        public TrapDoor(int x, int y, bool open = true) 
            : base(x, y)
        {
            _isOpen = open;
        }

        override public void init()
        {
            base.init();

            spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/trapdoor-sheet"), 64, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("open", new int[] { 1 }));
            spgraphic.add(new bAnim("close", new int[] { 0 }));

            if (!_isOpen)
            {
                mask.w = 64;
                mask.h = 3;
                mask.offsetx = 0;
                mask.offsety = 0;
                spgraphic.play("open");
            }
            else
            {
                prevWidth = 64;
                mask.w = 0;
                mask.h = 0;
                mask.offsetx = 0;
                mask.offsety = 0;
                spgraphic.play("close");
            }

            // Mask used to check for collisions with player
            isAnyoneOnTopMask = new bMask(x, y - 1, 8, 3, 32 - 4, 0);

            spgraphic.play("idle");
        }

        public void open()
        {
            // Play sound?
            prevWidth = _mask.w;
            _mask.w = 0;
            spgraphic.play("open");
            _isOpen = !_isOpen;
        }

        public void close()
        {
            // Play sound?
            _mask.w = prevWidth;
            spgraphic.play("close");
            _isOpen = !_isOpen;
        }

        public bool isOpen()
        {
            return _isOpen;
        }

        public override void onUpdate()
        {
            base.onUpdate();

            if (!isOpen())
            {
                bMask holdMyMaskPlease = _mask;
                _mask = isAnyoneOnTopMask;
                bool playerOnTop = placeMeeting(x, y - 1, "player");
                _mask = holdMyMaskPlease;

                if (playerOnTop)
                {
                    onSolved();
                }
            }

            spgraphic.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            spgraphic.render(sb, pos);
        }

        /**
         * IREWARDER METHODS
         */
        public void onReward(IContraption contraption)
        {
            // This may be me. Oh ho!!
            if (isOpen())
            {
                close();
            }
            else
            {
                open();
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
