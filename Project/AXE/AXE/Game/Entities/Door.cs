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
    class Door : Entity, IRewarder
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

        public Door(int x, int y, Type type)
            : base(x, y)
        {
            this.type = type;
        }

        public override void init()
        {
            base.init();

            mask.w = 8;
            mask.h = 24;
            mask.offsetx = 8;
            mask.offsety = 8;

            spgraphic = new bSpritemap((game as AxeGame).res.sprDoorSheet, 24, 32);
            spgraphic.add(new bAnim("closed", new int[] { 0 }));
            spgraphic.add(new bAnim("open", new int[] { 1 }));
            if (type == Type.Entry || type == Type.ExitClose)
                spgraphic.play("closed");
            else
                spgraphic.play("open");

            if (isExit())
            {
                sign = new bSpritemap((game as AxeGame).res.sprSignSheet, 32, 24);
                sign.add(new bAnim("idle", new int[] { 0 }));
                sign.add(new bAnim("blink", new int[] { 1, 0 }, 0.5f, false));
                sign.play("idle");

                random = new Random();
                timer[0] = random.Next(60);

                signPosition = new Vector2(x + spgraphic.width / 2 - sign.width / 2, y - 18);
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

        public bool onPlayerExit()
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
}
