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
using AXE.Game.Entities.Base;

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
        // Exit
        public int playersThatLeft;

        // Screen text
        public string stageLabel;
        public string infoLabel;
        PlayerDisplay[] playerDisplays;

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

            cursor = new bStamp((game as AxeGame).res.sprCursor);
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

            /*background = new bStamp("HOHO!");
            background.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);*/

            // Add player
            if (id < Controller.getInstance().data.maxLevels)
            {
                foreach (PlayerData pdata in GameData.get().playerData)
                {
                    spawnPlayer(pdata);
                }
            }
            else
            {
                // Handle ending/nonsense
            }

            // Add loaded entities
            handleEntities(levelMap.entities);

            // Start
            camera = new bCamera2d(game.GraphicsDevice);
            camera.bounds = new Rectangle(levelMap.x, levelMap.y, levelMap.tilemap.width, levelMap.tilemap.height);
   
            state = State.Gameplay;

            renderQueue = new List<bEntity>();

            playerDisplays = new PlayerDisplay[] { 
                new PlayerDisplay(PlayerIndex.One, GameData.get().playerData[0], playerA),
                new PlayerDisplay(PlayerIndex.Two, GameData.get().playerData[1], playerB)
            };

            for (int i = 0; i < playerDisplays.Length; i++)
            {
                playerDisplays[i].world = this;
                playerDisplays[i].game = game;
                playerDisplays[i].init();
            }

            playersThatLeft = 0;
        }

        public Player spawnPlayer(PlayerData pdata)
        {
            Player player = null;
            // TODO: add logic for positioning and multiplayer
            if (pdata.playing && pdata.alive)
            {
                int playerX = (int)levelMap.playerStart.X;
                int playerY = (int)levelMap.playerStart.Y;

                if (pdata.id == PlayerIndex.One)
                {
                    // Removing the old player prevents glitches
                    // but also removes the corpse and is less funny
                    if (playerA != null)
                        remove(playerA);
                    player = new Player(playerX, playerY, GameData.get().playerData[0]);
                    playerA = player;
                    // Adding axe based on GameData
                    spawnPlayerWeapon(playerA.data, playerA);
                    _add(playerA, "player");
                }
                else if (pdata.id == PlayerIndex.Two)
                {
                    // Removing the old player prevents glitches
                    // but also removes the corpse and is less funny
                    if (playerB != null)
                        remove(playerB);
                    player = new Player(playerX + 32, playerY, GameData.get().playerData[1]);
                    playerB = player;
                    // Adding axe based on GameData
                    spawnPlayerWeapon(playerB.data, playerB);
                    _add(playerB, "player");
                }
            }

            return player;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

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
                    foreach (bEntity c in entities["items"])
                        if (p != c && p.collides(c))
                        {
                            c.onCollision("player", p);
                            p.onCollision("items", c);
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

                    foreach (bEntity e in entities["axe"])
                        if (w != e && w.collides(e))
                        {
                            // Check first, since it depends on the state of the axe and the
                            // first onCollision will set the other axe to bounce, hence it won't
                            // be flying on the next on collision and things won't work
                            if (axeToAxeCollisionCondition(w as Axe, e as Axe)) 
                            {
                                e.onCollision("axe", w);
                                w.onCollision("axe", e);
                            }
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

            for (int i = 0; i < playerDisplays.Length; i++)
                playerDisplays[i].update();

            stageLabel = buildStageLabel();
            infoLabel = "CREDITS: " + (GameData.get().credits) + " - COINS: " + (GameData.get().coins + " ( " + Controller.getInstance().activePlayers + ")");
        }

        public string buildStageLabel()
        {
            string number = "" + (id + 1);
            while (number.Length < 2)
                number = " " + number;
            string label = "STAGE " + number;
            return label;
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);

            //matrix *= camera.get_transformation();

            /*sb.End();

            sb.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    (game as AxeGame).effect,
                    matrix);*/

            // Render bg
            sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Color.Black);
            if (background != null)
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

            for (int i = 0; i < playerDisplays.Length; i++)
                playerDisplays[i].render(dt, sb);
            sb.DrawString(game.gameFont, stageLabel, new Vector2(game.getWidth() / 2 - stageLabel.Length * 8 / 2, 0), Color.White);
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
                else if (e is Item)
                    _add(e, "items");
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

        public bool axeToAxeCollisionCondition(Axe a, Axe b)
        {
            return a.state == Axe.MovementState.Flying && b.state == Axe.MovementState.Flying;
        }

        public void displayPlayerCountdown(PlayerIndex who)
        {
            if (who == PlayerIndex.One)
            {
                playerDisplays[0].startTimer();
            }
            else
                playerDisplays[1].startTimer();
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
