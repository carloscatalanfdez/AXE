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
    class Dagger : Enemy, IHazardProvider
    {
        public enum State { None, Idle, Turn, Walk, Jump, Attacking, Attacked, Throw, Falling, Dead }
        const int CHANGE_STATE_TIMER = 0;
        const int THROW_REACTION_TIMER = 1;
        const int THROW_COOLDOWN_TIMER = 2;
        const int DEAD_ANIM_TIMER = 3;

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
        int throwReactionTime;

        int attackThreshold;
        int attackChargeTime;
        int attackTime;
        KillerRect weaponHitZone;

        float hspeed;
        
        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int turnBaseTime, turnOptionalTime;

        bool isTrackingPlayer;
        bool canThrow;
        int throwCoolTime;

        int deathAnimDuration;

        public Dagger(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprDaggerSheet, 30, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("walk", new int[] { 0, 1, 2, 1 }, 0.3f));
            spgraphic.add(new bAnim("seen-reacting", new int[] { 1 }));
            spgraphic.add(new bAnim("throw", new int[] { 0, 1 }, 0.4f, false));
            spgraphic.add(new bAnim("attack", new int[] { 1 }, 0.3f, false));
            spgraphic.add(new bAnim("attacked", new int[] { 1 }));
            spgraphic.add(new bAnim("jump", new int[] { 8, 9 }, 0.7f, false));
            spgraphic.add(new bAnim("fall", new int[] { 10 }));
            spgraphic.add(new bAnim("death", new int[] { 1 }));
            spgraphic.play("idle");

            loadNormalMask();

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

            isTrackingPlayer = false;
            canThrow = true;
            throwCoolTime = 40;

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            beginChase = false;
            throwReactionTime = 5;

            attackThreshold = 16;
            attackChargeTime = 10;
            attackTime = 8;

            state = State.None;
            changeState(State.Idle);
            
            attributes.Add(Enemy.ATTR_SOLID);
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
                    case State.Throw:
                        spgraphic.play("throw");
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
                case THROW_REACTION_TIMER:
                    if (state != State.Throw)
                        changeState(State.Throw);
                    break;
                case THROW_COOLDOWN_TIMER:
                    canThrow = true;
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
                if (vspeed < 0)
                {
                    state = State.Jump;
                }
                else
                {
                    state = State.Falling;
                    fallingToDeath = false;
                    fallingFrom = pos;
                }
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

                    spgraphic.play("fall");

                    break;
                case State.Throw:
                    if (spgraphic.currentAnim.finished)
                    {
                        throwDagger();
                        changeState(State.Idle);
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

            if (canThrow && (state == State.Idle || state == State.Walk || state == State.Jump || state == State.Falling))
            {
                Dir facingDir = facing;
                if (facingDir == Dir.Left)
                    watchMask.offsetx = _mask.offsetx - watchMask.w;
                else
                    watchMask.offsetx = _mask.offsetx + _mask.w;
                watchMask.offsety = (graphicHeight() - watchMask.h);

                loadThrowMask();
                bool canIseeYou = isPlayerOnSight(facingDir, false, new String[] { "solid" }, watchMask, watchWrappedMask);
                loadNormalMask();

                if (canIseeYou)
                {
                    canThrow = false;
                    if (isTrackingPlayer)
                    {
                        isTrackingPlayer = false;
                        changeState(State.Throw);
                    }
                    else
                    {
                        // Yeah, let's go
                        facing = facingDir;
                        timer[THROW_REACTION_TIMER] = throwReactionTime;
                    }
                }
            }

            if (state == State.Walk || state == State.Jump || state == State.Falling)
            {
                Vector2 remnant;
                // Check wether we collide first with a solid or a onewaysolid,
                // and use that data to position the Dagger enemy.
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
                //result.render(sb);
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

        public void loadNormalMask()
        {
            _mask.w = 16;
            _mask.h = 21;
            _mask.offsetx = 7;
            _mask.offsety = 11;
        }

        public void loadThrowMask()
        {
            _mask.w = 16;
            _mask.h = 4;
            _mask.offsetx = 7;
            _mask.offsety = 15;
        }

        public void handleSoundEffects()
        {
        }

        public void onDeath()
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
        
        public void throwDagger()
        {
            // spawn dagger
            int spawnX = facing == Dir.Left ? 0 : _mask.offsetx + _mask.w;
            FlameSpiritBullet bullet =
                new FlameSpiritBullet(x + spawnX, y + 15, spgraphic.flipped);
            bullet.setOwner(this);
            world.add(bullet, "hazard");

            timer[THROW_COOLDOWN_TIMER] = throwCoolTime;
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
