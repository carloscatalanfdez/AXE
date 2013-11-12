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
        public enum State { None, Idle, Walk, Throwing, Falling, Dead }
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

        IWeapon weapon;

        Vector2 moveTo;
        bMask watchMask;

        public State state;

        int hspeed;

        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int throwBaseTime, throwOptionalTime;

        SoundEffect sfxGrab;
        SoundEffect sfxThrow;

        int deathAnimDuration;

        public EvilAxeHolder(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/axethrower-sheet"), 17, 26);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("walk", new int[] { 0, 1 }, 0.3f));
            spgraphic.add(new bAnim("throw", new int[] { 1 }, 1.0f, false));
            spgraphic.add(new bAnim("jump", new int[] { 2 }));
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

            idleBaseTime = 80;
            idleOptionalTime = 80;
            walkBaseTime = 30;
            walkOptionalTime = 30;
            throwBaseTime = 50;
            throwOptionalTime = 20;

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            sfxGrab = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-evilpick");
            sfxThrow = game.Content.Load<SoundEffect>("Assets/Sfx/sfx-evilthrow");

            state = State.None;
            changeState(State.Idle);

            // Spawn with axe
            spawnAxe();
        }

        protected bool checkForGround(int x, int y)
        {
            bool onAir = !placeMeeting(x, y + 1, "solid");
            if (onAir)
                onAir = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);

            return !onAir;
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
                    case State.Throwing:
                        sfxThrow.Play();

                        weapon.onThrow(3, facing);
                        timer[0] = throwBaseTime + Tools.random.Next(throwOptionalTime) - throwOptionalTime;
                        
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
                        case State.Throwing:
                            changeState(State.Idle);
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

                // VERY IMPORTANT
                // When holding the mask, we need to hold the original _mask, since
                // mask itself is a property and will return a hacked wrapped mask sometimes
                bMask holdMyMaskPlease = _mask;
                mask = watchMask;

                bool sawYou = placeMeeting(x, y, "player", alivePlayerCondition);
                mask = holdMyMaskPlease; // thank you!

                if (sawYou)
                {
                    if (weapon != null)
                    {
                        changeState(State.Throwing);
                    }
                }
            }

            if (state == State.Walk || state == State.Falling)
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

            // Do this at the end, to make sure we're not going to throw this axe
            // (init is not called yet and it will to explode)
            if (state == State.Idle)
            {
                // Randomly spawn axe if needed
                if (Tools.random.Next(80) < 1 && weapon == null)
                {
                    sfxGrab.Play();
                    spawnAxe();
                }
            }

            spgraphic.flipped = (facing == Dir.Left);

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

        public void onDeath()
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

        bool alivePlayerCondition(bEntity me, bEntity other)
        {
            if (other is Player)
                return (other as Player).state != Player.MovementState.Death &&
                    (other as Player).state != Player.MovementState.Revive;
            else
                return false;
        }

        protected void spawnAxe()
        {
            Vector2 axePos = getHandPosition();
            NormalAxe normalAxe = new NormalAxe((int)axePos.X, (int)axePos.Y, this);
            setWeapon(normalAxe);
            world.add(normalAxe, "axe");
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
    }
}
