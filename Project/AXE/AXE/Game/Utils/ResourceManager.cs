using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

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
            effect = contentManager.Load<Effect>(resourcesPath + "/scanlines");
            badladnsBanner = contentManager.Load<Texture2D>(resourcesPath + "/badladns_banner");

            // Spritesheets
            sprCursor = loadSprite("cursor");

            sprKnightASheet = loadSprite("knight-sheet");
            sprKnightBSheet = loadSprite("knight-sheet-alt");
            sprAxeSheet     = loadSprite("axe-sheet");
            sprStickSheet   = loadSprite("stick-sheet");
            
            sprCoinSheet    = loadSprite("coin-sheet");
            sprHighfallGuardSheet 
                            = loadSprite("highfallguard-sheet");

            sprLeverSheet   = loadSprite("lever-sheet");
            sprTrapdoorSheet= loadSprite("trapdoor-sheet");
            sprDoorSheet    = loadSprite("door-sheet");
            sprSignSheet    = loadSprite("sign-sheet");

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

            // Tilesets
            tsetBasic       = loadTileset("basic");

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
        }

        // Hardcoding resources for now to ease development
        // Misc
        public Effect effect;
        public Texture2D badladnsBanner;

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
        public Texture2D sprDoorSheet;
        public Texture2D sprSignSheet;

        public Texture2D sprSlimeSheet;
        public Texture2D sprAxeThrowerSheet;
        public Texture2D sprFlamewrathSheet;
        public Texture2D sprFlameSheet;
        public Texture2D sprFlameBulletSheet;
        public Texture2D sprImpSheet;
        public Texture2D sprZombieSheet;

        // Tileset
        public Texture2D tsetBasic;

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
    }
}
