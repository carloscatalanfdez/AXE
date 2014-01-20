using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using AXE.Game.Entities.Contraptions;
using AXE.Game.Screens;
using AXE.Game.Control;

namespace AXE.Game.Entities
{
    class ExitDoor : Entity, IRewarder
    {
        public const int EXIT_TRANSITION_TIMER = 2;
        public int exitTransitionWaitTime;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }
        public bSpritemap sign;
        public Vector2 signPosition;
        protected Random random;

        public enum Type { Entry, ExitOpen, ExitClose };
        public Type type;
        public Lock myLock;

        public ExitDoor(int x, int y, Type type)
            : base(x, y)
        {
            // No open doors? 
            /*if (type == Type.ExitOpen)
                type = Type.ExitClose;*/
            this.type = type;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprExitDoorSheet;
            if (sign != null)
                sign.image = (game as AxeGame).res.sprSignSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 8;
            mask.h = 24;
            mask.offsetx = 8;
            mask.offsety = 8;

            spgraphic = new bSpritemap((game as AxeGame).res.sprExitDoorSheet, 24, 32);
            spgraphic.add(new bAnim("closed", new int[] { 0 }));
            spgraphic.add(new bAnim("open", new int[] { 1 }));
            if (type == Type.Entry || type == Type.ExitClose)
                spgraphic.play("closed");
            else
                spgraphic.play("open");

            myLock = null;
            if (isExit())
            {
                sign = new bSpritemap((game as AxeGame).res.sprSignSheet, 32, 24);
                sign.add(new bAnim("idle", new int[] { 0 }));
                sign.add(new bAnim("blink", new int[] { 1, 0 }, 0.5f, false));
                sign.play("idle");

                random = new Random();
                timer[0] = random.Next(60);

                signPosition = new Vector2(x + spgraphic.width / 2 - sign.width / 2, y - 18);

                if (!isOpen())
                {
                    myLock = new Lock(x+12, y+15);
                    world.add(myLock, "items");
                }
            }

            exitTransitionWaitTime = 15;

            layer = 19;
        }

        public override void onTimer(int n)
        {
            if (n == 0)
            {
                sign.play("blink");
                timer[0] = random.Next(60);
            }
            else if (n == EXIT_TRANSITION_TIMER)
            {
                Controller.getInstance().goToNextLevel();
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();
            spgraphic.update();

            if (isExit())
            {
                sign.update();

                if (sign.currentAnim.finished)
                    sign.play("idle");
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            spgraphic.render(sb, pos);

            if (isExit())
                sign.render(sb, signPosition);

            base.render(dt, sb);
        }

        public bool tryOpen(Player player)
        {
            if (player.data.keys > 0)
            {
                player.data.keys--;
                if (myLock != null)
                {
                    open();
                    myLock.open();
                }
            }

            return true;
        }

        public bool onPlayerExit(Player player)
        {
            (world as LevelScreen).playersThatLeft++;
            if ((world as LevelScreen).playersThatLeft >= Controller.getInstance().activePlayers)
            {
                spgraphic.play("closed");
                timer[EXIT_TRANSITION_TIMER] = exitTransitionWaitTime;
            }

            return true;
        }

        public void onReward(IContraption contraption)
        {
            if (isOpen())
            {
                close();
            }
            else
            {
                open();
            }
        }

        public void open()
        {
            spgraphic.play("open");
            type = Type.ExitOpen;
        }

        public void close()
        {
            spgraphic.play("closed");
            type = Type.ExitClose;
        }

        public bool isOpen()
        {
            return type == Type.ExitOpen;
        }

        public bool isExit()
        {
            return type == Type.ExitOpen || type == Type.ExitClose;
        }

        public override void onClick()
        {
            if (isExit())
                onReward(null);
        }
    }

    class Lock : Entity
    {
        protected bSpritemap sprite
        {
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        protected bool opened;
        protected float vspeed;

        public Lock(int x, int y) : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            sprite = new bSpritemap(Game.res.sprLockSheet, 11, 13);
            sprite.add(new bAnim("closed", new int[] { 0 }));
            sprite.add(new bAnim("open", new int[] { 1 }));
            sprite.play("closed");

            opened = false;
            
            layer = 18;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprLockSheet;
        }

        public override void onUpdate()
        {
            base.onUpdate();

            if (opened)
            {
                x += 2;
                pos.Y += vspeed;
                vspeed += 0.5f;

                if (timer[0] >= 0 && timer[0] < 5)
                {
                    visible = !visible;
                }
            }

            sprite.update();
        }

        public void open()
        {
            opened = true;
            Game.res.sfxUnlock.Play();
            vspeed = -2;
            setTimer(0, 8, 15);
        }

        public override void onTimer(int n)
        {
            if (n == 0)
            {
                world.remove(this);
                visible = false;
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            sprite.render(sb, pos);
        }
    }

    class Key : Entity
    {
        bSpritemap sprite
        {
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        public bool collected;

        public Key(int x, int y)
            : base(x, y)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprKeySheet;
        }

        public override void init()
        {
            base.init();

            sprite = new bSpritemap(Game.res.sprKeySheet, 10, 11);
            sprite.add(new bAnim("idle", new int[] { 0 }));
            sprite.play("idle");

            mask.w = 10;
            mask.h = 11;

            collected = false;
        }

        public override void update()
        {
            base.update();

            sprite.update();
        }

        public override void onCollision(string type, bEngine.bEntity other)
        {
            if (type == "player")
            {
                onCollected(other as Player);
            }
        }

        public void onCollected(Entity other)
        {
            if (collected)
                return;

            if (other is Player)
            {
                PlayerData data = (other as Player).data;
                if (data.keys < 9)
                {
                    data.keys++;
                    Game.res.sfxKeyA.Play();
                    collected = true;
                    world.remove(this);
                }
            }
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }
    }
}
