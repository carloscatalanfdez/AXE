using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AXE.Game.Entities;
using AXE.Game.Entities.Base;
using AXE.Common;
using bEngine;

namespace AXE.Game.Control
{
    class PlayerDisplay : bEntity
    {
        public const int PLAYER_TIMER_DURATION = 10;
        public const int PLAYER_TIMER_STEPSPERSECOND = 30;

        PlayerIndex index;
        PlayerData playerData;
        int playerNumber;
        int continueTimer;

        string renderLine1, renderLine2;
        Color line1Color;
        Color line2Color;

        Player player;

        public PlayerDisplay(PlayerIndex index, PlayerData data, Player player)
            : base(0, 0)
        {
            this.index = index;
            this.playerData = data;
            if (index == PlayerIndex.One)
                playerNumber = 1;
            else
                playerNumber = 2;

            this.player = player;
        }

        public override void init()
        {
            base.init();

            if (playerNumber == 1)
            {
                line1Color = new Color(132, 153, 164);
                pos = Vector2.Zero;
            }
            else
            {
                line1Color = new Color(159, 127, 127);
                pos = new Vector2(208, 0);
            }
            line2Color = Color.White;
        }

        public void startTimer()
        {
            continueTimer = PLAYER_TIMER_DURATION * PLAYER_TIMER_STEPSPERSECOND;
        }

        public override void update()
        {
            base.update();

            renderLine1 = playerNumber + "UP";

            if (playerData.playing)
            {
                if (playerData.alive)
                {
                    if ((playerData.powerUps & PowerUpPickable.HIGHFALLGUARD_EFFECT) != 0)
                    {
                        renderLine1 += " HF";
                    }

                    renderLine2 = "       00 00";
                }
                else
                {
                    continueTimer--;
                    renderLine2 = "CONTINUE? " + (continueTimer / PLAYER_TIMER_STEPSPERSECOND * 1f);
                    if (continueTimer < 0)
                        Controller.getInstance().handleCountdownEnd(playerData.id);
                    else if (Controller.getInstance().playerInput[playerNumber].pressed(PadButton.start))
                    {
                        if (Controller.getInstance().playerStart(index))
                            player.revive();
                    }
                }
            }
            else
            {
                renderLine2 = "PRESS START";
            }
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            sb.DrawString(game.gameFont, renderLine1, pos, line1Color);
            sb.DrawString(game.gameFont, renderLine2, new Vector2(x+8, y+8), line2Color);
        }
    }
}
