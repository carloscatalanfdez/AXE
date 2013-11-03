using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;

using AXE.Game.Screens;
using AXE.Game.Entities;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities
{
    class Axe : Entity, IWeapon
    {
        public enum MovementState { Idle, Grabbed, Flying, Bouncing };

        public MovementState state;
        public Player.Dir dir;

        public bSpritemap graphic;

        public IWeaponHolder holder;

        public float current_hspeed;
        public float current_vspeed;
        public float gravity;

        public SoundEffect sfxThrow;
        public SoundEffect sfxHit;
        public SoundEffect sfxDrop;
        public SoundEffect sfxGrab;

        public Axe(int x, int y, IWeaponHolder holder) : base(x, y)
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
            graphic.add(new bAnim("cw-rotation", fss, 0.7f));
            int[] fsss = { 8, 9, 10, 11 };
            graphic.add(new bAnim("ccw-rotation", fsss, 0.7f));
            int[] fssss = { 12 };
            graphic.add(new bAnim("idle", fssss, 0.0f));

            current_hspeed = current_vspeed = 0;
            gravity = 0.5f;

            layer = 10;

            if (holder == null)
            {
                graphic.play("idle");
                dir = Player.Dir.None;
                state = MovementState.Idle;
            }
            else
            {
                graphic.play("grabbed");
                dir = holder.getFacing();
                state = MovementState.Grabbed;
            }

            sfxThrow = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-thrown");
            sfxHit = game.Content.Load<SoundEffect>("Assets/Sfx/axe-hit");
            sfxDrop = game.Content.Load<SoundEffect>("Assets/Sfx/axe-drop");
            sfxGrab = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-grab");
        }

        public override int graphicWidth()
        {
            return 14;
        }

        override public void onUpdate()
        {
            base.onUpdate();

            // Prepare step
            Vector2 moveTo = pos;

            if (holder != null)
                dir = holder.getFacing();

            switch (state)
            {
                case MovementState.Grabbed:
                    pos = holder.getHandPosition() - getGrabPosition();
                    break;
                case MovementState.Flying:
                    moveTo.X += current_hspeed;
                    Vector2 remnant = moveToContact(moveTo, "solid");

                    // We have been stopped
                    if (remnant.X != 0)
                    {
                        // Stop accelerating if we have stopped
                        current_hspeed = - current_hspeed / 10;
                        current_vspeed = -2;
                        state = MovementState.Bouncing;
                        sfxHit.Play();
                    }
                    break;
                case MovementState.Bouncing:
                    current_vspeed += gravity;

                    moveTo.Y += current_vspeed;
                    moveTo.X += current_hspeed;
                    Vector2 oldPos = pos;
                    Vector2 remnantOneWay = moveToContact(moveTo, "onewaysolid", onewaysolidCondition);
                    Vector2 posOneWay = pos;
                    pos = oldPos;
                    Vector2 remnantSolid = moveToContact(moveTo, "solid");
                    Vector2 posSolid = pos;
                    if (remnantOneWay.Length() > remnantSolid.Length())
                    {
                        remnant = remnantOneWay;
                        pos = posOneWay;
                    }
                    else
                    {
                        remnant = remnantSolid;
                        pos = posSolid;
                    }

                    if (remnant.Y != 0 && current_vspeed < 0)
                    {
                        // Touched ceiling
                        current_vspeed = 0;
                    }
                    else if (remnant.Y != 0 && current_vspeed > 0)
                    {
                        current_vspeed = 0;
                        state = MovementState.Idle;
                        sfxDrop.Play();
                    }

                    break;
                default:
                    break;
            }
        }

        public override void onUpdateEnd()
        {
            base.onUpdateEnd();

            switch (state)
            {
                case MovementState.Idle:
                    graphic.play("idle");
                    if (dir == Player.Dir.Left)
                        graphic.flipped = true;
                    else if (dir == Player.Dir.Right)
                        graphic.flipped = false;

                    break;
                case MovementState.Grabbed:
                    graphic.play("grabbed");
                    if (dir == Player.Dir.Left)
                        graphic.flipped = true;
                    else if (dir == Player.Dir.Right)
                        graphic.flipped = false;

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

            graphic.update();
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.render(sb, pos);
        }

        /* IWeapon implementation */
        public Vector2 getGrabPosition()
        {
            return new Vector2(5, 6);
        }

        public void onGrab(IWeaponHolder holder)
        {
            sfxGrab.Play();
            holder.setWeapon(this);
            graphic.play("grabbed");
            dir = holder.getFacing();
            state = MovementState.Grabbed;
            this.holder = holder;
        }

        public virtual void onThrow(int force, Player.Dir dir)
        {
            sfxThrow.Play();
            state = MovementState.Flying;
            current_hspeed = force * holder.getDirectionAsSign(dir);
            holder.removeWeapon();
            holder = null;
        }
    }
}
