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
using AXE.Game.Utils;

namespace AXE.Game.Entities.Enemies
{
    class TerritorialRapier : Enemy, IHazardProvider, IHazard
    {
        public enum State { None, Idle, Seathing, Unseathing, Defending, Attacking, Attacked, Turn, Deflecting, Falling, Dead }
        const int CHANGE_STATE_TIMER = 0;
        const int DEAD_ANIM_TIMER = 1;
        const int COOL_DOWN_TIMER = 2;

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
        bMask attackZoneMask;
        bMaskList attackZoneWrappedMask;
        bMask backMask;
        bMaskList backWrappedMask;

        KillerRect weaponHitZone;

        public State state;

        int idleBaseTime, idleOptionalTime;
        int defendingBaseTime, defendingOptionalTime;
        int attackBaseTime, attackOptionalTime;
        int deflectingBaseTime, deflectingOptionalTime;
        int attackCoolDown;

        int deathAnimDuration;

        public TerritorialRapier(int x, int y, bool flipped)
            : base(x, y)
        {
            facing = flipped ? Dir.Left : Dir.Right;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprRapierSheet;
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprRapierSheet, 42, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0, 1 }, 0.05f));
            spgraphic.add(new bAnim("unseathe", new int[] { 0, 4, 5, 6, 8, 8 }, 0.44f, false));
            spgraphic.add(new bAnim("seathe", new int[] { 8, 8, 6, 5, 4, 0 }, 0.44f, false));
            spgraphic.add(new bAnim("defend", new int[] { 8, 9 }, 0.1f));
            spgraphic.add(new bAnim("attack", new int[] { 10 }, 1.0f, false));
            spgraphic.add(new bAnim("deflect", new int[] { 10 }, 0.1f, false));
            spgraphic.add(new bAnim("death", new int[] { 0 }));
            spgraphic.add(new bAnim("jump", new int[] { 0 }));
            spgraphic.add(new bAnim("turn", new int[] { 0 }, 0.3f, false));
            spgraphic.play("idle");

            mask.w = 26;
            mask.h = 25;
            mask.offsetx = 6;
            mask.offsety = 7;

            watchMask = new bMask(x, y, 75, 40);
            watchMask.game = game;
            bMask maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            bMask maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            watchWrappedMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            watchWrappedMask.game = game;

            attackZoneMask = new bMask(x, y, 30, 40);
            attackZoneMask.game = game;
            maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            attackZoneWrappedMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            attackZoneWrappedMask.game = game;

            backMask = new bMask(x, y, 40, 40);
            backMask.game = game;
            maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            backWrappedMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            backWrappedMask.game = game;

            vspeed = 0f;
            gravity = 0.5f;

            weaponHitZone = null;
            attackCoolDown = 20;

            deathAnimDuration = 50;

            idleBaseTime = 120;
            idleOptionalTime = 60;
            defendingBaseTime = 100;
            defendingOptionalTime = 80;
            attackBaseTime = 15;
            attackOptionalTime = 10;
            deflectingBaseTime = 5;
            deflectingOptionalTime = 5;

            deathFallThreshold = 20;

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
                        timer[CHANGE_STATE_TIMER] = idleBaseTime + Tools.random.Next(idleOptionalTime) - idleOptionalTime;
                        break;
                    case State.Attacked:
                        timer[CHANGE_STATE_TIMER] = attackBaseTime + Tools.random.Next(attackOptionalTime) - attackOptionalTime;

                        int xx, yy = 4;
                        if (facing == Dir.Right)
                            xx = 20;
                        else
                            xx = -10;
                        weaponHitZone = new KillerRect(x + xx, y + yy, 20, 27, Player.DeathState.ForceHit);
                        weaponHitZone.setOwner(this);
                        world.add(weaponHitZone, "hazard");

                        break;
                    case State.Deflecting:
                        timer[CHANGE_STATE_TIMER] = deflectingBaseTime + Tools.random.Next(deflectingOptionalTime) - deflectingOptionalTime;
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
                            changeState(State.Turn);
                            break;
                        case State.Attacked:
                            if (weaponHitZone != null)
                            {
                                world.remove(weaponHitZone);
                                weaponHitZone = null;
                            }

                            timer[COOL_DOWN_TIMER] = attackCoolDown;
                            changeState(State.Defending);
                            break;
                        case State.Defending:
                            changeState(State.Seathing);
                            break;
                        case State.Deflecting:
                            changeState(State.Defending);
                            break;
                    }
                    break;
                case DEAD_ANIM_TIMER:
                    break;
            }
        }

        protected bool shouldAttackPlayer()
        {
            Dir facingDir = facing;
            if (facingDir == Dir.Left)
                attackZoneMask.offsetx = _mask.offsetx - attackZoneMask.w;
            else
                attackZoneMask.offsetx = _mask.offsetx + _mask.w;
            attackZoneMask.offsety = (graphicHeight() - watchMask.h);

            bMask holdMyMaskPlease = _mask;
            bMask wrappedmask = generateWrappedMask(attackZoneMask, attackZoneWrappedMask);
            mask = wrappedmask;

            bEntity spottedEntity = instancePlace(x, y, "player", null, alivePlayerCondition);
            mask = holdMyMaskPlease; // thank you!

            if (spottedEntity != null)
            {
                return true;
            }

            return false;
        }

        protected bool shouldTurn()
        {
            Dir facingDir = facing;
            if (facingDir == Dir.Left)
                backMask.offsetx = _mask.offsetx + _mask.w - 10;
            else
                backMask.offsetx = _mask.offsetx - 30;
            backMask.offsety = (graphicHeight() - backMask.h);

            bMask holdMyMaskPlease = _mask;
            bMask wrappedmask = generateWrappedMask(backMask, backWrappedMask);
            mask = wrappedmask;

            bEntity spottedEntity = instancePlace(x, y, "player", null, alivePlayerCondition);
            mask = holdMyMaskPlease; // thank you!

            if (spottedEntity != null)
            {
                return true;
            }

            return false;
        }

        protected bool shouldDefend()
        {
            Dir facingDir = facing;
            if (facingDir == Dir.Left)
                watchMask.offsetx = _mask.offsetx - watchMask.w;
            else
                watchMask.offsetx = _mask.offsetx + _mask.w;
            watchMask.offsety = (graphicHeight() - watchMask.h);

            bMask holdMyMaskPlease = _mask;
            bMask wrappedmask = generateWrappedMask(watchMask, watchWrappedMask);
            mask = wrappedmask;

            bEntity spottedEntity = instancePlace(x, y, "player", null, alivePlayerCondition);
            mask = holdMyMaskPlease; // thank you!

            if (spottedEntity != null)
            {
                return true;
            }

            return false;
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

                        if (shouldDefend())
                        {
                            changeState(State.Unseathing);
                        }

                        break;
                    }
                case State.Turn:
                    {
                        spgraphic.play("turn");

                        if (spgraphic.currentAnim.finished)
                        {
                            turn();
                            changeState(State.Idle);
                        }
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
                case State.Seathing:
                    {
                        spgraphic.play("seathe");

                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Idle);
                        }
                        break;
                    }
                case State.Defending:
                    {
                        spgraphic.play("defend");

                        if (timer[COOL_DOWN_TIMER] < 0 && shouldAttackPlayer())
                        {
                            changeState(State.Attacking);
                        }
                        else if (!shouldDefend())
                        {
                            if (timer[CHANGE_STATE_TIMER] < 0)
                                timer[CHANGE_STATE_TIMER] = defendingBaseTime + Tools.random.Next(defendingOptionalTime) - defendingOptionalTime;  
                        }
                        else
                        {
                            timer[CHANGE_STATE_TIMER] = -1;                  
                        }

                        break;
                    }
                case State.Attacking:
                    {
                        spgraphic.play("attack");
                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Attacked);
                        }
                            
                        break;
                    }
                case State.Deflecting:
                    {
                        spgraphic.play("deflect");
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

            if (state == State.Idle || state == State.Defending)
            {
                if (shouldTurn())
                {
                    changeState(State.Turn);
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

            //sb.Draw(bDummyRect.sharedDummyRect(game), watchMask.rect, new Color(0.2f, 0.0f, 0.5f, 0.2f));
            //sb.Draw(bDummyRect.sharedDummyRect(game), attackZoneMask.rect, new Color(0.5f, 0.0f, 0.2f, 0.2f));
            //sb.Draw(bDummyRect.sharedDummyRect(game), backMask.rect, new Color(0.2f, 0.5f, 0.0f, 0.2f));

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
            if (state != State.Dead && state != State.Falling)
            {
                if (type == "hazard" && other is Axe)
                {
                    Axe axe = other as Axe;
                    if (!(axe is NormalAxe) && axe.state == Axe.MovementState.Flying)
                    {
                        int midPos = _mask.x + _mask.w / 2;
                        int axePos = axe.getRelativeXPos(midPos);
                        if ((facing == Dir.Left && axePos < midPos) || (facing == Dir.Right && axePos > midPos))
                        {
                            (other as Axe).onBounce();
                            changeState(State.Deflecting);
                        }
                    }
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
