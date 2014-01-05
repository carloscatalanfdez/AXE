using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using bEngine;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Entities.Axes;
using AXE.Game.Utils;
using AXE.Game.Screens;

namespace AXE.Game.Entities.Enemies
{
    class Zombie : Enemy, IHazardProvider, IHazard
    {
        public enum State { Invisible, In, Walk, Out, Falling, Dead }
        const int CHANGE_STATE_TIMER = 0;
        const int DEAD_ANIM_TIMER = 1;

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

        float hspeed;

        int invisibleBaseTime, invisibleOptionalTime;
        int walkBaseTime, walkOptionalTime;

        int deathAnimDuration;

        public Zombie(int x, int y)
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
            spgraphic.add(new bAnim("invisible", new int[] { 17 }));
            spgraphic.add(new bAnim("walk", new int[] { 5, 6 }, 0.3f));
            spgraphic.add(new bAnim("out", new int[] { 2, 1, 0, 1, 0 }, 0.4f, false));
            spgraphic.add(new bAnim("in", new int[] { 0, 1, 0, 1, 0, 1, 2, 3, 4, 4, 4, 4 }, 0.4f, false));
            spgraphic.add(new bAnim("jump", new int[] { 10 }));
            spgraphic.add(new bAnim("death", new int[] { 10 }));
            spgraphic.play("invisible");

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

            hspeed = 2f;// 0.2f;
            vspeed = 0f;
            gravity = 0.5f;
            deathFallThreshold = 5;

            invisibleBaseTime = 100;
            invisibleOptionalTime = 80;
            walkBaseTime = 120;
            walkOptionalTime = 70;
            deathAnimDuration = 50;

            state = State.Walk;
            changeState(State.Invisible);

            attributes.Add(ATTR_SOLID);
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                switch (newState)
                {
                    case State.Invisible:
                        collidable = false;
                        timer[CHANGE_STATE_TIMER] = invisibleBaseTime + Tools.random.Next(invisibleOptionalTime) - invisibleOptionalTime;
                        break;
                    case State.Walk:
                        collidable = true;
                        timer[CHANGE_STATE_TIMER] = walkBaseTime + Tools.random.Next(walkOptionalTime) - walkOptionalTime;
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
                        case State.Invisible:
                            if (Tools.random.Next(2) < 1)
                                turn();

                            changeState(State.In);
                            break;
                        case State.Walk:
                            if (Tools.random.Next(2) < 1)
                                turn();

                            changeState(State.Out);
                            break;
                    }
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
                case State.Invisible:
                    {
                        spgraphic.play("invisible");

                        // Allow movement here too
                        Vector2 nextPosition = new Vector2(x + directionToSign(facing) * hspeed, y);
                        bool wontFall = checkForGround(
                                (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                                (int)nextPosition.Y);
                        bool wontCollide = !placeMeeting(
                                (int)nextPosition.X,
                                (int)nextPosition.Y, new String[] { "solid" });
                        if (wontFall && wontCollide)
                            moveTo.X += directionToSign(facing) * (hspeed / 3f);

                        break;
                    }
                case State.In:
                    {
                        spgraphic.play("in");

                        if (spgraphic.currentAnim.finished)
                        {
                            Dir facingDir = facing;
                            if (facingDir == Dir.Left)
                                watchMask.offsetx = _mask.offsetx - watchMask.w;
                            else
                                watchMask.offsetx = _mask.offsetx + _mask.w;
                            watchMask.offsety = (graphicHeight() - watchMask.h);

                            // Is he were I'm looking?
                            if (!isPlayerOnSight(facingDir, false, new String[] { "solid" }, watchMask, watchWrappedMask))
                            {
                                // well then turn
                                turn();
                            }

                            changeState(State.Walk);
                        }
                        else if (spgraphic.currentAnim.frame == 2)
                        {
                            collidable = true;
                        }
                        break;
                    }
                case State.Walk:
                    {
                        spgraphic.play("walk");

                        Vector2 nextPosition = new Vector2(x + directionToSign(facing) * hspeed, y);
                        bool wontFall = checkForGround(
                                (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                                (int)nextPosition.Y);
                        bool wontCollide = !placeMeeting(
                                (int)nextPosition.X,
                                (int)nextPosition.Y, new String[] { "solid" });
                        if (wontFall && wontCollide)
                            moveTo.X += directionToSign(facing) * hspeed;

                        if (!wontFall || !wontCollide)
                            changeState(State.Out);

                        break;
                    }
                case State.Out:
                    {
                        spgraphic.play("out");

                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Invisible);
                        }
                        else if (spgraphic.currentAnim.frame == 1)
                        {
                            collidable = false;
                        }

                        break;
                    }
                case State.Falling:
                    {
                        spgraphic.play("jump");

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
                                onDeath(null); // You'd be dead, buddy!
                            changeState(State.Walk);
                        }

                        moveTo.Y += vspeed;

                        break;
                    }
                case State.Dead:
                    {
                        spgraphic.play("death");
                        float factor = (timer[DEAD_ANIM_TIMER] / (deathAnimDuration * 1f));
                        color *= factor;
                        if (color.A <= 0)
                        {
                            world.remove(this);
                        }
                        break;
                    }
            }

            if (state == State.Walk || state == State.Falling || state == State.Invisible)
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
                        changeState(State.Out);
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

            spgraphic.flipped = (facing == Dir.Left);

            handleSoundEffects();

            // Uberdebuggo temporal thingie!
            if (mouseHover && input.check(Microsoft.Xna.Framework.Input.Keys.D))
                world.remove(this);
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

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

        public override void onDeath(Entity killer)
        {
            if (state != State.Dead)
            {
                Game.res.sfxPlayerHit.Play();
                state = State.Dead;
                color = new Color(164, 0, 0, 255);
                timer[DEAD_ANIM_TIMER] = deathAnimDuration;
                collidable = false;
            }

            base.onDeath(killer);
        }

        public override bool onHit(Entity other)
        {
            if (state != State.Dead)
            {
                base.onHit(other);

                if (other is Axe)
                {
                    Entity killer = other.getKillOwner();
                    onDeath(killer);

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
            }

            return false;
        }

        public override void onCollision(string type, bEntity other)
        {
            if (state == State.Walk)
            {
                if (type == "player")
                {
                    (other as Player).onCollision("hazard", this);
                }
            }
        }

        /**
         * IHAZARD METHODS
         */
        public void setOwner(IHazardProvider owner)
        {
        }

        public IHazardProvider getOwner()
        {
            return this;
        }

        public Player.DeathState getType()
        {
            return Player.DeathState.Generic;
        }

        public virtual void onHit()
        {
            changeState(State.Out);
        }

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
        }
    }
}
