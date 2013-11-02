﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;
using bEngine.Helpers.Transitions;

using AXE.Common;
using AXE.Game.Control;
using AXE.Game.Screens;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities
{
    class Player : Entity, IWeaponHolder
    {
        // Utilities
        GameInput mginput = GameInput.getInstance();

        // Some declarations
        public enum MovementState { Idle, Walk, Jump, Fall, Ladder, Death, Attacking, Attacked, Exit };
        public enum ActionState { None, Squid }
        public enum Dir { None, Left, Right };

        // Data
        public PlayerData data;

        // Graphic, movement vars
        public bSpritemap graphic;
        public float hspeed;
        public float vspeed, gravity;
        public Dir showWrapEffect;

        // Accelerated movement
        public float current_hspeed;
        public float haccel;
        public float air_haccel;
        public bool justLanded;
        public float runSpeedFactor;
        public float jumpPower;
        public Dir jumpedFacing;
        public float jumpMaxSpeed;

        // State params
        public int deathDelayTime;
        public bool playDeathAnim;
        
        // State vars
        public MovementState state;
        public ActionState action;
        public Dir facing;

        // Step variables
        public Vector2 initialPosition;
        public Vector2 moveTo;

        protected IWeapon weapon;
        protected int attackAnimationPositionCorrection
        {
            get { return -3; }
        }
        public bool appliedPositionCorrection;
        public Vector2 graphicPositionCorrection;

        // Debug
        String debugText;
        bool floater;

        public Player(int x, int y, PlayerData data)
            : base(x, y)
        {
            this.data = data;
        }

        override public void init()
        {
            base.init();

            mask = new bMask(0, 0, 16, 24, 4, 8);
            mask.game = game;
            attributes.Add("player");
            attributes.Add("moveable");

            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/knight-sheet"), 24, 32);
            graphic.add(new bAnim("idle", new int[] { 0 }, 0.1f));
            graphic.add(new bAnim("walk", new int[] { 1, 2, 3, 2 }, 0.2f));
            graphic.add(new bAnim("jump", new int[] { 8 }, 0.0f));
            graphic.add(new bAnim("death", new int[] { 8 }));
            graphic.add(new bAnim("squid", new int[] { 9 }));
            graphic.add(new bAnim("fall", new int[] { 8 }, 0.4f));
            graphic.add(new bAnim("ladder", new int[] { 10, 11 }, 0.1f));
            graphic.add(new bAnim("readyweapon", new int[] { 0, 16, 17, 17, 17 }, 0.5f, false));
            graphic.add(new bAnim("thrownweapon", new int[] { 18, 18 }, 0.2f, false));
            graphic.add(new bAnim("exit", new int[] { 4 }));

            graphic.play("idle");
            layer = 0;

            showWrapEffect = Dir.None;

            hspeed = 1.5f;
            vspeed = 0f;
            gravity = 0.5f;

            current_hspeed = 0;
            haccel = 0.2f;
            air_haccel = 0f;
            justLanded = false;
            runSpeedFactor = 2;
            jumpPower = 5.5f;
            jumpMaxSpeed = 0.0f;
            deathDelayTime = 0;
            playDeathAnim = false;
            appliedPositionCorrection = false;
            graphicPositionCorrection = Vector2.Zero;

            state = MovementState.Idle;
            action = ActionState.None;
            facing = Dir.Right;

            debugText = "";
            floater = false;
        }

        public int directionToSign(Dir dir) {
            if (dir == Dir.Left)
            {
                return -1;
            }
            else if (dir == Dir.Right)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override int graphicWidth()
        {
            return graphic.width;
        }

        override public void update()
        {
            if (isPaused())
                return;

            // Prepare step
            color = Color.White;

            initialPosition = pos;
            moveTo = pos;

            float _hspeed = hspeed;
            float _haccel = haccel;

            // Debug
            handleDebugRoutines();

            // Check for outside playfield death
            if (state != MovementState.Death && y + mask.h / 2 > (world as LevelScreen).height)
            {
                onDeath("fall");
            }

            Stairs ladder = (Stairs) instancePlace(x, y, "stairs");
            bool onladder = ladder != null;
            bool toLadder = false;

            bool onair = !placeMeeting(x, y + 1, "solid");
            if (onair)
                onair = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);
            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    state = MovementState.Idle;

                    if (mginput.check(Pad.left))
                    {
                        // Going right - squid
                        if (current_hspeed > 0)
                        {
                            action = ActionState.Squid;
                            current_hspeed -= _haccel*2;
                        }
                        else
                        {
                            action = ActionState.None;
                            current_hspeed = Math.Max(current_hspeed - _haccel, -_hspeed);
                        }
                        moveTo.X += current_hspeed;
                        facing = Dir.Left;
                        state = MovementState.Walk;
                        // If has started moving, hasn't just landed
                        justLanded = false;
                    }
                    else if (mginput.check(Pad.right))
                    {
                        if (current_hspeed < 0)
                        {
                            action = ActionState.Squid;
                            current_hspeed += _haccel*2;
                        }
                        else
                        {
                            action = ActionState.None;
                            current_hspeed = Math.Min(current_hspeed + _haccel, _hspeed);
                        }
                        moveTo.X += current_hspeed;
                        facing = Dir.Right;
                        state = MovementState.Walk;
                        // If has started moving, hasn't just landed
                        justLanded = false;
                    }
                    else
                    {
                        action = ActionState.None;

                        moveTo.X += current_hspeed;

                        // Decelerate
                        float temp_haccel = haccel;
                        if (justLanded)
                            temp_haccel = 2 * haccel;
                        if (Math.Abs(current_hspeed) > temp_haccel)
                        {
                            if (current_hspeed > 0)
                                current_hspeed -= 2*temp_haccel;
                            else if (current_hspeed < 0)
                                current_hspeed += 2*temp_haccel;
                        }
                        else
                        {
                            justLanded = false;
                            current_hspeed = 0;
                        }
                    }   

                    if (onair)
                    {
                        if (vspeed < 0)
                        {
                            state = MovementState.Jump;
                        }
                        else
                        {
                            state = MovementState.Fall;
                        }
                    }
                    else
                    {
                        vspeed = 0f;
                        
                        if (mginput.pressed(Pad.a))
                        {
                            state = MovementState.Jump;
                            justLanded = false;
                            jumpedFacing = facing;
                            jumpMaxSpeed = Math.Max(Math.Abs(current_hspeed)*1.5f, 3);
                            bool runningJump = Math.Abs(current_hspeed) > hspeed;
                            current_hspeed = Math.Sign(current_hspeed)*
                                Math.Min(Math.Abs(current_hspeed * 1.25f), jumpMaxSpeed);
                            vspeed = -jumpPower;
                            if (runningJump)
                                vspeed -= jumpPower / 8;
                            if (action == ActionState.Squid)
                                vspeed -= jumpPower / 8 * 2;
                        }

                        // Ladders
                        if (action == ActionState.None)
                        {
                            if (onladder)
                            {
                                if (input.up() || input.down() && (!input.left() && !input.right()))
                                {
                                    state = MovementState.Ladder;
                                    justLanded = false;
                                    current_hspeed = 0;
                                }
                            }
                            else if (input.down() && placeMeeting(x, y + 1, "stairs"))
                            {
                                moveTo.Y += 2;
                                bEntity g = (bEntity)instancePlace(moveTo, "stairs");
                                if (g != null)
                                    moveTo.X = g.x;
                                toLadder = true;
                                state = MovementState.Ladder;
                                justLanded = false;
                                current_hspeed = 0;
                            }
                            else if (input.up() && placeMeeting(x, y, "items"))
                            {
                                bEntity door = (bEntity)instancePlace(x, y, "items");
                                if (door is Door)
                                {
                                    Door exitDoor = (door as Door);
                                    if (exitDoor.type == Door.Type.Exit)
                                    {
                                        state = MovementState.Exit;
                                        timer[1] = 15;
                                    }
                                }
                            }
                        }
                    }

                    break;
                case MovementState.Ladder:
                    if (onladder)
                    {
                        moveTo.X = ladder.x - mask.offsetx;

                        if (input.up())
                            moveTo.Y -= hspeed;
                        else if (input.down())
                            moveTo.Y += hspeed;
                        if ((input.left() || input.right()) && !(input.down() || input.up()))
                            if (placeMeeting(x, y + 1, "solid") || placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition))
                                state = MovementState.Idle;
                    }
                    else
                    {
                        state = MovementState.Jump;
                        if (input.up())
                            vspeed = -2.0f;
                        else
                            vspeed = 0;
                    }
                    break;
                case MovementState.Fall:
                case MovementState.Jump:
                    if (onair)
                    {
                        if (mginput.released(Pad.a) && vspeed < 0)
                            vspeed /= 2;

                        if (!floater)
                            vspeed += gravity;

                        if (mginput.check(Pad.left))
                        {
                            // Going right - squid
                            if (current_hspeed > 0)
                            {
                                action = ActionState.Squid;
                                current_hspeed -= air_haccel;
                                if (jumpedFacing == Dir.Right)
                                    current_hspeed = Math.Max(current_hspeed, 0);
                            }
                            else
                            {
                                action = ActionState.None;
                                current_hspeed = Math.Max(current_hspeed - air_haccel, -jumpMaxSpeed);
                            }
                            facing = Dir.Left;
                        }
                        else if (mginput.check(Pad.right))
                        {
                            if (current_hspeed < 0)
                            {
                                action = ActionState.Squid;
                                current_hspeed += air_haccel/* * 2*/;
                                if (jumpedFacing == Dir.Left)
                                    current_hspeed = Math.Min(current_hspeed, 0);
                            }
                            else
                            {
                                action = ActionState.None;
                                current_hspeed = Math.Min(current_hspeed + air_haccel, jumpMaxSpeed);
                            }
                            facing = Dir.Right;
                        }

                        if (vspeed > 0)
                        {
                            state = MovementState.Fall;
                        }

                        moveTo.X += current_hspeed;
                    }
                    // Go to standing state, acknowleding the case in which
                    // the player is moving through a one way platform
                    else if (vspeed >= 0)
                    {
                        state = MovementState.Idle;
                    }
                    break;
                case MovementState.Death:

                    bool deathAnimationFinished = true; // TODO
                    if (deathAnimationFinished)
                    {
                        // Animation end, restart...
                        Controller.getInstance().handlePlayerDeath();
                    }

                    break;
                case MovementState.Attacking:
                    if (!appliedPositionCorrection)
                    {
                        // moveTo.X += attackAnimationPositionCorrection * getDirectionAsSign(facing);
                        graphicPositionCorrection.X = attackAnimationPositionCorrection * getDirectionAsSign(facing);
                        appliedPositionCorrection = true;
                    }

                    if (graphic.currentAnim.finished)
                    {
                        if (weapon != null)
                        {
                            weapon.onThrow(10, facing);
                            state = MovementState.Attacked;
                            graphic.play("thrownweapon");
                        }
                    }
                    break;
                case MovementState.Attacked:
                    if (graphic.currentAnim.finished)
                    {
                        // moveTo.X -= attackAnimationPositionCorrection * getDirectionAsSign(facing);
                        // graphicPositionCorrection.X = -attackAnimationPositionCorrection * getDirectionAsSign(facing);
                        graphicPositionCorrection = Vector2.Zero;
                        appliedPositionCorrection = false;
                        state = MovementState.Idle;
                    }

                    break;
            }

            moveTo.Y += vspeed;

            if (state == MovementState.Death || toLadder)
                pos = moveTo;
            else
            {
                Vector2 remnant;
                // Check wether we collide first with a solid or a onewaysolid,
                // and use that data to position the player character.
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

                // We have been stopped
                if (remnant.X != 0)
                {
                    // Stop accelerating if we have stopped
                    current_hspeed = 0;
                }

                // Wrap (effect)
                if (x < 0)
                    showWrapEffect = Dir.Right;
                else if (x + (graphic.width) > (world as LevelScreen).width)
                    showWrapEffect = Dir.Left;
                else
                    showWrapEffect = Dir.None;


                // Wrap (mechanic)
                /*if (x + (graphic.width) / 2 < 0)
                    x = (world as LevelScreen).width - (graphic.width/2);
                else if (x + (graphic.width) / 2 > (world as LevelScreen).width)
                    x = -(graphic.width)/2;*/

                // The y movement was stopped
                if (remnant.Y != 0 && vspeed < 0)
                {
                    // Touched ceiling
                    if (!handleJumpHit())
                        vspeed = 0;
                }
                else if (remnant.Y != 0 && vspeed > 0)
                {
                    // Landed
                    justLanded = true;
                }
            }

            // Handle axe
            if (mginput.pressed(Pad.b))
            {
                if (weapon != null)
                {
                    if (!onair)
                    {
                        if (state != MovementState.Attacking && state != MovementState.Attacked)
                        {
                            state = MovementState.Attacking;
                            graphic.play("readyweapon");
                        }
                    }
                    else
                    {
                        weapon.onThrow(10, facing);
                    }
                }
                else
                {
                    if (state != MovementState.Attacking && state != MovementState.Attacked)
                    {
                        bEntity entity = instancePlace(pos, "axe");
                        if (entity != null)
                        {
                            (entity as Axe).onGrab(this);
                        }
                    }
                }
            }

            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    graphic.color = Color.White;
                    if (state == MovementState.Idle)
                        graphic.play("idle");
                    else if (state == MovementState.Walk)
                        graphic.play("walk");

                    if (action == ActionState.Squid)
                        graphic.play("squid");

                    if (facing == Dir.Right)
                        graphic.flipped = false;
                    else
                        graphic.flipped = true;
                    break;
                case MovementState.Jump:
                    graphic.color = Color.Red;
                    graphic.play("jump");
                    if (facing == Dir.Right)
                        graphic.flipped = false;
                    else
                        graphic.flipped = true;
                    break;
                case MovementState.Fall:
                    graphic.color = Color.Red;
                    graphic.play("fall");
                    if (facing == Dir.Right)
                        graphic.flipped = false;
                    else
                        graphic.flipped = true;
                    break;
                case MovementState.Ladder:
                    graphic.play("ladder");
                    graphic.flipped = false;
                    if (moveTo.Y - initialPosition.Y != 0)
                        graphic.currentAnim.resume();
                    else
                        graphic.currentAnim.pause();

                    break;
                case MovementState.Death:
                    graphic.currentAnim.pause();
                    if (playDeathAnim)
                        graphic.play("death");
                    break;
                case MovementState.Exit:
                    graphic.play("exit");
                    break;
            }

            if (justLanded)
                graphic.color = Color.Yellow;


            graphic.color = Color.White;

            base.update();
            graphic.update();
        }

        public static bool onewaysolidCondition(bEntity me, bEntity other)
        {
            if (me is Player)
            {
                Player p = me as Player;
                return (p.initialPosition.Y + p.mask.offsety + me.mask.h <= other.mask.y);
            }
            else
                return true;
        }

        // Return true if handled, else handled by flow
        public bool handleJumpHit()
        {
            return false;
        }

        public override void onTimer(int n)
        {
            switch (n)
            {
                case 0:
                    state = MovementState.Death;
                    playDeathAnim = true;
                    collidable = false;
                    vspeed = -jumpPower;
                    break;
                case 1:
                    Door door = (instancePlace(x, y, "items") as Door);
                    if (door != null)
                    {
                        door.graphic.play("closed");
                        if (weapon != null)
                            world.remove((weapon as bEntity));
                        visible = false;
                    }
                    break;
            }
        }

        public override void onCollision(string type, bEntity other)
        {
            if (state == MovementState.Death)
                return;

            if (type == "solid")
                color = Color.Yellow;
        }

        public void onDeath(String type = "")
        {
            if (state != MovementState.Death)
            {
                // TODO: Use type to change anim?
                state = MovementState.Death;
            }
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.render(sb, pos + graphicPositionCorrection);
            Color c = graphic.color;
            if (showWrapEffect == Dir.Left)
            {
                //graphic.color = Color.Aqua;
                graphic.render(sb, new Vector2(0 + (pos.X - (world as LevelScreen).width), pos.Y) + graphicPositionCorrection);
            }
            else if (showWrapEffect == Dir.Right)
            {
                //graphic.color = Color.Aqua;
                graphic.render(sb, new Vector2((world as LevelScreen).width + pos.X, pos.Y) + graphicPositionCorrection);
            }
            graphic.color = c;

            sb.DrawString(game.gameFont, debugText, new Vector2(x, y - 8), Colors.white);
        }

        public bool isFlipped()
        {
            return graphic.flipped;
        }

        public bool isPaused()
        {
            return (world as LevelScreen).isPaused();
        }

        /* IWeaponHolder implementation */
        public void setWeapon(IWeapon weapon)
        {
            this.weapon = weapon;
        }

        public void removeWeapon()
        {
            this.weapon = null;
        }

        public Vector2 getPosition()
        {
            return pos;
        }

        public Vector2 getHandPosition()
        {
            Vector2 hand = new Vector2(x, y);

            hand.Y += 21;
            if (facing == Dir.Right)
                hand.X += 19;
            else if (facing == Dir.Left)
                hand.X += 2;

            switch (graphic.currentAnim.frame)
            {
                case 1:
                    if (facing == Dir.Right)
                        hand.X = x + 15;
                    else
                        hand.X = x + 5;
                    hand.Y = y + 22;
                break;
                case 2:
                    if (facing == Dir.Right)
                        hand.X = x + 18;
                    else
                        hand.X = x + 1;
                    hand.Y = y + 21;
                break;
                case 3:
                    if (facing == Dir.Right)
                        hand.X = x + 19;
                    else
                        hand.X = x + 0;
                    hand.Y = y + 18;
                break;
                case 16:
                    if (facing == Dir.Right)
                        hand.X = x + 17;
                    else
                        hand.X = x + 3;
                    hand.Y = y + 11;
                break;
                case 17:
                    if (facing == Dir.Right)
                        hand.X = x;
                    else
                        hand.X = x + 20;
                    hand.Y = y + 10;
                    
                break;
                case 18:
                    // No weapon in this frame :D
                break;
            }

            hand += graphicPositionCorrection;

            return hand;
        }

        public Dir getFacing()
        {
            return facing;
        }

        /** 20131102, RDLH: This should not be in the interface! **/
        public int getDirectionAsSign(Dir dir)
        {
            return directionToSign(dir);
        }

        void handleDebugRoutines()
        {
        }
    }
}
