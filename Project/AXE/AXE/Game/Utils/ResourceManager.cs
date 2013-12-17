using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace AXE.Game.Utils
{
    public class ResourceManager
    {
        protected static ResourceManager instance = null;
        public static ResourceManager get()
        {
            if (instance == null)
                instance = new ResourceManager();
            return instance;
        }

        bool _ready;
        public bool IsReady
        {
            get { return _ready; }
        }

        AxeGame game;
        ContentManager contentManager;

        string resourcesPath = "Assets";

        string backgroundsPath = "Backgrounds";
        string tilesetsPath = "Tilesets";
        string spritesPath = "Sprites";
        string sfxPath = "Sfx";
        string musicPath = "Music";

        public ResourceManager()
        {
            _ready = false;
        }

        public void init(AxeGame game)
        {
            this.game = game;
            contentManager = game.Content;

            _ready = true;
        }

        protected Song loadMusic(string fname)
        {
            return contentManager.Load<Song>(resourcesPath + "/" + musicPath + "/" + fname);
        }

        protected SoundEffect loadSfx(string fname)
        {
            return contentManager.Load<SoundEffect>(resourcesPath + "/" + sfxPath + "/" + fname);
        }

        protected Texture2D loadSprite(string fname)
        {
            return contentManager.Load<Texture2D>(resourcesPath + "/" + spritesPath + "/" + fname);
        }

        protected Texture2D loadBackground(string fname)
        {
            return contentManager.Load<Texture2D>(resourcesPath + "/" + backgroundsPath + "/" + fname);
        }

        protected Texture2D loadTileset(string fname)
        {
            return contentManager.Load<Texture2D>(resourcesPath + "/" + tilesetsPath + "/" + fname);
        }

        public void loadContent()
        {
            // Misc
            effect          = contentManager.Load<Effect>(resourcesPath + "/scanlines");
            badladnsBanner  = contentManager.Load<Texture2D>(resourcesPath + "/badladns_banner");
            sprTotalLine    = loadSprite("total-line");

            // Spritesheets
            sprCursor = loadSprite("cursor");

                // Player
            sprKnightASheet = loadSprite("knight-sheet");
            sprKnightBSheet = loadSprite("knight-sheet-alt");

                // Weapons
            sprAxeSheet     = loadSprite("axe-sheet");
            sprStickSheet   = loadSprite("stick-sheet");
            
                // Items
            sprCoinSheet    = loadSprite("coin-sheet");
            sprHighfallGuardSheet 
                            = loadSprite("highfallguard-sheet");

                // Contraptions
            sprLeverSheet   = loadSprite("lever-sheet");
            sprTrapdoorSheet= loadSprite("trapdoor-sheet");
            sprExitDoorSheet= loadSprite("door-sheet");
            sprSignSheet    = loadSprite("sign-sheet");
            sprDoorSheet    = loadSprite("doors-sheet");
            sprLocksSheet   = loadSprite("locks-sheet");
            sprKeysSheet    = loadSprite("keys-sheet");

                // Enemies
            sprSlimeSheet   = loadSprite("corrosiveslime-sheet");
            sprAxeThrowerSheet 
                            = loadSprite("axethrower-sheet");
            sprFlamewrathSheet 
                            = loadSprite("flamewrath-sheet");
            sprFlameSheet   = loadSprite("flame-sheet");
            sprFlameBulletSheet 
                            = loadSprite("flamewrath-bullet-sheet");
            sprImpSheet     = loadSprite("imp-sheet");
            sprZombieSheet  = loadSprite("zombie-sheet");
            sprDaggerSheet = loadSprite("dagger-sheet");

            sprDragonBossSheet = loadSprite("dragonboss-sheet");
            sprFireBulletSheet = loadSprite("fire-bullet-sheet");
            sprVFireBulletSheet = loadSprite("vfire-bullet-sheet");
            sprGargoyleSheet = loadSprite("gargoyle-sheet");
            sprVGargoyleSheet = loadSprite("vgargoyle-sheet");

            // Tilesets
            tsetBasic       = loadTileset("basic");

            // Backgrounds
            bgTest          = loadBackground("bg");

            // Sfx
            sfxStepA        = loadSfx("sfx-step.1");
            sfxStepB        = loadSfx("sfx-step.2");
            sfxStepC        = loadSfx("sfx-step.3");
            sfxLanded       = loadSfx("sfx-land");
            sfxCharge       = loadSfx("sfx-charge");
            sfxPlayerHit    = loadSfx("sfx-playerhit");
            sfxThrow        = loadSfx("sfx-thrown");
            sfxHit          = loadSfx("axe-hit");
            sfxDrop         = loadSfx("axe-drop");
            sfxGrab         = loadSfx("sfx-grab");
            sfxHurt         = loadSfx("sfx-hurt");

            sfxEvilPick     = loadSfx("sfx-evilpick");
            sfxEvilThrow    = loadSfx("sfx-evilthrow");
            sfxDirtstepA    = loadSfx("sfx-dirtstep.1");
            sfxDirtstepB    = loadSfx("sfx-dirtstep.2");
            sfxDirtstepC    = loadSfx("sfx-dirtstep.3");

            sfxOpenDoor     = loadSfx("sfx-opendoor");
            sfxCloseDoor    = loadSfx("sfx-closedoor");
            sfxUnlock       = loadSfx("sfx-unlock");
            sfxKeyA         = loadSfx("sfx-gotkey.1");
            sfxKeyB         = loadSfx("sfx-gotkey.2");
            sfxKeyC         = loadSfx("sfx-gotkey.3");

            sfxGreatBell    = loadSfx("sfx-bell");
            sfxBigBell      = loadSfx("sfx-bell.1");
            sfxMidBell      = loadSfx("sfx-bell.2");

            sfxDragonShriek = loadSfx("dragon-shriek");

            ostGameOver = loadMusic("ost-gameover");
            ostDungeon = loadMusic("ost-dungeon");
            ostDungeonBoss = loadMusic("ost-dungeon-boss");
        }

        public Song getSong(String name)
        {
            switch (name)
            {
                case "dungeon-boss":
                    return ostDungeonBoss;
                case "dungeon":
                default:
                    return ostDungeon;
            }
        }

        // Hardcoding resources for now to ease development
        // Misc
        public Effect effect;
        public Texture2D badladnsBanner;
        public Texture2D sprTotalLine;

        // Spritesheets
        public Texture2D sprCursor;

        public Texture2D sprKnightASheet;
        public Texture2D sprKnightBSheet;

        public Texture2D sprAxeSheet;
        public Texture2D sprStickSheet;

        public Texture2D sprCoinSheet;
        public Texture2D sprHighfallGuardSheet;

        public Texture2D sprLeverSheet;
        public Texture2D sprTrapdoorSheet;
        public Texture2D sprExitDoorSheet;
        public Texture2D sprSignSheet;
        public Texture2D sprDoorSheet;
        public Texture2D sprLocksSheet;
        public Texture2D sprKeysSheet;

        public Texture2D sprSlimeSheet;
        public Texture2D sprAxeThrowerSheet;
        public Texture2D sprFlamewrathSheet;
        public Texture2D sprFlameSheet;
        public Texture2D sprFlameBulletSheet;
        public Texture2D sprGargoyleSheet;
        public Texture2D sprImpSheet;
        public Texture2D sprZombieSheet;
        public Texture2D sprDaggerSheet;
        public Texture2D sprDragonBossSheet;
        public Texture2D sprFireBulletSheet;
        public Texture2D sprVFireBulletSheet;
        public Texture2D sprVGargoyleSheet;
        // Tileset
        public Texture2D tsetBasic;

        // Background
        public Texture2D bgTest;

        // Sfx
        public SoundEffect sfxStepA;
        public SoundEffect sfxStepB;
        public SoundEffect sfxStepC;
        public SoundEffect sfxLanded;
        public SoundEffect sfxCharge;
        public SoundEffect sfxPlayerHit;
        public SoundEffect sfxThrow;
        public SoundEffect sfxHit;
        public SoundEffect sfxDrop;
        public SoundEffect sfxGrab;
        public SoundEffect sfxHurt;

        public SoundEffect sfxEvilPick;
        public SoundEffect sfxEvilThrow;
        public SoundEffect sfxDirtstepA;
        public SoundEffect sfxDirtstepB;
        public SoundEffect sfxDirtstepC;

        public SoundEffect sfxOpenDoor;
        public SoundEffect sfxCloseDoor;
        public SoundEffect sfxUnlock;
        public SoundEffect sfxKeyA;
        public SoundEffect sfxKeyB;
        public SoundEffect sfxKeyC;

        public SoundEffect sfxGreatBell;
        public SoundEffect sfxBigBell;
        public SoundEffect sfxMidBell;

        public SoundEffect sfxDragonShriek;

        // Music
        public Song ostGameOver;
        public Song ostDungeon;
        public Song ostDungeonBoss;
    }
}
