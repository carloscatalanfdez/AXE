using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;

using AXE.Game.Screens;
using AXE.Game.Entities;
using AXE.Game.Entities.Base;
using AXE.Game.Control;
using AXE.Game.Entities.Axes;

namespace AXE.Game.Entities
{
    class Axe : Entity, IWeapon
    {
        public enum MovementState { Idle, Grabbed, Flying, Stuck, Bouncing };
        public MovementState state;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        public PlayerData.Weapons type;
        public bMask flyingMask;
        public bMask idleMask;

        // Misc
        public int wrapCount;
        public int wrapLimit;

        // When grabbed
        public IWeaponHolder holder;

        // When thrown
        public bool justLaunched;
        public IWeaponHolder thrower;

        // When stuck
        public Entity stuckTo;  // Entity the axe is stuck to
        public Vector2 stuckOffset;  // Offset at (in stuckTo's coordinates) which the axe is stuck
        public Dir stuckToSide;  // StuckTo's facing direction when we got stuck
        public Dir stuckFacing;  // Axe's facing direction when we got stuck

        public float current_hspeed;
        public float current_vspeed;
        public float gravity;

        public SoundEffect sfxThrow;
        public SoundEffect sfxHit;
        public SoundEffect sfxDrop;
        public SoundEffect sfxGrab;
        public SoundEffect sfxHurt;

        public int xTraveledFlightDistance; // How far on the x axis has the axe been flying
        public int yTraveledFlightDistance; // How far on the y axis has the axe been flying

        public Axe(int x, int y, IWeaponHolder holder) : base(x, y)
        {
            this.holder = holder;  // can be null
        }

        public Axe(int x, int y)
            : this(x, y, null)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprStickSheet;
            initSoundEffects();
        }

        public override void init()
        {
            base.init();

            initGraphic();
            initSoundEffects();
            initHolderState();

            initParams();
        }

        virtual public void loadIdleMask()
        {
            if (idleMask == null)
            {
                idleMask = new bMask(x, y, 14, 14, 0, 0);
                idleMask.game = game;
            }

            _mask = idleMask;
        }

        virtual public void loadFlyMask()
        {
            if (flyingMask == null)
            {
                flyingMask = new bMask(0, 0, 0, 0);
                flyingMask.game = game;
            }

            /**
             * Find the width of the flying axe: it has to be as big as the current hspeed, except if the axe
             * hasn't covered said distance yet
             */ 
            int flyingWidth = (int)Math.Min(Math.Abs(current_hspeed), xTraveledFlightDistance);
            flyingWidth = Math.Max(1, flyingWidth);
            if (current_hspeed < 0)
            {
                flyingMask.w = flyingWidth;
                flyingMask.offsetx = idleMask.offsetx;
            }
            else if (current_hspeed > 0)
            {
                flyingMask.w = flyingWidth;
                flyingMask.offsetx = idleMask.offsetx + idleMask.w - flyingWidth;
            }
            else
            {
                flyingMask.w = idleMask.w;
                flyingMask.offsetx = idleMask.offsetx;
            }

            /**
             * Find the height of the flying axe: it has to be as big as the current vspeed, except if the axe
             * hasn't covered said distance yet
             */ 
            int flyingHeight = (int)Math.Min(Math.Abs(current_vspeed), yTraveledFlightDistance);
            flyingHeight = Math.Max(1, flyingHeight);
            if (current_vspeed < 0)
            {
                flyingMask.h = flyingHeight;
                flyingMask.offsety = idleMask.offsety;
            }
            else if (current_vspeed > 0)
            {
                flyingMask.h = flyingHeight;
                flyingMask.offsety = idleMask.offsety + idleMask.h - flyingHeight;
            }
            else
            {
                flyingMask.h = idleMask.h;
                flyingMask.offsety = idleMask.offsety;
            }

            _mask = flyingMask;
        }

        protected virtual void initParams()
        {
            loadIdleMask();
            attributes.Add("axe");
            current_hspeed = current_vspeed = 0;
            gravity = 0.5f;

            resetAxeFlight();

            type = PlayerData.Weapons.Stick;

            layer = 10;
        }

        protected virtual void initGraphic()
        {
            spgraphic = new bSpritemap((game as AxeGame).res.sprStickSheet, 14, 14);
            loadAnims();
        }

        protected virtual void loadAnims()
        {
            int[] fs = { 0 };
            spgraphic.add(new bAnim("grabbed", fs, 0.0f));
            int[] fss = { 4, 5, 6, 7 };
            spgraphic.add(new bAnim("cw-rotation", fss, 0.7f));
            int[] fsss = { 8, 9, 10, 11 };
            spgraphic.add(new bAnim("ccw-rotation", fsss, 0.7f));
            int[] fssss = { 12 };
            spgraphic.add(new bAnim("idle", fssss, 0.0f));
        }

        protected virtual void initHolderState()
        {
            if (holder == null)
            {
                spgraphic.play("idle");
                facing = Player.Dir.None;
                state = MovementState.Idle;
            }
            else
            {
                spgraphic.play("grabbed");
                facing = holder.getFacing();
                state = MovementState.Grabbed;
            }
        }

        protected virtual void initSoundEffects()
        {
            sfxThrow = (game as AxeGame).res.sfxThrow;
            sfxHit = (game as AxeGame).res.sfxHit;
            sfxDrop = (game as AxeGame).res.sfxDrop;
            sfxGrab = (game as AxeGame).res.sfxGrab;
            sfxHurt = (game as AxeGame).res.sfxHurt;
        }

        public override int graphicWidth()
        {
            return 14;
        }

        public override void onCollision(string type, bEntity other)
        {
            // Do nothing if the previous owner just threw it
            if ((other is IWeaponHolder) 
                && ((other as IWeaponHolder) == thrower) 
                && (xTraveledFlightDistance < (other as Entity).graphicWidth()))
            {
                return;
            } 
            else if (type == "enemy" && state == MovementState.Flying)
            {
                Entity entity = other as Entity;
                if (other.collidable)
                    onHitSolid(entity);
            }
            else if (type == "axe" && state == MovementState.Flying && (other as Axe).state == MovementState.Flying)
            {
                onBounce();
                // Make the other bounce so as not to overcome it!
                // TODO: Implement some kind of axe power checking
                // (I added the ! to the first phrase to align em)
                (other as Axe).onBounce();
            }
        }

        override public void onUpdate()
        {
            base.onUpdate();

            if (state == MovementState.Flying)
            {
                loadFlyMask();
            }
            else
            {
                loadIdleMask();
            }

            // Prepare step
            Vector2 moveTo = pos;

            if (holder != null)
                facing = holder.getFacing();

            switch (state)
            {
                case MovementState.Grabbed:
                    resetAxeFlight();

                    pos = holder.getHandPosition() - getGrabPosition();
                    break;
                case MovementState.Flying:
                    {
                        if (justLaunched && !collides(thrower as bEntity))
                        {
                            justLaunched = false;
                        }

                        moveTo.X += current_hspeed;
                        moveTo.Y += current_vspeed;

                        xTraveledFlightDistance += (int)Math.Abs(pos.X - moveTo.X);
                        yTraveledFlightDistance += (int)Math.Abs(pos.Y - moveTo.Y);

                        Vector2 remnant;
                        remnant = moveToContact(moveTo, "solid");

                        // We have been stopped
                        if (remnant.X != 0 || remnant.Y != 0)
                        {
                            // Stop accelerating if we have stopped
                            bEntity entity = instancePlace(moveTo, "solid");
                            onHitSolid(entity);
                        }

                        break;
                    }
                case MovementState.Bouncing:
                    {   
                        resetAxeFlight();

                        Vector2 remnant;

                        current_vspeed += gravity;

                        moveTo.Y += current_vspeed;
                        moveTo.X += current_hspeed;
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

                        if (remnant.Y != 0 && current_vspeed < 0)
                        {
                            // Touched ceiling
                            current_vspeed = 0;
                        }
                        else if (remnant.Y != 0 && current_vspeed > 0)
                        {
                            current_vspeed = 0;
                            state = MovementState.Idle;
                            sfxDrop.Play();
                        }
                        break;
                    }
                case MovementState.Stuck:
                    resetAxeFlight();

                    if (stuckTo != null)
                    {
                        if (stuckToSide == stuckTo.facing)
                        {
                            pos = stuckTo.pos + stuckOffset;
                            facing = stuckFacing;
                        }
                        else
                        {
                            pos.X = stuckTo.pos.X + stuckTo.graphicWidth() - graphicWidth() - stuckOffset.X;
                            pos.Y = stuckTo.pos.Y + stuckOffset.Y;
                            facing = stuckFacing == Dir.Left ? Dir.Right : Dir.Left;
                        }
                    }

                    break;
                case MovementState.Idle:
                    bool onair = !placeMeeting(x, y + 1, "solid");
                    if (onair)
                        onair = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);
                    if (onair)
                    {
                        state = MovementState.Bouncing;
                        facing = Dir.None;
                    }
                    break;
                default:
                    break;
            }

            int w = (world as LevelScreen).width;
            bool justWrapped =
                (previousPosition.X > w / 2 && (pos.X < w / 2/* || pos.X > w*/)
                    && facing == Dir.Right) ||
                (previousPosition.X < w / 2 && (pos.X > w / 2/* || pos.X < 0*/)
                    && facing == Dir.Left);

            if (justWrapped)
            {
                wrapCount++;
                if (wrapCount > wrapLimit)
                    world.remove(this);
                else if (wrapCount == wrapLimit)
                {
                    // you're close, buddy, next time don't wrap or you'll be out
                    wrappable = false;
                    // unless someone stops you, that is
                }
            }
        }

        public override void onUpdateEnd()
        {
            base.onUpdateEnd();

            if (state != MovementState.Stuck)
            {
                stuckTo = null;
            }

            switch (state)
            {
                case MovementState.Stuck:
                case MovementState.Idle:
                    spgraphic.play("idle");
                    if (facing == Player.Dir.Left)
                        spgraphic.flipped = true;
                    else if (facing == Player.Dir.Right)
                        spgraphic.flipped = false;

                    break;
                case MovementState.Grabbed:
                    spgraphic.play("grabbed");
                    if (facing == Player.Dir.Left)
                        spgraphic.flipped = true;
                    else if (facing == Player.Dir.Right)
                        spgraphic.flipped = false;

                    break;
                case MovementState.Flying:
                case MovementState.Bouncing:
                    if (facing == Player.Dir.Left)
                    {
                        spgraphic.play("ccw-rotation");
                    }
                    else if (facing == Player.Dir.Right)
                    {
                        spgraphic.play("cw-rotation");
                    }
                    else
                    {
                        spgraphic.play("idle");
                    }

                    break;
            }

            spgraphic.update();
        }

        public void resetAxeFlight()
        {
            wrapCount = 0;
            xTraveledFlightDistance = yTraveledFlightDistance = 0;
            wrappable = true;
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            spgraphic.render(sb, pos);
        }

        /* IWeapon implementation */
        public virtual Vector2 getGrabPosition()
        {
            return new Vector2(5, 6);
        }

        public void onGrab(IWeaponHolder holder)
        {
            wrapCount = 0;
            sfxGrab.Play();
            holder.setWeapon(this);
            spgraphic.play("grabbed");
            facing = holder.getFacing();
            state = MovementState.Grabbed;
            this.holder = holder;
            thrower = null;
        }

        public virtual void onThrow(int force, Player.Dir dir, Vector2 handPosition)
        {
            if (dir == Dir.Left)
            {
                onThrow((float) -force, 0.0f, handPosition);
            }
            else
            {
                onThrow((float) force, 0.0f, handPosition);
            }
        }

        public virtual void onThrow(float hspeed, float vspeed, Vector2 handPosition)
        {
            resetAxeFlight();

            // Holder is stored during flight
            pos = handPosition - getGrabPosition();
            thrower = holder;
            justLaunched = true;
            sfxThrow.Play();
            state = MovementState.Flying;
            current_hspeed = hspeed;
            current_vspeed = vspeed;
            if (holder != null)
            {
                holder.removeWeapon();
            }
            if (current_hspeed > 0)
            {
                facing = Dir.Right;
            }
            else if (current_hspeed < 0)
            {
                facing = Dir.Left;
            }
            else
            {
                facing = Dir.None;
            }

            holder = null;
        }

        public virtual void onDrop()
        {
            state = MovementState.Idle;
            thrower = holder;
            if (holder != null)
            {
                holder.removeWeapon();
                holder = null;
            }
        }

        public virtual void onBounce(bool playYourSound = true)
        {
            current_hspeed = -current_hspeed / 10;
            current_vspeed = -2;
            state = MovementState.Bouncing;
            if (playYourSound)
                sfxHit.Play();
        }

        public virtual void onHitSolid(bEntity entity)
        {
            if (justLaunched && entity is Player)
                return;

            // notify other entity
            AxeHitResponse hitResponse = null;
            if (entity != null && (entity is Entity))
            {
                hitResponse = (entity as Entity).onAxeHit(this);
            }

            if (hitResponse == null)
            {
                hitResponse = AxeHitResponse.generateDefaultResponse(this, entity);
            }

            hitResponse.applyChangesOnAxe(this);
        }

        public virtual void onStuck(bEntity entity)
        {
            if (justLaunched && entity is Player)
                return;

            current_hspeed = current_vspeed = 0;
            state = MovementState.Stuck;
            if (entity is Entity)
            {
                stuckTo = entity as Entity;
                stuckOffset = pos - entity.pos;
                stuckToSide = stuckTo.facing;
                stuckFacing = facing;

                sfxHurt.Play();
            }
            else
            {
                // otherwise it's a solid and the axe can remain there
                sfxHit.Play();
            }
        }

        public override Entity getKillOwner()
        {
            Entity killer = this;
            if (holder != null && holder is Entity)
            {
                killer = holder as Entity;
            }
            else if (this.thrower != null && thrower is Entity)
            {
                killer = thrower as Entity;
            }

            return killer;
        }
    }

    /**
     * Class that determines the behavior of an axe after a hit
     * This is the base behavior in which the axe just keeps on flying
     * after hitting anything
     */ 
    class AxeHitResponse
    {
        public AxeHitResponse()
        {
        }

        public virtual void applyChangesOnAxe(Axe axe)
        {
        }

        /**
         * Returns the excepted axe behavior after hitting a random solid/entity
         */
        public static AxeHitResponse generateDefaultResponse(Axe axe, bEntity target)
        {
            if (axe is NormalAxe)
            {
                return new AxeHitResponseStuck(target);
            }
            else
            {
                return new AxeHitResponseBounce();
            }
        }

        /**
         * Returns the excepted axe behavior after hitting a general Entity
         */
        public static AxeHitResponse generateDefaultEntityResponse(Axe axe, bEntity target)
        {
            return generateDefaultResponse(axe, target);
        }

        /**
         * Returns the excepted axe behavior after hitting an Enemy
         * Usually we want it to bounce if it's a tomahawk, or keep on flying (until a certain limit)
         * if it's a powered-up axe
         */
        public static AxeHitResponse generateDefaultEnemyResponse(Axe axe, bEntity target)
        {
            if (axe is NormalAxe)
            {
                // Todo: implement limit on this ones?
                return new AxeHitResponse(); // Keep on rocking in the free world
            }
            else
            {
                return new AxeHitResponseBounce();
            }
        }

        /**
         * Returns a stuck-on-entity/solid behavior
         */
        public static AxeHitResponse generateStuckResponse(bEntity stuckTo)
        {
            return new AxeHitResponseStuck(stuckTo);
        }

        /**
         * Returns a bounce behavior
         */
        public static AxeHitResponse generateBounceResponse()
        {
            return new AxeHitResponseBounce();
        }

        /**
         * Returns a redirect behavior, new direction is defined in polar coordinates
         */
        public static AxeHitResponse generateRedirectResponseWithAngle(float angle, float force)
        {
            AxeHitResponseRedirect hitResponse = new AxeHitResponseRedirect();
            hitResponse.setAngleForce(angle, force);
            return hitResponse;
        }

        /**
         * Returns a redirect behavior, new direction is defined in cartesia ncoordinates
         */
        public static AxeHitResponse generateRedirectResponseWithSpeed(float hspeed, float vspeed)
        {
            AxeHitResponseRedirect hitResponse = new AxeHitResponseRedirect();
            hitResponse.setSpeed(hspeed, vspeed);
            return hitResponse;
        }
    }

    /**
     * Class that determines the behavior of an stuck axe
     */
    class AxeHitResponseStuck : AxeHitResponse
    {
        public bEntity stuckTo;

        public AxeHitResponseStuck(bEntity stuckTo)
        {
            this.stuckTo = stuckTo;
        }

        public override void applyChangesOnAxe(Axe axe)
        {
            axe.onStuck(stuckTo);
        }
    }

    /**
     * Class that determines the behavior of an axe that bounces after a hit
     */
    class AxeHitResponseBounce : AxeHitResponse
    {
        public override void applyChangesOnAxe(Axe axe)
        {
            axe.onBounce();
        }
    }

    /**
     * Class that continues changes its direction after bouncing
     */
    class AxeHitResponseRedirect : AxeHitResponse
    {
        public float hspeed;
        public float vspeed;

        /**
         * Not tested!
         */
        public void setAngleForce(float angle, float force)
        {
            this.hspeed = (float) (force * Math.Acos(angle));
            this.vspeed = (float) (force * Math.Asin(angle));
        }

        public void setSpeed(float hspeed, float vspeed)
        {
            this.hspeed = hspeed;
            this.vspeed = vspeed;
        }

        public override void applyChangesOnAxe(Axe axe)
        {
            axe.holder = axe.thrower;
            axe.onThrow(hspeed, vspeed, axe.pos + axe.getGrabPosition() /* hac to keep the axe in place */);
        }
    }
}
