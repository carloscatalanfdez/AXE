using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Screens;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities
{
    class Axe : Entity
    {
        public enum MovementState { Idle, Grabbed, Flying, Bouncing };

        public MovementState state;
        public Player.Dir dir;

        public bSpritemap graphic;

        public Player holder;

        public float current_hspeed;
        public float current_vspeed;

        public Axe(int x, int y, Player holder) : base(x, y)
        {
            this.holder = holder;  // can be null
        }

        public Axe(int x, int y)
            : this(x, y, null)
        {
        }

        public override void init()
        {
            base.init();

            mask = new bMask(0, 0, 14, 14);
            mask.game = game;
            attributes.Add("axe");

            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/axe-sheet"), 14, 14);
            int[] fs = { 0 };
            graphic.add(new bAnim("grabbed", fs, 0.0f));
            int[] fss = { 4, 5, 6, 7 };
            graphic.add(new bAnim("cw-rotation", fss, 0.0f));
            int[] fsss = { 8, 9, 10, 11 };
            graphic.add(new bAnim("ccw-rotation", fsss, 0.0f));
            int[] fssss = { 12 };
            graphic.add(new bAnim("idle", fssss, 0.0f));

            current_hspeed = current_vspeed = 0;

            if (holder == null)
            {
                graphic.play("idle");
                dir = Player.Dir.None;
                state = MovementState.Idle;
            }
            else
            {
                graphic.play("grabbed");
                dir = holder.facing;
                state = MovementState.Grabbed;
            }
        }

        public void onGrab(Player holder)
        {
            holder.axe = this;
            graphic.play("grabbed");
            dir = holder.facing;
            state = MovementState.Grabbed;
            this.holder = holder;
        }

        public override int graphicWidth()
        {
            return 14;
        }

        public virtual void onThrow(int force, Player.Dir dir)
        {
            state = MovementState.Flying;
            current_hspeed = force * holder.directionToSign(dir);
            holder.axe = null;
            holder = null;
        }

        override public void update()
        {
            if ((world as LevelScreen).isPaused())
                return;

            // Prepare step
            Vector2 moveTo = pos;

            switch (state)
            {
                case MovementState.Grabbed:
                    pos = holder.pos;
                    break;
                case MovementState.Flying:
                    moveTo.X += current_hspeed;
                    Vector2 remnant;
                    // Check wether we collide first with a solid or a onewaysolid,
                    // and use that data to position the player character.
                    Vector2 oldPos = pos;
                    Vector2 remnantOneWay = moveToContact(moveTo, "solid");
                    Vector2 posOneWay = pos;
                    pos = oldPos;
                    remnant = moveToContact(moveTo, "solid");
                    Vector2 posSolid = pos;

                    // We have been stopped
                    if (remnant.X != 0)
                    {
                        // Stop accelerating if we have stopped
                        current_hspeed = 0;
                        state = MovementState.Bouncing;
                    }
                    break;
                default:
                    break;
            }

            switch (state)
            {
                case MovementState.Idle:
                    graphic.play("idle");
                    break;
                case MovementState.Grabbed:
                    graphic.play("grabbed");
                    break;
                case MovementState.Flying:
                case MovementState.Bouncing:
                    if (dir == Player.Dir.Left)
                    {
                        graphic.play("ccw-rotation");
                    }
                    else
                    {
                        graphic.play("cw-rotation");
                    }
                    
                    break;
            }

            base.update();
            graphic.update();
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.render(sb, pos);
        }
    }
}
