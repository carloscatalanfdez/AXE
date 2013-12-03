using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using bEngine;
using AXE.Game.Screens;
using Microsoft.Xna.Framework;
using AXE.Game.Utils;
using AXE.Game.Entities.Axes;

namespace AXE.Game.Entities.Enemies
{
    class CorrosiveSlime : Enemy
    {
        public enum State { None, IdleTop, WalkTop, MoveToHide, HideCorner, PrepareFall, Fall, Explode, Dead }
        const int CHANGE_STATE_TIMER = 0;
        const int DEAD_ANIM_TIMER = 2;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        // gravity things
        float gravity;
        float vspeed;
        int hspeed;

        Vector2 moveTo;
        bMask dropWatchMask;
        bMaskList dropWrappedWatchMask;

        bMask hideWatchMask;

        public State state;

        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int hideBaseTime, hideOptionalTime;
        int dropBaseTime, dropOptionalTime;

        int hideChance;
        int deathAnimDuration;

        public CorrosiveSlime(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprSlimeSheet, 32, 32);
            spgraphic.add(new bAnim("idle-top", new int[] { 0, 1 }, 0.2f));
            spgraphic.add(new bAnim("walk-top", new int[] { 2, 3 }, 0.2f));
            spgraphic.add(new bAnim("run-top", new int[] { 2, 3 }, 0.6f));
            spgraphic.add(new bAnim("hide-corner", new int[] { 4, 5 }, 0.1f));
            spgraphic.add(new bAnim("fall", new int[] { 0, 10, 20, 30 }, 0.3f, false));
            spgraphic.add(new bAnim("explode", new int[] { 31, 32, 33, 34 }, 0.6f, false));
            
            spgraphic.play("idle-top");

            loadTopMask();

            dropWatchMask = new bMask(x, y, 20, (world as LevelScreen).height);
            bMask maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            bMask maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            dropWrappedWatchMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            dropWrappedWatchMask.game = game;

            hideWatchMask = new bMask(x, y, (world as LevelScreen).width / 4, 10);

            hspeed = 1;
            vspeed = 0f;
            gravity = 0.5f;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            walkBaseTime = 30;
            walkOptionalTime = 30;
            hideBaseTime = 120;
            hideOptionalTime = 60;
            dropBaseTime = 3;
            dropOptionalTime = 3;

            hideChance = 100; // it will only hide if a random from 0 to this number is 0

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            state = State.None;
            changeState(State.IdleTop);

            attributes.Add(ATTR_SOLID);
        }

        protected void loadFallMask()
        {
            _mask.w = 13;
            _mask.h = 15;
            _mask.offsetx = 9;
            _mask.offsety = 14;
        }

        protected void loadCornerMask()
        {
            int xoffset;
            if (facing == Dir.Right)
            {
                xoffset = 16;
            }
            else
            {
                xoffset = 6;
            } 
            
            _mask.w = 10;
            _mask.h = 10;
            _mask.offsetx = xoffset;
            _mask.offsety = 0;
        }

        protected void loadTopMask()
        {
            _mask.w = 20;
            _mask.h = 13;
            _mask.offsetx = 6;
            _mask.offsety = 0;
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                switch (newState)
                {
                    case State.IdleTop:
                        timer[CHANGE_STATE_TIMER] = idleBaseTime + Tools.random.Next(idleOptionalTime) - idleOptionalTime;
                        loadTopMask();
                        break;
                    case State.WalkTop:
                        timer[CHANGE_STATE_TIMER] = walkBaseTime + Tools.random.Next(walkOptionalTime) - walkOptionalTime;
                        loadTopMask();
                        break;
                    case State.HideCorner:
                        timer[CHANGE_STATE_TIMER] = hideBaseTime + Tools.random.Next(hideOptionalTime) - hideOptionalTime;
                        loadCornerMask();
                        break;
                    case State.PrepareFall:
                        timer[CHANGE_STATE_TIMER] = dropBaseTime + Tools.random.Next(dropOptionalTime) - dropOptionalTime;
                        loadTopMask();
                        break;
                    case State.Fall:
                        loadFallMask();
                        break;
                    default:
                        break;
                }

                if (performChange)
                    state = newState;
            }
        }

        public override void onTimer(int n)
        {
            switch (n)
            {
                case CHANGE_STATE_TIMER:
                    switch (state)
                    {
                        case State.IdleTop:
                            if (Tools.random.Next(2) < 1)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;
                            changeState(State.WalkTop);
                            break;
                        case State.WalkTop:
                            changeState(State.IdleTop);
                            break;
                        case State.PrepareFall:
                            changeState(State.Fall);
                            break;
                        case State.HideCorner:
                            if (facing == Dir.Left)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;

                            changeState(State.WalkTop);
                            break;
                        default:
                            break;
                    }
                    break;
                case DEAD_ANIM_TIMER:
                    break;
            }
        }

        public override bool onHit(Entity other)
        {
            base.onHit(other);

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

        private bool isOnCeiling()
        {
            return state == State.IdleTop || state == State.HideCorner || state == State.WalkTop || state == State.MoveToHide;
        }

        public override void onUpdate()
        {
            base.onUpdate();

            spgraphic.update();

            moveTo = pos;
            bool onAir;
            if (isOnCeiling())
            {
                onAir = !checkForCeiling(x, y);
            }
            else
            {
                onAir = !checkForGround(x, y);
            }

            if (onAir && state != State.Fall)
            {
                changeState(State.Fall);
            }

            switch (state)
            {
                case State.PrepareFall:
                case State.IdleTop:
                    spgraphic.play("idle-top");
                    break;
                case State.WalkTop:
                    spgraphic.play("walk-top");

                    Vector2 nextPosition = new Vector2(x + directionToSign(facing) * hspeed, y);
                    bool wontFall = checkForCeiling(
                            (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                            (int)nextPosition.Y);
                    bool wontCollide = !placeMeeting(
                            (int)nextPosition.X,
                            (int)nextPosition.Y, new String[] { "player", "solid" });
                    if (wontFall && wontCollide)
                        moveTo.X += directionToSign(facing) * hspeed;
                    else if (!wontFall)
                        changeState(State.IdleTop);
                    else if (!wontCollide)
                    {
                        switchDirections();
                    }

                    break;
                case State.MoveToHide:
                    spgraphic.play("run-top");

                    int hsp = (int)(hspeed * 3);
                    nextPosition = new Vector2(x + directionToSign(facing) * hsp, y);
                    wontFall = checkForCeiling(
                            (int)(nextPosition.X + directionToSign(facing) * graphicWidth() / 2),
                            (int)nextPosition.Y);
                    wontCollide = !placeMeeting(
                            (int)nextPosition.X,
                            (int)nextPosition.Y, new String[] { "solid" });
                    if (wontFall)
                        moveTo.X += directionToSign(facing) * hsp;

                    if (!wontCollide)
                    {
                        // Place yourself on the fucking corner
                        moveToContact(moveTo, "solid");
                        // and hide
                        changeState(State.HideCorner);
                    }

                    break;
                case State.HideCorner:
                    spgraphic.play("hide-corner");
                    break;
                case State.Fall:
                    if (onAir)
                    {
                        vspeed += gravity;
                        spgraphic.play("fall");
                    }
                    else
                    {
                        changeState(State.Explode);
                        spgraphic.play("explode");
                    }

                    moveTo.Y += vspeed;

                    break;
                case State.Explode:
                    if (spgraphic.currentAnim.finished)
                    {
                        onDeath();
                    }
                    break;
                case State.Dead:
                    float factor = (timer[DEAD_ANIM_TIMER] / (deathAnimDuration * 1f));
                    color *= factor;
                    if (color.A <= 0)
                    {
                        world.remove(this);
                    }
                    break;
            }

            if (state == State.IdleTop || state == State.WalkTop || state == State.MoveToHide)
            {
                if (isPlayerOnSight(Dir.None, true, new String[] { "onewaysolid", "solid" }, dropWatchMask, dropWrappedWatchMask))
                {
                    // Yeah, let's go
                    changeState(State.PrepareFall);
                }
            }

            if (state == State.IdleTop || state == State.WalkTop)
            {
                // Ok so no dropping
                // Hiding?
                Dir facingDir = facing;
                if (facingDir == Dir.Left)
                    hideWatchMask.offsetx = -hideWatchMask.w;
                else
                    hideWatchMask.offsetx = graphicWidth();

                bMask holdMyMaskPlease = _mask;
                mask = hideWatchMask;

                bool spottedSolid = placeMeeting(x, y, "solid");
                mask = holdMyMaskPlease; // thank you!

                if (spottedSolid && Tools.random.Next(100) < 1)
                {
                    facing = facingDir;
                    changeState(State.MoveToHide);
                }
            }

            if (state == State.WalkTop || state == State.Fall || state == State.MoveToHide)
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
                }

                // The y movement was stopped
                if (remnant.Y != 0 && vspeed < 0)
                {
                    // Touched ceiling
                    vspeed = 0;
                }
                else if (remnant.Y != 0 && vspeed > 0)
                {
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

        public void onDeath()
        {
            if (state != State.Dead)
            {
                attributes.Remove(ATTR_SOLID);
                state = State.Dead;
                color = new Color(164, 0, 0, 255);
                timer[DEAD_ANIM_TIMER] = deathAnimDuration;
            }
        }
    }
}
