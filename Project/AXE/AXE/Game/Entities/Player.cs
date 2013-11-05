﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;
using bEngine.Helpers.Transitions;

using AXE.Common;
using AXE.Game.Control;
using AXE.Game.Screens;
using AXE.Game.Entities.Base;
using AXE.Game.Utils;

namespace AXE.Game.Entities
{
    class Player : Entity, IWeaponHolder
    {
        // Utilities
        GameInput mginput;
        Random random;

        // Some declarations
        public enum MovementState { Idle, Walk, Jump, Ladder, Death, Attacking, Attacked, Exit };
        public enum ActionState { None, Squid };
        public const int EXIT_ANIM_TIMER = 1;
        public const int EXIT_TRANSITION_TIMER = 2;
        public int exitTransitionWaitTime;
        public int exitAnimationWaitTime;
        // Data
        public PlayerData data;

        // Graphic, movement vars
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }
        public float hspeed;
        public float vspeed, gravity;

        // Accelerated movement
        public float current_hspeed;
        public float haccel;
        public float air_haccel;
        public bool isLanding;
        public float runSpeedFactor;
        public float jumpPower;
        public Dir jumpedFacing;
        public float jumpMaxSpeed;
        public bool falling;

        // State params
        public int deathDelayTime;
        public bool playDeathAnim;
        
        // State vars
        public MovementState previousState;
        public MovementState state;
        public ActionState action;

        // Step variables
        public Vector2 stepInitialPosition;
        public Vector2 moveTo;

        // Weapon holding variables
        protected IWeapon weapon;
        protected HotspotContainer hotspotContainer;
        
        // Sound effects
        protected List<SoundEffect> sfxSteps;
        protected SoundEffect sfxLanded;
        protected SoundEffect sfxCharge;

        // Debug
        String debugText;
        bool floater;

        public Player(int x, int y, PlayerData data)
            : base(x, y)
        {
            this.data = data;
            this.mginput = GameInput.getInstance(data.id);
        }

        override public void init()
        {
            base.init();

            random = new Random();

            mask = new bMask(0, 0, 16, 24, 7, 8);
            mask.game = game;
            attributes.Add("player");
            attributes.Add("moveable");

            spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/knight-sheet"), 30, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0 }, 0.1f));
            spgraphic.add(new bAnim("walk", new int[] { 1, 2, 3, 2 }, 0.2f));
            spgraphic.add(new bAnim("jump", new int[] { 8 }, 0.0f));
            spgraphic.add(new bAnim("death", new int[] { 8 }));
            spgraphic.add(new bAnim("squid", new int[] { 9 }));
            spgraphic.add(new bAnim("fall", new int[] { 8 }, 0.4f));
            spgraphic.add(new bAnim("ladder", new int[] { 10, 11 }, 0.1f));
            spgraphic.add(new bAnim("readyweapon", new int[] { 0, 16, 17, 17, 17 }, 0.5f, false));
            spgraphic.add(new bAnim("air-readyweapon", new int[] { 8, 19, 20, 20, 20 }, 0.6f, false));
            spgraphic.add(new bAnim("thrownweapon", new int[] { 18, 18 }, 0.2f, false));
            spgraphic.add(new bAnim("air-thrownweapon", new int[] { 18, 18 }, 0.2f, false));
            spgraphic.add(new bAnim("exit", new int[] { 4 }));

            hotspotContainer = new HotspotContainer("Assets/SpriteConfs/knight-hotspots.cfg");

            spgraphic.play("idle");
            layer = 0;

            hspeed = 1.5f;
            vspeed = 0f;
            gravity = 0.5f;

            current_hspeed = 0;
            haccel = 0.2f;
            air_haccel = 0f;
            isLanding = false;
            runSpeedFactor = 2;
            jumpPower = 5.5f;
            jumpMaxSpeed = 0.0f;
            deathDelayTime = 0;
            playDeathAnim = false;

            state = MovementState.Idle;
            action = ActionState.None;
            facing = Dir.Right;

            debugText = "";
            floater = false;

            exitTransitionWaitTime = 15;
            exitAnimationWaitTime = 15;

            loadSoundEffects();
        }

        protected void loadSoundEffects()
        {
            sfxSteps = new List<SoundEffect>();
            for (int i = 1; i <= 3; i++)
                sfxSteps.Add(game.Content.Load<SoundEffect>("Assets/Sfx/sfx-step." + i));

            sfxLanded = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-land");
            sfxCharge = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-charge");
        }

        public override int graphicWidth()
        {
            return spgraphic.width;
        }

        public override void onUpdateBegin()
        {
            base.onUpdateBegin();

            previousState = state;
        }

        override public void onUpdate()
        {
            base.onUpdate();

            // Prepare step
            color = Color.White;

            stepInitialPosition = pos;
            moveTo = pos;

            float _hspeed = hspeed;
            float _haccel = haccel;

            // Debug
            handleDebugRoutines();

            handleSoundEffects();

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

                    handleAcceleratedMovement(ref _haccel, ref _hspeed);

                    if (onair)
                    {
                        if (vspeed < 0)
                        {
                            state = MovementState.Jump;
                        }
                        else
                        {
                            state = MovementState.Jump;
                            falling = true;
                        }
                    }
                    else
                    {
                        vspeed = 0f;
                        
                        if (mginput.pressed(PadButton.a))
                        {
                            state = MovementState.Jump;
                            isLanding = false;
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
                                    isLanding = false;
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
                                isLanding = false;
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
                                        timer[EXIT_ANIM_TIMER] = exitAnimationWaitTime;
                                    }
                                }
                            }
                        }
                    }

                    break;
                case MovementState.Ladder:
                    if (onladder)
                    {
                        // Choose one offset (doesn't matter which one, really, both should be good)
                        bMask currentMask = mask;
                        if (currentMask is bMaskList)
                        {
                            int maskXOffset = Math.Max((currentMask as bMaskList).masks[0].offsetx, (currentMask as bMaskList).masks[1].offsetx);
                            moveTo.X = ladder.x - maskXOffset;
                        }
                        else
                        {
                            moveTo.X = ladder.x - mask.offsetx;
                        }

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
                case MovementState.Jump:
                    if (onair)
                    {
                        handleOnAirMovement();
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
                    if (spgraphic.currentAnim.finished)
                    {
                        if (weapon != null)
                        {
                            weapon.onThrow(10, facing);
                            state = MovementState.Attacked;
                            if (!onair)
                                spgraphic.play("thrownweapon");
                            else
                                spgraphic.play("air-thrownweapon");
                        }
                    }
                    else
                    {
                        if (onair)
                        {
                            handleOnAirMovement();
                        }
                        // Go to standing state, acknowleding the case in which
                        // the player is moving through a one way platform
                        else if (vspeed > 0)
                        {
                            // state = MovementState.Idle; // WUT??
                        }
                    }

                    break;
                case MovementState.Attacked:
                    if (spgraphic.currentAnim.finished)
                    {
                        if (!onair)
                            state = MovementState.Idle;
                        else
                            state = MovementState.Jump;
                    }
                    else
                    {
                        {
                            if (onair)
                            {
                                handleOnAirMovement();
                            }
                            // Go to standing state, acknowleding the case in which
                            // the player is moving through a one way platform
                            else if (vspeed > 0)
                            {
                                // state = MovementState.Attacked; // WUT??
                            }
                        }
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
                    isLanding = true;
                    sfxSteps[0].Play();
                    sfxSteps[1].Play();
                }
            }

            // Handle axe
            if (mginput.pressed(PadButton.b))
            {
                if (weapon != null)
                {
                    if (!onair)
                    {
                        if (state != MovementState.Attacking && state != MovementState.Attacked)
                        {
                            state = MovementState.Attacking;
                            spgraphic.play("readyweapon");
                            sfxCharge.Play();
                        }
                    }
                    else
                    {
                        if (state != MovementState.Attacking && state != MovementState.Attacked)
                        {
                            state = MovementState.Attacking;
                            spgraphic.play("air-readyweapon");
                            sfxCharge.Play();
                        }
                        // weapon.onThrow(10, facing);
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
        }

        public override void onUpdateEnd()
        {
            base.onUpdateEnd();

            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    spgraphic.color = Color.White;
                    if (state == MovementState.Idle)
                        spgraphic.play("idle");
                    else if (state == MovementState.Walk)
                        spgraphic.play("walk");

                    if (action == ActionState.Squid)
                        spgraphic.play("squid");

                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
                case MovementState.Jump:
                    spgraphic.color = Color.Red;
                    if (falling)
                        spgraphic.play("fall");
                    else
                        spgraphic.play("jump");
                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
                case MovementState.Ladder:
                    spgraphic.play("ladder");
                    spgraphic.flipped = false;
                    if (moveTo.Y - previousPosition.Y != 0)
                        spgraphic.currentAnim.resume();
                    else
                        spgraphic.currentAnim.pause();

                    break;
                case MovementState.Death:
                    spgraphic.currentAnim.pause();
                    if (playDeathAnim)
                        spgraphic.play("death");
                    break;
                case MovementState.Exit:
                    spgraphic.play("exit");
                    break;
            }

            if (isLanding)
                spgraphic.color = Color.Yellow;


            graphic.color = Color.White;

            spgraphic.update();
        }

        public void handleAcceleratedMovement(ref float _haccel, ref float _hspeed)
        {
            if (mginput.check(PadButton.left))
            {
                // Going right - squid
                if (current_hspeed > 0)
                {
                    action = ActionState.Squid;
                    current_hspeed -= _haccel * 2;
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
                isLanding = false;
            }
            else if (mginput.check(PadButton.right))
            {
                if (current_hspeed < 0)
                {
                    action = ActionState.Squid;
                    current_hspeed += _haccel * 2;
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
                isLanding = false;
            }
            else
            {
                action = ActionState.None;

                moveTo.X += current_hspeed;

                // Decelerate
                float temp_haccel = haccel;
                if (isLanding)
                    temp_haccel = 2 * haccel;
                if (Math.Abs(current_hspeed) > temp_haccel)
                {
                    if (current_hspeed > 0)
                        current_hspeed -= 2 * temp_haccel;
                    else if (current_hspeed < 0)
                        current_hspeed += 2 * temp_haccel;
                }
                else
                {
                    isLanding = false;
                    current_hspeed = 0;
                }
            }   
        }

        public void handleOnAirMovement()
        {
            if (mginput.released(PadButton.a) && vspeed < 0)
                vspeed /= 2;

            if (!floater)
                vspeed += gravity;

            if (mginput.check(PadButton.left))
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
            else if (mginput.check(PadButton.right))
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

            if (vspeed > 0 && state != MovementState.Attacking && state != MovementState.Attacked)
            {
                state = MovementState.Jump;
                falling = true;
            }

            moveTo.X += current_hspeed;
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
                case EXIT_ANIM_TIMER:
                    Door door = (instancePlace(x, y, "items") as Door);
                    if (door != null)
                    {
                        door.spgraphic.play("closed");
                        if (weapon != null)
                            world.remove((weapon as bEntity));
                        visible = false;
                        timer[EXIT_TRANSITION_TIMER] = exitTransitionWaitTime;
                    }
                    break;
                case EXIT_TRANSITION_TIMER:
                    Controller.getInstance().goToNextLevel();
                    break;
            }
        }

        public override void onCollision(string type, bEntity other)
        {
            if (state == MovementState.Death)
                return;

            if (type == "solid")
                color = Color.Turquoise;

            if (type == "enemy")
            {
                // First, reposition
                pos.X = stepInitialPosition.X;
                if (placeMeeting(x, y, "enemy"))
                {
                    // If it doesn't work (still colliding) jump or something
                    // Check first if the enemy is on top, so to not jump throw it
                    if (vspeed < 0 && other.y + (other as Entity).graphicHeight() / 2 < y)
                    {
                        if (!handleJumpHit())
                        {
                            vspeed = 0;
                            pos.Y = stepInitialPosition.Y;
                        }
                    }
                    else
                    {
                        state = MovementState.Jump;
                        vspeed = -jumpPower / 2;
                        if (other.x + (other as Entity).graphicWidth()/2 < x + graphicWidth()/2)
                        {
                            jumpedFacing = Dir.Right;
                            facing = Dir.Left;
                            current_hspeed = getDirectionAsSign(Dir.Right) * hspeed;
                        }
                        else if (other.x + (other as Entity).graphicWidth() / 2 > x + graphicWidth() / 2)
                        {
                            jumpedFacing = Dir.Left;
                            facing = Dir.Right;
                            current_hspeed = getDirectionAsSign(Dir.Left) * hspeed;
                        }
                    }
                }
            }
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
            spgraphic.render(sb, pos);
            Color c = spgraphic.color;
            spgraphic.color = c;

            sb.DrawString(game.gameFont, debugText, new Vector2(x, y - 8), Colors.white);
        }

        public bool isFlipped()
        {
            return spgraphic.flipped;
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

            int curFrame = spgraphic.currentAnim.frame;

            // Right and Left direction hotspots for the current frame
            if (!hotspotContainer.hotspots.ContainsKey(curFrame))
            {
                // default value
                curFrame = -1;
            }
            Vector2[] hotspotsPoints = hotspotContainer.hotspots[curFrame];

            if (facing == Dir.Right)
            {
                hand += hotspotsPoints[0];
            }
            else
            {
                hand += hotspotsPoints[1];
            }

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
        /* End of IWeaponHolder implementation */

        bool playedStepEffect = false;
        public void handleSoundEffects()
        {
            float relativeX = pos.X / (world as LevelScreen).width - 0.5f;
            switch (state)
            {
                case MovementState.Walk:
                    int currentFrame = spgraphic.currentAnim.frame;
                    if (currentFrame == 2 && !playedStepEffect)
                    {
                        playedStepEffect = true;
                        sfxSteps[random.Next(sfxSteps.Count)].Play(1f, 0.0f, relativeX);
                    }
                    else if (currentFrame != 2)
                        playedStepEffect = false;

                    break;
                default:
                    break;
            }
        }

        void handleDebugRoutines()
        {
        }
    }
}
