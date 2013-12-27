using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE.Game;
using AXE.Game.Screens;
using AXE.Game.Entities.Base;
using AXE.Game.Utils;
using AXE.Game.Entities.Axes;
using AXE.Game.Control;
using AXE.Common;
using AXE.Game.Entities.Contraptions;

namespace AXE.Game.Entities.Enemies
{
    class Undead : Enemy, IHazardProvider
    {
        public enum State { None, Idle, Turn, Walk, Fall, StandUp, Chase, Attacking, Attacked, Falling, Dead }
        const int CHANGE_STATE_TIMER = 0;
        const int CHASE_REACTION_TIMER = 1;
        const int DEAD_ANIM_TIMER = 2;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        // gravity things
        bool fallingToDeath;
        int deathFallThreshold;
        Vector2 fallingFrom;
        float gravity;
        float vspeed;

        Vector2 moveTo;
        bMask watchMask;
        bMaskList watchWrappedMask;

        public State state;
        bool beginChase;
        int chaseReactionTime;

        int attackThreshold;
        int attackChargeTime;
        int attackTime;
        KillerRect weaponHitZone;

        float hspeed;
        
        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int turnBaseTime, turnOptionalTime;
        int fallBaseTime, fallOptionalTime;
        int standUpBaseTime, standUpOptionalTime;

        int fallChance = 5;

        int deathAnimDuration;

        public Undead(int x, int y)
            : base(x, y)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprZombieSheet;
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprZombieSheet, 30, 32);
            spgraphic.add(new bAnim("idle", new int[] { 4 }));
            spgraphic.add(new bAnim("walk", new int[] { 0, 1, 2, 3 }, 0.1f));
            spgraphic.add(new bAnim("chase-reacting", new int[] { 4 }));
            spgraphic.add(new bAnim("chase", new int[] { 8, 9, 10, 11 }, 0.1f));
            spgraphic.add(new bAnim("attack", new int[] { 12, 13, 14 }, 0.3f, false));
            spgraphic.add(new bAnim("attacked", new int[] { 15 }));
            spgraphic.add(new bAnim("fall", new int[] { 5, 6 }, 0.4f, false));
            spgraphic.add(new bAnim("stand-up", new int[] { 7 }));
            spgraphic.add(new bAnim("jump", new int[] { 16 }));
            spgraphic.add(new bAnim("death", new int[] { 16 }));
            spgraphic.play("idle");

            mask.w = 16;
            mask.h = 21;
            mask.offsetx = 7;
            mask.offsety = 11;

            watchMask = new bMask(x, y, 90, 21);
            watchMask.game = game;
            bMask maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            bMask maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            watchWrappedMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            watchWrappedMask.game = game;

            hspeed = 0.2f;
            vspeed = 0f;
            gravity = 0.5f;
            deathFallThreshold = 5;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            walkBaseTime = 70;
            walkOptionalTime = 70;
            deathAnimDuration = 50;
            turnBaseTime = 20;
            turnOptionalTime = 20;

            fallBaseTime = 50;
            fallOptionalTime = 50;
            standUpBaseTime = 30;
            standUpOptionalTime = 30;

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            beginChase = false;
            chaseReactionTime = 15;

            attackThreshold = 16;
            attackChargeTime = 10;
            attackTime = 8;

            state = State.None;
            changeState(State.Idle);
            
            attributes.Add(ATTR_SOLID);
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                switch (newState)
                {
                    case State.Idle:
                        timer[0] = idleBaseTime + Tools.random.Next(idleOptionalTime) - idleOptionalTime;
                        break;
                    case State.Walk:
                        timer[0] = walkBaseTime + Tools.random.Next(walkOptionalTime) - walkOptionalTime;
                        break;
                    case State.Turn:
                        timer[0] = turnBaseTime + Tools.random.Next(turnOptionalTime) - turnOptionalTime;
                        break;
                    case State.Fall:
                        timer[0] = fallBaseTime + Tools.random.Next(fallOptionalTime) - fallOptionalTime;
                        break;
                    case State.StandUp:
                        timer[0] = standUpBaseTime + Tools.random.Next(standUpOptionalTime) - standUpOptionalTime;
                        break;
                    case State.Chase:
                        beginChase = false;
                        timer[1] = chaseReactionTime;
                        break;
                    case State.Attacking:
                        timer[0] = attackChargeTime;
                        break;
                    case State.Attacked:
                        int xx, yy = 4;
                        if (facing == Dir.Right)
                            xx = 10;
                        else
                            xx = -0;
                        weaponHitZone = new KillerRect(x + xx, y + yy, 20, 27, Player.DeathState.ForceHit);
                        weaponHitZone.setOwner(this);
                        world.add(weaponHitZone, "hazard");
                        timer[0] = attackTime;
                        break;
                }

                if (performChange)
                    state = newState;
            }
        }

        public void turn()
        {
            if (facing == Dir.Left)
                facing = Dir.Right;
            else
                facing = Dir.Left;
        }

        public override void onTimer(int n)
        {
            switch (n)
            {
                case CHANGE_STATE_TIMER:
                    switch (state)
                    {
                        case State.Idle:
                            if (Tools.random.Next(2) < 1)
                                changeState(State.Turn);
                            
                            changeState(State.Walk);
                            break;
                        case State.Walk:
                            if (Tools.random.Next(2) < 1)
                                changeState(State.Turn);
                            else
                                changeState(State.Idle);
                            break;
                        case State.Turn:
                            turn();
                            changeState(State.Idle);
                            break;
                        case State.Fall:
                            changeState(State.StandUp);
                            break;
                        case State.StandUp:
                            changeState(State.Idle);
                            break;
                        case State.Chase:
                            break;
                        case State.Attacking:
                            changeState(State.Attacked);
                            // Sound!
                            break;
                        case State.Attacked:
                            changeState(State.Idle);
                            if (weaponHitZone != null)
                            {
                                world.remove(weaponHitZone);
                                weaponHitZone = null;
                            }

                            break;
                    }
                    break;
                case CHASE_REACTION_TIMER:
                    if (state == State.Chase)
                        beginChase = true;
                    break;
                case DEAD_ANIM_TIMER:
                    break;
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            spgraphic.update();

            moveTo = pos;
            bool onAir = !checkForGround(x, y);

            if (onAir)
            {
                state = State.Falling;
                fallingFrom = pos;
                fallingToDeath = false;
            }

            switch (state)
            {
                case State.Idle:
                    spgraphic.play("idle");
                    break;
                case State.Walk:
                    spgraphic.play("walk");

                    Vector2 nextPosition = new Vector2(x + directionToSign(facing) * hspeed, y);
                    bool wontFall = checkForGround(
                            (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                            (int)nextPosition.Y);
                    bool wontCollide = !placeMeeting(
                            (int)nextPosition.X,
                            (int)nextPosition.Y, new String[] { "player", "solid" });
                    if (wontFall && wontCollide)
                        moveTo.X += directionToSign(facing) * hspeed;

                    if (!wontFall || !wontCollide)
                        changeState(State.Turn);

                    break;
                case State.Turn:
                    spgraphic.play("idle");
                    break;
                case State.Falling:
                    if (onAir)
                    {
                        vspeed += gravity;
                        if (vspeed > 0 && fallingFrom == Vector2.Zero)
                        {
                            fallingToDeath = false;
                            fallingFrom = pos;
                        }

                        if (vspeed > 0 && pos.Y - fallingFrom.Y >= deathFallThreshold)
                        {
                            fallingToDeath = true;
                        }
                    }
                    else
                    {
                        if (fallingToDeath)
                            onDeath(); // You'd be dead, buddy!
                        changeState(State.Idle);
                    }

                    moveTo.Y += vspeed;

                    spgraphic.play("jump");

                    break;
                case State.Fall:
                    spgraphic.play("fall");
                    break;
                case State.StandUp:
                    spgraphic.play("stand-up");
                    break;
                case State.Chase:
                    if (beginChase)
                    {
                        spgraphic.play("chase");

                        // Should I keep trying?
                        bool shouldKeepChasing = true;
                        if (Tools.random.Next(30) < 1)
                        {
                            if (!isPlayerOnSight(facing, false, new String[] { "solid" }, watchMask, watchWrappedMask))
                            {
   
                                shouldKeepChasing = false;
                            }
                        }

                        if (shouldKeepChasing)
                        {
                            float hsp = hspeed;
                            nextPosition = new Vector2(x + directionToSign(facing) * hsp, y);
                            wontFall = checkForGround(
                                    (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                                    (int)nextPosition.Y);
                            wontCollide = !placeMeeting(
                                    (int)nextPosition.X,
                                    (int)nextPosition.Y, new String[] { "player", "solid" });
                            if (wontFall && wontCollide)
                                moveTo.X += directionToSign(facing) * hsp;
                            else if (!wontFall || !wontCollide)
                                changeState(State.Idle);
                        }
                        else
                        {
                            changeState(State.Idle);
                        }
                    }
                    else
                    {
                        if (state == State.Chase)
                            spgraphic.play("chase-reacting");
                    }
                    break;
                case State.Attacking:
                    spgraphic.play("attack");
                    break;
                case State.Attacked:
                    spgraphic.play("attacked");
                    break;
                case State.Dead:
                    spgraphic.play("death");
                    float factor = (timer[DEAD_ANIM_TIMER] / (deathAnimDuration * 1f));
                    color *= factor;
                    if (color.A <= 0)
                    {
                        world.remove(this);
                    }
                    break;
            }

            if (state == State.Idle || state == State.Walk)
            {
                Dir facingDir = facing;
                if (facingDir == Dir.Left)
                    watchMask.offsetx = _mask.offsetx - watchMask.w;
                else
                    watchMask.offsetx = _mask.offsetx + _mask.w;
                watchMask.offsety = (graphicHeight() - watchMask.h);


                if (isPlayerOnSight(facingDir, false, new String[] { "solid" }, watchMask, watchWrappedMask))
                {
                    // Yeah, let's go
                    facing = facingDir;
                    changeState(State.Chase);
                }
            }
            else if (state == State.Chase)
            {
                Player[] players = (world as LevelScreen).players;
                foreach (Player player in players)
                {
                    if (player != null && player.state != Player.MovementState.Death && (player.pos - pos).Length() < attackThreshold)
                    {
                        changeState(State.Attacking);
                    }
                }
            }

            if (state == State.Walk || state == State.Chase || state == State.Falling)
            {
                int currentX = (int)Math.Round(pos.X);
                int nextX = (int)Math.Round(moveTo.X);
                int c = currentX - nextX;
                if (Math.Abs(pos.Y - moveTo.Y) < 1 && Math.Abs(currentX - nextX) < 1)
                {
                    pos = moveTo;
                }
                else
                {
                    Vector2 remnant;
                    // Check wether we collide first with a solid or a onewaysolid,
                    // and use that data to position the undead enemy.
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
                        if (state == State.Chase)
                        {
                            changeState(State.Turn);
                        }
                    }

                    // The y movement was stopped
                    if (remnant.Y != 0 && vspeed < 0)
                    {
                        // Touched ceiling
                        vspeed = 0;
                    }
                    else if (remnant.Y != 0 && vspeed > 0)
                    {
                        // Landed
                    }
                }
            }

            if (state == State.Chase || state == State.Walk)
            {
                int chance = state == State.Chase ? fallChance * 5 : fallChance;
                if (Tools.random.Next(5000) < chance)
                {
                    changeState(State.Fall);
                }
            }

            spgraphic.flipped = (facing == Dir.Left);

            handleSoundEffects();

            // Uberdebuggo temporal thingie!
            if (mouseHover && input.check(Microsoft.Xna.Framework.Input.Keys.D))
                world.remove(this);
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (bConfig.DEBUG)
            {
                bMask result = generateWrappedMask(watchMask, watchWrappedMask);
                result.render(sb);
            }

            spgraphic.color = color;
            spgraphic.render(sb, pos);

            if (bConfig.DEBUG)
                sb.DrawString(game.gameFont, state.ToString() + " [" + timer[0] + "]", new Vector2(x, y - 8), Color.White);
        }

        public override int graphicWidth()
        {
            return spgraphic.width;
        }

        public override int graphicHeight()
        {
            return spgraphic.height;
        }

        public void handleSoundEffects()
        {
        }

        public override void onDeath()
        {
            if (state != State.Dead)
            {
                state = State.Dead;
                color = new Color(164, 0, 0, 255);
                timer[DEAD_ANIM_TIMER] = deathAnimDuration;
            }
        }

        public override bool onHit(Entity other)
        {
            base.onHit(other);

            if (other is Axe)
            {
                onDeath();

                if (rewarder != null)
                {
                    if (contraptionRewardData.target == null)
                    {
                        contraptionRewardData.target = ((other as NormalAxe).thrower as bEntity);
                    }
                }
                onSolved();

                return true;
            }

            return false;
        }

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
            // tamed = true;
        }
    }
}
