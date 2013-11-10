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
        public GameInput playerAInput;
        public GameInput playerBInput;

        Controller()
        {
            data = new GameData();
            playerAInput = new GameInput(PlayerIndex.One);
            playerBInput = new GameInput(PlayerIndex.Two);
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
                data.playerAData.alive = true;
            if (data.playerBData.playing)
                data.playerBData.alive = true;

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
                game.changeWorld(new GameOverScreen());
            else
                ; // Someone is still alive, so do nothing
        }

        /** Returns true if valid start press **/
        public bool playerStart(PlayerIndex who)
        {
            if (GameData.get().credits > 0)
            {
                GameData.get().credits--;
                if (who == PlayerIndex.One)
                {
                    GameData.get().playerAData.playing = true;
                    GameData.get().playerAData.alive = true;
                }
                else if (who == PlayerIndex.Two)
                {
                    GameData.get().playerBData.playing = true;
                    GameData.get().playerBData.alive = true;
                }

                return true;
            }

            return false;
        }
    }
}
