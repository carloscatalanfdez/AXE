using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace AXE.Game.Entities
{
    class Door : Entity
    {
        public bSpritemap graphic;
        public bSpritemap sign;
        public Vector2 signPosition;
        protected Random random;

        public enum Type { Entry, Exit };
        public Type type;

        public Door(int x, int y, Type type)
            : base(x, y)
        {
            this.type = type;
        }

        public override void init()
        {
            base.init();

            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/door-sheet"), 24, 32);
            graphic.add(new bAnim("closed", new int[] { 0 }));
            graphic.add(new bAnim("open", new int[] { 1 }));
            if (type == Type.Entry)
                graphic.play("closed");
            else
                graphic.play("open");

            mask.w = 8;
            mask.h = 8;
            mask.offsetx = 8;
            mask.offsety = 8;

            if (type == Type.Exit)
            {
                sign = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/sign-sheet"), 32, 24);
                sign.add(new bAnim("idle", new int[] { 0 }));
                sign.add(new bAnim("blink", new int[] { 1, 0 }, 0.5f, false));
                sign.play("idle");

                random = new Random();
                timer[0] = random.Next(60);

                signPosition = new Vector2(x + graphic.width / 2 - sign.width / 2, y - 18);
            }

            layer = 19;
        }

        public override void onTimer(int n)
        {
            if (n == 0)
            {
                sign.play("blink");
                timer[0] = random.Next(60);
            }
        }

        public override void update()
        {
            base.update();

            graphic.update();

            if (type == Type.Exit)
            {
                sign.update();

                if (sign.currentAnim.finished)
                    sign.play("idle");
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos);

            if (type == Type.Exit)
                sign.render(sb, signPosition);
        }
    }
}
