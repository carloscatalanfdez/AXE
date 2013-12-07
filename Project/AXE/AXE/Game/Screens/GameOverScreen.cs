using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;
using AXE.Common;
using AXE.Game.Control;
using AXE.Game.UI;
using AXE.Game.Utils;

namespace AXE.Game.Screens
{
    class GameOverScreen : Screen
    {
        bool finished;
        string message;

        List<int> times;
        TimedLabel leftTitle, rightTitle;
        TimedLabel treausuresLabel, killsLabel, scoreLabel, soulsLabel, coinsLabel;
        TimedLabel treausuresValue, killsValue, scoreValue, soulsValue, coinsValue;
        TimedLabel treausuresCoins, killsCoins, scoreCoins, soulsCoins, coinsCoins;
        TimedStamp totalLine;
        TimedLabel totalCoins;
        TimedLabel pouchLabel;

        int treausures, kills, score, souls, coins;
        string scoreUnits;
        int cTreausures, cKills, cScore, cSouls, cCoins;
        int cTotal, pouch;
        int transferDelta;

        int addTimer;

        public GameOverScreen()
            : base()
        {
        }

        public override void init()
        {
            entities.Add("entities", new List<bEntity>());

            finished = false;

            // Get game results
            treausures = Tools.random.Next(10000);
            kills = Tools.random.Next(10000);
            score = Tools.random.Next(25000000);
            souls = Tools.random.Next(100);
            coins = Tools.random.Next(300);

            // Treat them
            score = score / 1000; // 185100 = 185k
            scoreUnits = "K";
            if (score > 1000)
            {
                score = score / 1000; // 25000000 = 25M
                scoreUnits = "M";
            }

            // Calculate coins
            cTreausures = treausures / 100;
            cKills = kills / 20;
            cScore = score / 10 * (scoreUnits == "M" ? 25 : 1);
            cSouls = souls;
            cCoins = coins;
            cTotal = cTreausures + cKills + cScore + cScore + cCoins;
            if (cTotal > 1000)
                transferDelta = 125;
            else if (cTotal > 100)
                transferDelta = 50;
            if (cTotal > 20)
                transferDelta = 5;
            else
                transferDelta = 1;
            pouch = GameData.get().coins;

            GameData.get().coins += cTotal;
            GameData.saveGame();
            
            message = "YOU ARE DEAD";
            (game as AxeGame).res.sfxGreatBell.Play();

            finished = false;

            times = new List<int>();
            Color w = Color.White;

            leftTitle = ntl(72, 120, "YOU MANAGED", atd(40), new Color(161, 161, 161), (game as AxeGame).res.sfxBigBell);
            
            treausuresLabel = ntl(72, 136, "TREAUSURES", atd(40), w);
            treausuresValue = ntl(160, 136, padString("" + treausures, 4), atd(0), w);

            killsLabel = ntl(72, 152, "DEFEATED", atd(40), w);
            killsValue = ntl(160, 152, padString("" + kills, 4), atd(0), w);

            scoreLabel = ntl(72, 168, "SCORE", atd(45), w);
            scoreValue = ntl(160, 168, padString(score + scoreUnits, 4), atd(0), w);

            soulsLabel = ntl(72, 184, "SOULS", atd(40), w);
            soulsValue = ntl(160, 184, padString("" + souls, 4), atd(0), w);

            coinsLabel = ntl(72, 200, "COINS", atd(40), w);
            coinsValue = ntl(160, 200, padString("" + coins, 4), atd(0), w);

            rightTitle = ntl(200, 120, "COINS", atd(100), new Color(247, 224, 118), (game as AxeGame).res.sfxBigBell);

            treausuresCoins 
                       = ntl(208, 136, padString("" + cTreausures, 3), atd(40), w);
            killsCoins = ntl(208, 152, padString("" + cKills, 3), atd(40), w);
            scoreCoins = ntl(208, 168, padString("" + cScore, 3), atd(45), w);
            soulsCoins = ntl(208, 184, padString("" + cSouls, 3), atd(40), w);
            coinsCoins = ntl(208, 200, padString("" + cCoins, 3), atd(40), w);

            totalLine = new TimedStamp(200, 208, (game as AxeGame).res.sprTotalLine, atd(40), (game as AxeGame).res.sfxBigBell);
            _add(totalLine, "entities");

            totalCoins = ntl(200, 216, buildTotalString(), atd(60), w, (game as AxeGame).res.sfxBigBell);

            pouchLabel = ntl(104, 232, buildPouchString(), atd(80), w, (game as AxeGame).res.sfxGreatBell);

            addTimer = times[times.Count - 1] + 60;
        }

        
        protected override bool _add(bEntity e, string category)
        {
            entities[category].Add(e);

            return base._add(e, category);
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (addTimer >= 0)
            {
                addTimer--;
                if (addTimer < 0)
                {
                    if (!finished)
                    {
                        if (cTotal > 0)
                        {
                            (game as AxeGame).res.sfxStepC.Play();
                            cTotal -= transferDelta;
                            // Add the correct coins
                            if (cTotal < 0)
                            {
                                transferDelta += cTotal;
                                cTotal = 0;
                            }
                            pouch += transferDelta;
                            addTimer = 1;

                            totalCoins.label = buildTotalString();
                            pouchLabel.label = buildPouchString();
                        }
                        else
                        {
                            finished = true;
                            addTimer = (game as AxeGame).FramesPerSecond * 10;
                        }
                    }
                    else
                    {
                        Controller.getInstance().onMenuStart();
                    }   
                }
            }

            foreach (String key in entities.Keys)
                foreach (bEntity entity in entities[key])
                    entity.update();

            if (finished && GameInput.getInstance(PlayerIndex.One).pressed(PadButton.start))
            {
                Controller.getInstance().onMenuStart();
            }
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            sb.DrawString(game.gameFont, message, new Vector2(112, 80), Color.DarkRed);

            foreach (String key in entities.Keys)
                foreach (bEntity entity in entities[key])
                    entity.render(dt, sb);
        }

        string buildTotalString()
        {
            return padString("" + cTotal, 4) + " COINS";
        }

        string buildPouchString()
        {
            return "COIN POUCH " + padString("" + pouch, 5) + " COINS";
        }

        string padString(string label, int width, char padder = ' ')
        {
            return Tools.padString(label, width, padder);
        }

        // Wrappers for shorter code
        TimedLabel ntl(int x, int y, string label, int time, Color color, SoundEffect sfx = null)
        {
            if (sfx == null)
                sfx = (game as AxeGame).res.sfxMidBell;
            TimedLabel tl = new TimedLabel(x, y, label, time, color, sfx);
            _add(tl, "entities");
            return tl;
        }

        int atd(int d)
        {
            return addTimeDelta(d);
        }

        int addTimeDelta(int delta)
        {
            int fps = (game as AxeGame).FramesPerSecond;
            int baseTime = fps;

            if (times.Count <= 0)
                times.Add(baseTime + delta);
            else
                times.Add(times[times.Count - 1] + delta);

            return times[times.Count - 1];
        }
    }
}
