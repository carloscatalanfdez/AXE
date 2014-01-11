using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using bEngine.Graphics;
using AXE.Game.Entities.Base;
using bEngine;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Entities.Axes;

namespace AXE.Game.Entities.Enemies
{
    class TerritorialRapier : Enemy, IHazardProvider, IHazard
    {
        public enum State { Idle, Unseathing, Defending, Falling, Dead, Walk }
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

        public TerritorialRapier(int x, int y, bool flipped)
            : base(x, y)
        {
            facing = flipped ? Dir.Left : Dir.Right;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprZombieSheet;
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprRapierSheet, 42, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0, 1 }, 0.05f));
            spgraphic.add(new bAnim("unseathe", new int[] { 0, 4, 5, 6, 8, 8 }, 0.44f, false));
            spgraphic.add(new bAnim("defend", new int[] { 8, 9, 10, 9 }, 0.1f));
            spgraphic.add(new bAnim("death", new int[] { 0 }));
            spgraphic.add(new bAnim("jump", new int[] { 0 }));
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

            vspeed = 0f;
            gravity = 0.5f;

            deathAnimDuration = 50;

            state = State.Idle;
            changeState(State.Idle);

            attributes.Add(ATTR_SOLID);
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                //switch (newState)
                //{
                //}

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
                    //switch (state)
                    //{
                    //}
                    //break;
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
                    {
                        spgraphic.play("idle");
                        break;
                    }
                case State.Unseathing:
                    {
                        spgraphic.play("unseathe");

                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Defending);
                        }
                        break;
                    }
                case State.Defending:
                    {
                        spgraphic.play("defend");

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
                            changeState(State.Idle);
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

            if (state == State.Falling)
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
                        changeState(State.Idle);
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
            if (state == State.Idle || state == State.Defending)
            {
                if (type == "player")
                {
                    changeState(State.Unseathing);
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
            changeState(State.Idle);
        }

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
        }
    }
}
