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

namespace AXE.Game.Screens
{
    class LevelScreen : Screen
    {
        // Some declarations...
        public enum State { Enter, Gameplay, Exit };

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

        // Debug
        String msg;
        
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

            state = State.Enter;

            // Init entity collections
            entities.Add("solid", new List<bEntity>());
            entities.Add("onewaysolid", new List<bEntity>());
            entities.Add("items", new List<bEntity>());
            entities.Add("player", new List<bEntity>());
            entities.Add("enemy", new List<bEntity>());
            entities.Add("stairs", new List<bEntity>());
            
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

                background = new bStamp(game.Content.Load<Texture2D>("Assets/Backgrounds/bg"));
            }


            // Add player
            if (id < Controller.getInstance().data.maxLevels)
            {
                // TODO: add logic for positioning and multiplayer
                int playerX = (int)levelMap.playerStart.X;
                int playerY = (int)levelMap.playerStart.Y;
                _add(new Player(playerX, playerY, GameData.get().playerAData), "player");
            }
            else
            {
                // Handle ending/nonsense
                _add(new Player(width / 2 - 30, height / 2, GameData.get().playerAData), "player");
            }

            // Add loaded entities
            handleEntities(levelMap.entities);

            // Start
            camera = new bCamera2d(game.GraphicsDevice);
            camera.bounds = new Rectangle(levelMap.x, levelMap.y, levelMap.tilemap.width, levelMap.tilemap.height);
   
            state = State.Gameplay;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (GameData.get().level < GameData.get().maxLevels)
                if (GameInput.getInstance().pressed(Pad.start))
                {
                    handlePause();
                }

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
            // sb.Draw(bDummyRect.sharedDummyRect(game), game.getViewRectangle(), Colors.white);
            background.render(sb, Vector2.Zero);

            // Render entities
            foreach (String key in entities.Keys)
                foreach (bEntity entity in entities[key])
                    entity.render(dt, sb);

            // Pause!
            if (paused)
            {
                // Pause render
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
                // Add entities with its categories here
                //else if (e is EntityCategory)
                //    _add(e, "category");
            }
        }

        public override bool isInstanceInView(bEntity e)
        {
            Rectangle viewRect = camera.viewRectangle;
            viewRect.Inflate(viewRect.Width / 4, viewRect.Height / 4);

            return viewRect.Intersects(e.mask.rect);
        }
    }
}
