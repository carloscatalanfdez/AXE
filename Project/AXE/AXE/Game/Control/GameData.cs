using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Control;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml.Serialization;

namespace AXE.Game.Control
{
    public class GameData
    {
        public static GameData get()
        {
            return Controller.getInstance().data;
        }

        // Declare here game data
        public int level;
        public int maxLevels;
        public int credits;
        // Meta
        GameDataStruct state;
        public int coins
        {
            get { return state.coins; }
            set { state.coins = value; }
        }

        public PlayerData playerAData;
        public PlayerData playerBData;

        public GameData()
        {
            playerAData = new PlayerData(PlayerIndex.One);
            playerBData = new PlayerData(PlayerIndex.Two);
        }

        public void startNewGame()
        {
            coins = 10;
        }

        public void initPlayData()
        {
            maxLevels = 2;
            // credits = 0;

            // DEBUG
            level = 0;
        }

        public static void saveGame()
        {
            IAsyncResult r = StorageDevice.BeginShowSelector(Microsoft.Xna.Framework.PlayerIndex.One, null, null);
            r.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(r);

            IAsyncResult result = device.BeginOpenContainer("AXE_DATA", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename = "axesave.sav";
            if (container.FileExists(filename))
                container.DeleteFile(filename);
            Stream stream = container.CreateFile(filename);

            XmlSerializer serializer = new XmlSerializer(typeof(GameDataStruct));
            serializer.Serialize(stream, GameData.get().state);
            stream.Close();
            container.Dispose();
        }

        public static bool loadGame()
        {
            IAsyncResult r = StorageDevice.BeginShowSelector(Microsoft.Xna.Framework.PlayerIndex.One, null, null);
            r.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(r);

            IAsyncResult result = device.BeginOpenContainer("AXE_DATA", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename = "axesave.sav";
            if (!container.FileExists(filename))
            {
                container.Dispose();
                return false;
            }

            Stream stream = container.OpenFile(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(GameDataStruct));
            GameDataStruct tempState = (GameDataStruct)serializer.Deserialize(stream);
            stream.Close();
            container.Dispose();

            // Load Actual Coins
            GameData.get().state = tempState;

            return true;
        }
    }

    public struct GameDataStruct
    {
        public int coins;
    }
}
