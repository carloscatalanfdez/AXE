using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using bEngine;
using bEngine.Helpers;
using bEngine.Helpers.Transitions;

using AXE.Common;
using AXE.Game;
using AXE.Game.Screens;
using Microsoft.Xna.Framework;

namespace AXE.Game.Control
{
    class Controller
    {
        static Controller _instance;
        public static Controller getInstance()
        {
            if (_instance == null)
                _instance = new Controller();
            return _instance;
        }

        AxeGame game;
        public GameData data;

        public int activePlayers;
        public GameInput[] playerInput;
        public GameInput playerAInput
        {
            get { return playerInput[0]; }
        }
        public GameInput playerBInput
        {
            get { return playerInput[1]; }
        }

        Controller()
        {
            data = new GameData();
            playerInput = new GameInput[] {
                new GameInput(PlayerIndex.One),
                new GameInput(PlayerIndex.Two)
            };
        }

        public void setGame(AxeGame game)
        {
            this.game = game;
        }

        public void onMenuStart()
        {
            // init game data here
            if (!GameData.loadGame())
                GameData.get().startNewGame();
            game.changeWorld(new TitleScreen(), new FadeToColor(game, Color.Black));
        }

        public void changePlayerButtonConf(PlayerIndex index, Dictionary<PadButton, List<Object>> mappingConf)
        {
            GameInput.getInstance(index).setMapping(mappingConf);
            // store to disk maybe?
        }

        public void onGameStart()
        {
            GameData data = GameData.get();
            // Init data
            data.initPlayData();
            
            // Set alive the playing characters
            if (data.playerAData.playing)
            {
                data.playerAData.alive = true;
            }
            if (data.playerBData.playing)
            {
                data.playerBData.alive = true;
            }

            // Go to first screen
            game.changeWorld(new LevelScreen(data.level), new FadeToColor(game, Color.Black, 10));
        }

        public void onGameEnd()
        {
        }

        public void onGameOver()
        {
        }

        public void onGameWin()
        {
            game.changeWorld(new WinScreen(), new FadeToColor(game, Color.Gray, 15));
        }

        public int goToNextLevel()
        {
            GameData.saveGame();

            // Handle level progression
            data.level += 1;
            if (data.level >= data.maxLevels)
                onGameWin();
            else
                game.changeWorld(new LevelScreen(data.level), new FadeToColor(game, Colors.clear, 10));
            return data.level;
        }

        public void handlePlayerDeath(PlayerData who)
        {
            who.alive = false;
            (game.world as LevelScreen).displayPlayerCountdown(who.id);
        }

        public void handleCountdownEnd(PlayerIndex who)
        {
            if (who == PlayerIndex.One)
                data.playerAData.playing = false;
            else if (who == PlayerIndex.Two)
                data.playerBData.playing = false;

            activePlayers--;
            if (activePlayers <= 0)
                game.changeWorld(new GameOverScreen(), new FadeToColor(game, Color.Black, 120));
        }

        /** Returns true if valid start press **/
        public bool playerStart(PlayerIndex who)
        {
            if (GameData.get().credits > 0)
            {
                GameData.get().credits--;
                PlayerData pdata;
                if (who == PlayerIndex.One)
                    pdata = GameData.get().playerAData;
                else if (who == PlayerIndex.Two)
                    pdata = GameData.get().playerBData;
                else
                    return false;

                if (!pdata.playing)
                {
                    // Give new axe on game start
                    if (pdata.weapon == PlayerData.Weapons.None)
                        pdata.weapon = PlayerData.Weapons.Axe;

                    pdata.playing = true;
                    activePlayers++;
                }
                pdata.alive = true;
                
                return true;
            }

            return false;
        }

        public bool canSwitchFullscreen()
        {
            return (game.world is TitleScreen);
        }
    }
}
