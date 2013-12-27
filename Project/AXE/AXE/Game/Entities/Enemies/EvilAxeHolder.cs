using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using bEngine;
using Microsoft.Xna.Framework;
using AXE.Game.Utils;
using AXE.Game.Screens;
using AXE.Game.Entities.Axes;

namespace AXE.Game.Entities.Base
{
    class EvilAxeHolder : Enemy, IWeaponHolder
    {
        public enum State { None, Idle, Walk, ChasingAxe, GrabbedAxe, Throwing, Falling, Dead }
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

        bool beginChase;
        int chaseReactionTime;

        IWeapon weapon;

        Vector2 moveTo;
        bMask watchMask;
        protected bMask weaponCatchMask;

        public State state;

        int hspeed;

        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int throwBaseTime, throwOptionalTime;
        int grabbedBaseTime, grabbedOptionalTime;

        SoundEffect sfxGrab;
        SoundEffect sfxThrow;

        int deathAnimDuration;

        public EvilAxeHolder(int x, int y)
            : base(x, y)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprAxeThrowerSheet;
            loadSoundEffects();
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprAxeThrowerSheet, 17, 26);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("walk", new int[] { 0, 2, 1 }, 0.3f));
            spgraphic.add(new bAnim("throw", new int[] { 1 }, 1.0f, false));
            spgraphic.add(new bAnim("jump", new int[] { 2 }));
            spgraphic.add(new bAnim("chase-prepare",  new int[] { 2 }));
            spgraphic.add(new bAnim("chase-running", new int[] { 5, 6 }, 0.5f));
            spgraphic.add(new bAnim("death", new int[] { 3, 4 }, 0.3f));
            spgraphic.play("idle");

            mask.w = 16;
            mask.h = 24;
            mask.offsetx = 1;
            mask.offsety = 2;

            watchMask = new bMask(x, y, 90, 26);

            hspeed = 1;
            vspeed = 0f;
            gravity = 0.5f;
            deathFallThreshold = 40;
            deathAnimDuration = 30;
            beginChase = false;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            walkBaseTime = 30;
            walkOptionalTime = 30;
            throwBaseTime = 50;
            throwOptionalTime = 20;
            grabbedBaseTime = 30;
            grabbedOptionalTime = 10;

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            loadSoundEffects();

            state = State.None;
            changeState(State.Idle);
            chaseReactionTime = 15;

            // Spawn with axe
            spawnAxe();

            weaponCatchMask = new bMask(x, y,
                (int)(graphicWidth() * 1.5f),
                (int)(graphicHeight() * 1.25f),
                -(int)(graphicWidth() * 0.25f),
                -(int)(graphicHeight() * 0.25f));

            attributes.Add(ATTR_SOLID);
        }

        public void loadSoundEffects()
        {
            sfxGrab = (game as AxeGame).res.sfxEvilPick;
            sfxThrow = (game as AxeGame).res.sfxEvilThrow;
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                switch (newState)
                {
                    case State.Idle:
                        timer[CHANGE_STATE_TIMER] = idleBaseTime + Tools.random.Next(idleOptionalTime) - idleOptionalTime;
                        break;
                    case State.Walk:
                        timer[CHANGE_STATE_TIMER] = walkBaseTime + Tools.random.Next(walkOptionalTime) - walkOptionalTime;
                        break;
                    case State.ChasingAxe:
                        beginChase = false;
                        timer[CHASE_REACTION_TIMER] = (int)(chaseReactionTime * 1.5f);
                        break;
                    case State.Throwing:
                        sfxThrow.Play();

                        weapon.onThrow(3, facing, getHandPosition());
                        timer[CHANGE_STATE_TIMER] = throwBaseTime + Tools.random.Next(throwOptionalTime) - throwOptionalTime;
                        break;
                    case State.GrabbedAxe:
                        timer[CHANGE_STATE_TIMER] = grabbedBaseTime + Tools.random.Next(grabbedBaseTime) - grabbedOptionalTime;
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
                        case State.Idle:
                            if (Tools.random.Next(2) < 1)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;


                            changeState(State.Walk);
                            break;
                        case State.Walk:
                            if (Tools.random.Next(2) < 1)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;

                            changeState(State.Idle);
                            break;
                        case State.ChasingAxe:
                            break;
                        case State.Throwing:
                            changeState(State.Idle);
                            break;
                        case State.GrabbedAxe:
                            changeState(State.Idle);
                            break;
                    }
                    break;
                case CHASE_REACTION_TIMER:
                    if (state == State.ChasingAxe)
                        beginChase = true;
                    break;
                case DEAD_ANIM_TIMER:
                    break;
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            weaponCatchMask.update(x, y);

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
                    else if (!wontFall)
                        changeState(State.Idle);
                    else if (!wontCollide)
                    {
                        if (facing == Dir.Left)
                            facing = Dir.Right;
                        else
                            facing = Dir.Left;

                        changeState(State.Walk);
                    }

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
                case State.ChasingAxe:
                    if (beginChase)
                    {
                        spgraphic.play("chase-running");

                        int hsp = (int)(hspeed * 3);
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
                        spgraphic.play("chase-prepare");
                    }

                    break;
                case State.Throwing:
                    spgraphic.play("throw");
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
                    watchMask.offsetx = -watchMask.w;
                else
                    watchMask.offsetx = graphicWidth();
                watchMask.offsety = (graphicHeight() - watchMask.h);

                if (weapon != null)
                {   // I have an axe and I plan to use it
                    // VERY IMPORTANT
                    // When holding the mask, we need to hold the original _mask, since
                    // mask itself is a property and will return a hacked wrapped mask sometimes
                    bMask holdMyMaskPlease = _mask;
                    mask = watchMask;
                    bool sawYou = placeMeeting(x, y, "player", alivePlayerCondition);
                    mask = holdMyMaskPlease; // thank you!

                    if (sawYou)
                    {
                        changeState(State.Throwing);
                    }
                }
                else
                {   // I'm looking for an axe
                    bEntity axeTarget = instancePlace(watchMask, "axe");

                    if (axeTarget != null && axeTarget is Axe)
                        handleSeeAxe(axeTarget as Axe);
                }
            }
            else if (state == State.ChasingAxe)
            {
                tryGrabAnyAxe();
            }

            if (state == State.Walk || state == State.Falling || state == State.ChasingAxe)
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
                    // Landed
                }
            }

            spgraphic.flipped = (facing == Dir.Left);
            spgraphic.update();

            handleSoundEffects();

            // Uberdebuggo temporal thingie!
            if (mouseHover && input.check(Microsoft.Xna.Framework.Input.Keys.D))
                world.remove(this);
        }

        private void handleSoundEffects()
        {
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            spgraphic.color = color;
            spgraphic.render(sb, pos);

            if (bConfig.DEBUG)
            {
                sb.Draw(bDummyRect.sharedDummyRect(game), weaponCatchMask.rect, new Color(0.2f, 0.2f, 0.2f, 0.2f));
                sb.DrawString(game.gameFont, state.ToString() + " [" + timer[0] + "]", new Vector2(x, y - 8), Color.White);
            }
        }

        public override int graphicWidth()
        {
            return spgraphic.width;
        }

        public override int graphicHeight()
        {
            return spgraphic.height;
        }

        public override void onDeath()
        {
            if (state != State.Dead)
            {
                state = State.Dead;
                color = new Color(164, 0, 0, 255);
                timer[DEAD_ANIM_TIMER] = deathAnimDuration;

                if (weapon != null)
                {
                    weapon.onDrop();
                }
            }
        }

        public override bool onHit(Entity other)
        {
            base.onHit(other);

            if (other is NormalAxe)
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
            else if (other is Axe)
            {
                // Get MaD!
                if (other.x + other.graphicWidth() / 2 < x + graphicWidth() / 2)
                    facing = Dir.Left;
                else facing = Dir.Right;

                return false;
            }

            return false;
        }

        protected void spawnAxe()
        {
            Vector2 axePos = getHandPosition();
            NormalAxe normalAxe = new NormalAxe((int)axePos.X, (int)axePos.Y, this);
            setWeapon(normalAxe);
            world.add(normalAxe, "axe");
        }

        public void handleSeeAxe(Axe axeTarget)
        {
            bool waitIsItFlying = axeTarget.state == Axe.MovementState.Flying;
            if (waitIsItFlying)
            {
                // try catch
                if (Tools.random.Next(3) < 1)
                    tryGrabAnyAxe();
            }
            else
            {
                bool goForIt;
                if (axeTarget.holder != null)
                {   // Steal it?
                    goForIt = Tools.random.Next(80) < 1;
                }
                else
                {
                    goForIt = true;
                }

                if (goForIt)
                {
                    changeState(State.ChasingAxe);
                }
            }
        }

        public bool tryGrabAnyAxe()
        {
            // Can I grab any axe?
            bMask wrappedMask = generateWrappedMask(weaponCatchMask);
            bEntity entity = instancePlace(wrappedMask, "axe");
            if (entity != null)
            {
                if ((entity as Axe).holder != null)
                {
                    ((entity as Axe).holder).onAxeStolen();
                    (entity as Axe).holder = null;
                }
                (entity as Axe).onGrab(this);
                sfxGrab.Play();

                spgraphic.play("idle");
                changeState(State.GrabbedAxe);

                return true;
            }

            return false;
        }

        /**
         * IAXEHOLDER METHODS
         */
        public void setWeapon(IWeapon weapon)
        {
            this.weapon = weapon;
        }

        public void removeWeapon()
        {
            this.weapon = null;
        }

        public Player.Dir getFacing()
        {
            return facing;
        }

        public int getDirectionAsSign(Player.Dir dir)
        {
            return directionToSign(dir);
        }

        public Vector2 getPosition()
        {
            return pos;
        }

        public Vector2 getHandPosition()
        {
            if (facing == Dir.Left)
            {
                return new Vector2(x - 1, y + 13);
            }
            else
            {
                return new Vector2(x + graphicWidth() + 1, y + 13);
            }
        }

        public void onAxeStolen()
        {
            weapon = null;
        }
    }
}
