using System;
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

namespace AXE.Game.Entities
{
    class Player : bEntity
    {
        // Utilities
        GameInput mginput = GameInput.getInstance();

        // Some declarations
        public enum MovementState { Idle, Walk, Jump, Fall, Death };
        public enum ActionState { None, Squid }
        public enum Dir { Left, Right };

        // Data
        public PlayerData data;

        // Graphic, movement vars
        public bSpritemap graphic;
        public float hspeed;
        public float vspeed, gravity;

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

            mask = new bMask(0, 0, 8, 24, 4, 8);
            mask.game = game;
            attributes.Add("player");
            attributes.Add("moveable");

            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/player"), 17, 26);
            int[] fs = { 0 };
            graphic.add(new bAnim("idle", fs, 0.1f));
            int[] fss = { 0, 1, 0, 2 };
            graphic.add(new bAnim("walk", fss, 0.5f));
            int[] fsss = { 2 };
            graphic.add(new bAnim("jump", fsss, 0.0f));
            int[] fssss = { 2 };
            graphic.add(new bAnim("death", fssss));
            int[] fsssss = { 1 };
            graphic.add(new bAnim("squid", fsssss));
            int[] fssssss = { 0,2,0,1 };
            graphic.add(new bAnim("fall", fssssss, 0.4f));

            graphic.play("idle");

            hspeed = 1.5f;
            vspeed = 0f;
            gravity = 0.5f;

            current_hspeed = 0;
            haccel = 0.2f;
            air_haccel = 0f;
            justLanded = false;
            runSpeedFactor = 2;
            jumpPower = 6.0f;
            jumpMaxSpeed = 0.0f;
            deathDelayTime = 0;
            playDeathAnim = false;

            state = MovementState.Idle;
            action = ActionState.None;
            facing = Dir.Right;

            debugText = "";
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
                        else if (mginput.pressed(Pad.b))
                        {
                            
                        }
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
                                current_hspeed += air_haccel * 2;
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
            }

            moveTo.Y += vspeed;

            if (state == MovementState.Death)
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

                // Dont' cross the border, scum!
                x = Math.Min(Math.Max(x, 0), (world as LevelScreen).width - graphic.width);

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
                        graphic.flipped = true;
                    else
                        graphic.flipped = false;
                    break;
                case MovementState.Jump:
                    graphic.color = Color.Red;
                    graphic.play("jump");
                    if (facing == Dir.Right)
                        graphic.flipped = true;
                    else
                        graphic.flipped = false;
                    break;
                case MovementState.Fall:
                    graphic.color = Color.Red;
                    graphic.play("fall");
                    if (facing == Dir.Right)
                        graphic.flipped = true;
                    else
                        graphic.flipped = false;
                    break;
                case MovementState.Death:
                    graphic.currentAnim.pause();
                    if (playDeathAnim)
                        graphic.play("death");
                    break;
            }

            if (justLanded)
                graphic.color = Color.Yellow;


            graphic.color = Color.White;

            base.update();
            graphic.update();
        }

        public bool onewaysolidCondition(bEntity me, bEntity other)
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
            graphic.render(sb, pos);

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

        void handleDebugRoutines()
        {
        }
    }
}
