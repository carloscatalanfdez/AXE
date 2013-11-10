using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;

using AXE.Common;
using AXE.Game.Entities;
using AXE.Game.Control;
using AXE.Game.Entities.Axes;
using AXE.Game.Entities.Contraptions;

namespace AXE.Game.Screens
{
    class LevelScreen : Screen
    {
        // Some declarations...
        public enum State { Enter, Gameplay, Exit };

        // Render ordering
        List<bEntity> renderQueue;

        // Management
        public int id;
        public String name;
        bool paused;

        public State state;

        // Level elements
        public int width { get { return levelMap.tilemap.width; } }
        public int height { get { return levelMap.tilemap.height; } }
        public bCamera2d camera;
        LevelMap levelMap;
        protected bStamp background;

        // Players
        public Player playerA, playerB;
        public Player[] players { get { return new Player[] {playerA, playerB}; } }

        // Screen text
        public string stageLabel;
        public string player1Label;
        public string player2Label;
        public string infoLabel;

        public const int PLAYER_TIMER_DURATION = 10;
        public const int PLAYER_TIMER_STEPSPERSECOND = 30;
        public int player1Timer;
        public int player2Timer;

        // Debug
        // String msg;
        public bStamp cursor;
        
        public LevelScreen(int id, int lastCheckpoint = -1)
            : base()
        {
            this.id = id;
            usesCamera = true;
        }

        public override void init()
        {
            base.init();

            paused = false;

            cursor = new bStamp(game.Content.Load<Texture2D>("Assets/Sprites/cursor"));
            state = State.Enter;

            // Init entity collections
            entities.Add("solid", new List<bEntity>());
            entities.Add("onewaysolid", new List<bEntity>());
            entities.Add("items", new List<bEntity>());
            entities.Add("player", new List<bEntity>());
            entities.Add("axe", new List<bEntity>());
            entities.Add("hazard", new List<bEntity>());
            entities.Add("enemy", new List<bEntity>());
            entities.Add("stairs", new List<bEntity>());
            entities.Add("coins", new List<bEntity>());
            entities.Add("contraptions", new List<bEntity>());
            entities.Add("rewarders", new List<bEntity>());

            // Load level
            if (id < Controller.getInstance().data.maxLevels)
            {
                String fname = id.ToString();
                levelMap = new LevelMap(fname);
                _add(levelMap, "solid"); // Adding to world performs init & loading
                name = levelMap.name;
            }
            else
            {
                // Handle ending/nonsense

                // Dummy floor for now
                bEntity ground = new bEntity(0, 246);
                ground.mask = new bMask(0, 0, 300, 20);
                ground.mask.game = game;
                _add(ground, "solid");

                Stairs stairs = new Stairs(0, 206, 20, 40);
                _add(stairs, "stairs");

                // Dummy map
                levelMap = new LevelMap(null);
                levelMap.tilemap = new bTilemap(400, 256, 8, 8, bDummyRect.sharedDummyRect(game));
                // Do not add it because there's no file and it will break
            }

            background = new bStamp(game.Content.Load<Texture2D>("Assets/Backgrounds/bg"));
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            // Add player
            if (id < Controller.getInstance().data.maxLevels)
            {
                // TODO: add logic for positioning and multiplayer
                int playerX = (int)levelMap.playerStart.X;
                int playerY = (int)levelMap.playerStart.Y;
                playerA = new Player(playerX, playerY, GameData.get().playerAData);
                
                // Adding axe based on GameData
                spawnPlayerWeapon(playerA.data, playerA);
                _add(playerA, "player");
            }
            else
            {
                // Handle ending/nonsense
                playerA = new Player(width / 2 - 30, height / 2, GameData.get().playerAData);
                _add(playerA, "player");
            }

            // Add loaded entities
            handleEntities(levelMap.entities);

            // Start
            camera = new bCamera2d(game.GraphicsDevice);
            camera.bounds = new Rectangle(levelMap.x, levelMap.y, levelMap.tilemap.width, levelMap.tilemap.height);
   
            state = State.Gameplay;

            renderQueue = new List<bEntity>();

            player1Timer = -1;
            player2Timer = -1;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            /*if (GameData.get().level < GameData.get().maxLevels)
                if (GameInput.getInstance(PlayerIndex.One).pressed(PadButton.start))
                {
                    handlePause();
                }*/

            foreach (String key in entities.Keys)
                foreach (bEntity entity in entities[key])
                    entity.update();

            // Collisions
            if (!paused && state == State.Gameplay)
            {
                foreach (bEntity p in entities["player"])
                {
                    foreach (bEntity e in entities["enemy"])
                        if (p != e && p.collides(e))
                        {
                            e.onCollision("player", p);
                            p.onCollision("enemy", e);
                        }
                    foreach (bEntity i in entities["items"])
                        if (p != i && p.collides(i))
                        {
                            p.onCollision("items", i);
                            i.onCollision("player", p);
                        }
                    foreach (bEntity h in entities["hazard"])
                        if (p != h && p.collides(h))
                        {
                            h.onCollision("player", p);
                            p.onCollision("hazard", h);
                        }
                    foreach (bEntity c in entities["coins"])
                        if (p != c && p.collides(c))
                        {
                            c.onCollision("player", p);
                            p.onCollision("coins", c);
                        }
                    foreach (bEntity a in entities["axe"])
                        if (p != a && p.collides(a))
                        {
                            a.onCollision("player", p);
                            p.onCollision("axe", a);
                        }
                }

                foreach (bEntity w in entities["axe"])
                {
                    foreach (bEntity e in entities["enemy"])
                        if (w.collides(e))
                        {
                            e.onCollision("axe", w);
                            w.onCollision("enemy", e);
                        }
                }
                    
            }
            
            // Update camera
            if (id >= GameData.get().maxLevels)
            {
                // Handle ending/nonsense
            }
            else
            {
                // Camera follows player?
            }
            
            // Debug: R for restart
            if (bGame.input.pressed(Microsoft.Xna.Framework.Input.Keys.R))
                game.changeWorld(new LevelScreen(id));
            if (bGame.input.pressed(Microsoft.Xna.Framework.Input.Keys.N))
                Controller.getInstance().goToNextLevel();

            if (GameData.get().playerAData.playing)
            {
                if (GameData.get().playerAData.alive)
                    player1Label = "1UP";
                else
                {
                    player1Timer--;
                    player1Label = "CONTINUE? " + (player1Timer / PLAYER_TIMER_STEPSPERSECOND*1f);
                    if (player1Timer < 0)
                        Controller.getInstance().handleCountdownEnd(playerA.data.id);
                    else if (Controller.getInstance().playerAInput.pressed(PadButton.start))
                    {
                        if (Controller.getInstance().playerStart(PlayerIndex.One))
                            playerA.revive();
                    }
                }
            }
            else
            {
                player1Label = "1P PRESS START";
            }

            if (GameData.get().playerBData.playing)
            {
                if (GameData.get().playerBData.alive)
                    player2Label = "2UP";
                else
                {
                    player2Timer--;
                    player2Label = "CONTINUE? " + (player2Timer / PLAYER_TIMER_STEPSPERSECOND * 1f);
                    if (player2Timer < 0)
                        Controller.getInstance().handleCountdownEnd(playerB.data.id);
                    else if (Controller.getInstance().playerBInput.pressed(PadButton.start))
                    {
                        if (Controller.getInstance().playerStart(PlayerIndex.Two))
                            playerB.revive();
                    }
                }
            }
            else
            {
                player2Label = "2P PRESS START";
            }

            stageLabel = "STAGE " + (id + 1);
            infoLabel = "CREDITS: " + (GameData.get().credits) + " - COINS: " + (GameData.get().coins);
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);

            //matrix *= camera.get_transformation();

            sb.End();

            sb.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    null,
                    matrix);

            // Render bg
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            background.render(sb, Vector2.Zero);

            // Render entities
            renderQueue.Clear();
            foreach (String key in entities.Keys)
                foreach (bEntity entity in entities[key])
                    if (!(entity is Entity) || (entity is Entity) && ((entity as Entity).visible))
                        renderQueue.Add(entity);

            renderQueue.Sort((a, b) => (b.layer - a.layer));
            foreach (bEntity entity in renderQueue)
                entity.render(dt, sb);

            cursor.render(sb, bGame.input.mousePosition);

            sb.DrawString(game.gameFont, player1Label, new Vector2(0, 0), Color.White);
            sb.DrawString(game.gameFont, stageLabel, new Vector2(game.getWidth() / 2 - stageLabel.Length * 8 / 2, 0), Color.White);
            sb.DrawString(game.gameFont, player2Label, new Vector2(width-player2Label.Length * 8, 0), Color.White);
            sb.DrawString(game.gameFont, infoLabel, new Vector2(game.getWidth()/2-infoLabel.Length*8/2, game.getHeight()-8), Color.White);

            // Pause!
            if (paused)
            {
                // Pause render
                sb.DrawString(game.gameFont, "PAUSE", new Vector2(game.getWidth() / 2 - ("PAUSE".Length * 8) / 2), Color.White);
            }
        }

        protected override bool _add(bEntity e, string category)
        {
            switch (category)
            {
                case "player":
                    entities["player"].Add(e);
                    Player player = (e as Player);
                    if (player.data.id == 0)
                        playerA = player;
                    else
                        playerB = player;
                    break;
                case "enemy":
                    entities["enemy"].Add(e);
                    break;
                case "solid":
                    entities["solid"].Add(e);
                    break;
                default:
                    if (entities.ContainsKey(category))
                    {
                        entities[category].Add(e);
                        return base._add(e, category);
                    }
                    else
                        return false;
            }

            return base._add(e, category);
        }

        public bool isPaused()
        {
            return paused;
        }

        public void handlePause()
        {
            paused = !paused;
        }

        public void handleEntities(List<bEntity> list)
        {
            foreach (bEntity e in list)
            {
                if (e == null)
                    continue;
                else if (e is OneWayPlatform)
                    _add(e, "onewaysolid");
                else if (e is Stairs)
                    _add(e, "stairs");
                else if (e is Door)
                    _add(e, "items");
                else if (e is AXE.Game.Entities.Base.Enemy)
                    _add(e, "enemy");
                else if (e is Coin)
                    _add(e, "coins");
                else if (e is TrapDoor)
                    _add(e, "solid");
                else if (e is Lever)
                    _add(e, "contraptions");
                // Contraptions and rewarders may be added before, but if they are not,
                // we'll add them to these categories
                else if (e is IContraption)
                    _add(e, "contraptions");
                else if (e is IRewarder)
                    _add(e, "rewarders");
            }
        }

        public override bool isInstanceInView(bEntity e)
        {
            Rectangle viewRect = camera.viewRectangle;
            viewRect.Inflate(viewRect.Width / 4, viewRect.Height / 4);

            return viewRect.Intersects(e.mask.rect);
        }

        public int layerSelector(bEntity a, bEntity b)
        {
            return a.layer - b.layer;
        }

        public void displayPlayerCountdown(PlayerIndex who)
        {
            if (who == PlayerIndex.One)
                player1Timer = PLAYER_TIMER_DURATION * PLAYER_TIMER_STEPSPERSECOND;
            else
                player2Timer = PLAYER_TIMER_DURATION * PLAYER_TIMER_STEPSPERSECOND;
        }

        public void spawnPlayerWeapon(PlayerData data, Player player)
        {
            Axe currentWeapon = null;
            switch (data.weapon)
            {
                case PlayerData.Weapons.None:
                    break;
                case PlayerData.Weapons.Stick:
                    currentWeapon = new Axe(player.x, player.y, player);
                    break;
                case PlayerData.Weapons.Axe:
                    currentWeapon = new NormalAxe(player.x, player.y, player);
                    break;
            }
            if (currentWeapon != null)
            {
                player.setWeapon(currentWeapon);
                _add(currentWeapon, "axe");
            }
        }
    }
}
