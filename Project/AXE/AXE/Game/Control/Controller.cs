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

        Controller()
        {
            data = new GameData();
        }

        public void setGame(AxeGame game)
        {
            this.game = game;
        }

        public void onMenuStart()
        {
            game.changeWorld(new TitleScreen());
        }

        public void onGameStart()
        {
            // init game data here
            GameData.get().init();

            // Go to first screen
            game.changeWorld(new LevelScreen(data.level), new FadeToColor(game, Colors.clear, 10));
        }

        public void onGameEnd()
        {
        }

        public void onGameOver()
        {
        }

        public void onGameWin()
        {
        }

        public int goToNextLevel()
        {
            // Handle level progression
            data.level += 1;
            if (data.level > data.maxLevels)
                onGameWin();
            else
                game.changeWorld(new LevelScreen(data.level), new FadeToColor(game, Colors.clear, 10));
            return data.level;
        }

        public void handlePlayerDeath()
        {
        }
    }
}
