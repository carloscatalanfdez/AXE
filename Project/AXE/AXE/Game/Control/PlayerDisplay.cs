using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AXE.Game.Entities;
using AXE.Game.Entities.Base;
using AXE.Common;
using bEngine;
using AXE.Game.Screens;
using AXE.Game.Utils;

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
        bool displayKeys;

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
                pos = new Vector2(200, 0);
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
            displayKeys = false;

            if (playerData.playing)
            {
                if (playerData.alive)
                {
                    // Line 1: 1UP POWERUPS
                    if ((playerData.powerUps & PowerUpPickable.HIGHFALLGUARD_EFFECT) != 0)
                    {
                        renderLine1 += " HF";
                    }
                    else
                        renderLine1 += " HF PS MD"; // Just testing

                    // Line 2: SCORE COINS KEYS
                    renderLine2 = " ";
                    string scoreStr = 
                        (playerData.score == 0 ? "0" : "") + playerData.score;
                    string coinsStr =
                        (playerData.collectedCoins == 0 ? "0" : "") 
                            + playerData.collectedCoins;
                    renderLine2 += Tools.padString(scoreStr, 8);
                    renderLine2 += " ";
                    renderLine2 += Tools.padString(coinsStr, 2);

                    displayKeys = true;
                }
                else
                {
                    if (player != null && 
                        (player.mginput.pressed(PadButton.a) ||
                        player.mginput.pressed(PadButton.b)))
                        continueTimer = continueTimer / PLAYER_TIMER_STEPSPERSECOND * 
                            PLAYER_TIMER_STEPSPERSECOND;
                    else
                        continueTimer--;
                    renderLine2 = "CONTINUE? " + (continueTimer / PLAYER_TIMER_STEPSPERSECOND * 1f);
                    if (continueTimer < 0)
                        Controller.getInstance().handleCountdownEnd(playerData.id);
                    else if (Controller.getInstance().playerInput[playerNumber-1].pressed(PadButton.start))
                    {
                        if (Controller.getInstance().playerStart(index))
                            player.revive();
                    }
                }
            }
            else
            {
                if (GameData.get().credits > 0)
                    renderLine2 = "PRESS START";
                else
                    renderLine2 = "INSERT COIN";
                if (Controller.getInstance().playerInput[playerNumber-1].pressed(PadButton.start))
                {
                    if (Controller.getInstance().playerStart(index))
                        player = (world as LevelScreen).spawnPlayer(playerData);
                }
            }
        }

        void renderKeys(int x, int y, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            if (playerData.keys > 0)
                sb.DrawString(game.gameFont, "K", new Vector2(x, y), Color.Gold);
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            sb.DrawString(game.gameFont, renderLine1, pos, line1Color);
            sb.DrawString(game.gameFont, renderLine2, new Vector2(x, y+8), line2Color);

            if (displayKeys)
                renderKeys(x+96, 8, sb);
        }
    }
}
