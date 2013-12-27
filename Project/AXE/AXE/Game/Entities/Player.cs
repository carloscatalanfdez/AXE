using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;
using bEngine.Helpers;
using bEngine.Helpers.Transitions;

using AXE.Common;
using AXE.Game.Control;
using AXE.Game.Screens;
using AXE.Game.Entities.Base;
using AXE.Game.Utils;
using Microsoft.Xna.Framework.Media;
using AXE.Game.Entities.Enemies;

namespace AXE.Game.Entities
{
    class Player : Entity, IWeaponHolder
    {
        // Utilities
        public GameInput mginput;
        Random random;

        // Some declarations
        public enum MovementState { Idle, Walk, Jump, Ladder, Death, Attacking, Attacked, OnFire, Activate, Exit, Revive };
        public enum DeathState { None, Generic, Fall, ForceHit, Fire, DeferredBurning };
        public enum ActionState { None, Squid };
        public enum BodyState { Standing, Crouching };

        public const int EXIT_ANIM_TIMER = 1;
        public const int ACTIVATION_TIME_TIMER = 3;
        public const int DEATH_BY_FIRE_TIMER = 4;
        public int exitAnimationWaitTime;
        public int deathByFireTime;

        // Data
        public PlayerData data;

        // Graphic, movement vars
        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }
        public float hspeed;
        public float vspeed, gravity;

        // Accelerated movement
        public float current_hspeed;
        public float haccel;
        public float air_haccel;
        public bool isLanding;
        public float runSpeedFactor;
        public float jumpPower;
        public Dir jumpedFacing;
        public float jumpMaxSpeed;
        public bool fallingToDeath;
        public Vector2 fallingFrom;
        public int deathFallThreshold;
        public bool onair;

        // State params
        public int deathDelayTime;
        public bool playDeathAnim;
        public bool waitingLanding;
        public Axe axeToCatch;
        
        // State vars
        public MovementState previousState;
        public MovementState state;
        public ActionState action;
        public DeathState deathCause;
        public BodyState bodyState;

        // Step variables
        public Vector2 stepInitialPosition;
        public Vector2 moveTo;

        // Weapon holding variables
        protected IWeapon weapon;
        protected HotspotContainer hotspotContainer;
        protected bMask weaponCatchMask;
        protected int actionPressedSteps;
        protected int weaponCatchThreshold;
        
        // Sound effects
        protected List<SoundEffect> sfxSteps;
        protected SoundEffect sfxLanded;
        protected SoundEffect sfxCharge;
        protected SoundEffect sfxHit;

        public Vector2 currentPlatformDelta;

        public int powerUps
        {
            get { return data.powerUps; }
            set { data.powerUps = value; }
        }

        // Timers
        public int activationTime;

        // Debug
        bool floater;

        public MovementState movementState { get { return state; } }
        public ActionState actionState { get { return action; } }
        public bool onAir { get { return onair; } }
        public float dhspeed { get { return current_hspeed; } }
        public float dvspeed { get { return vspeed; } }

        public Player(int x, int y, PlayerData data)
            : base(x, y)
        {
            this.data = data;
            this.mginput = GameInput.getInstance(data.id);
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            if (data.id == PlayerIndex.One)
                spgraphic.image = (game as AxeGame).res.sprKnightASheet;
            else
                spgraphic.image = (game as AxeGame).res.sprKnightBSheet;

            loadSoundEffects();
        }

        override public void init()
        {
            base.init();

            /* Definitions & loads */
            // Fetch global random
            random = Utils.Tools.random;

            // Set attributes (for collisions & location in world)
            attributes.Add(ATTR_SOLID);
            attributes.Add("player");
            attributes.Add("moveable");

            // Load graphic & setup
            Texture2D sheet;
            if (data.id == PlayerIndex.One)
                sheet = (game as AxeGame).res.sprKnightASheet;
            else
                sheet = (game as AxeGame).res.sprKnightBSheet;
            spgraphic = new bSpritemap(sheet, 30, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0 }, 0.1f));
            spgraphic.add(new bAnim("walk", new int[] { 1, 2, 3, 2 }, 0.2f));
            spgraphic.add(new bAnim("jump", new int[] { 8 }, 0.0f));
            spgraphic.add(new bAnim("activate", new int[] { 5 }));
            spgraphic.add(new bAnim("death", new int[] { 24, 25, 26, 27, 27, 27, 28, 28, 29, 29, 29, 29, 29 }, 0.2f, false));
            spgraphic.add(new bAnim("death-forcehit", new int[] { 24, 25, 26, 27, 27, 27, 28, 28, 29, 29, 29, 29, 29}, 0.2f, false));
            int[] dissolveDeathFrames = new int[15];
            for (int i = 32; i < (32 + 15); i++)
                dissolveDeathFrames[i - 32] = i;
            spgraphic.add(new bAnim("death-dissolve", dissolveDeathFrames, 0.5f, false));
            spgraphic.add(new bAnim("revive", new int[] { 29, 28, 27, 26, 25, 24 }, 0.1f, false));
            spgraphic.add(new bAnim("squid", new int[] { 9 }));
            spgraphic.add(new bAnim("fall", new int[] { 12 } ));
            spgraphic.add(new bAnim("ladder", new int[] { 10, 11 }, 0.1f));
            spgraphic.add(new bAnim("readyweapon", new int[] { 0, 16, 17, 17, 17 }, 0.5f, false));
            spgraphic.add(new bAnim("air-readyweapon", new int[] { 8, 19, 20, 20, 20 }, 0.6f, false));
            spgraphic.add(new bAnim("thrownweapon", new int[] { 18, 18 }, 0.2f, false));
            spgraphic.add(new bAnim("air-thrownweapon", new int[] { 18, 18 }, 0.2f, false));
            spgraphic.add(new bAnim("crouch-idle", new int[] { 6 }));
            spgraphic.add(new bAnim("crouch-walk", new int[] { 6, 7 }, 0.2f));
            spgraphic.add(new bAnim("crouch-squid", new int[] { 6 }));
            spgraphic.add(new bAnim("onfire", new int[] { 48, 49, 50, 51 }, 0.35f));

            spgraphic.add(new bAnim("exit", new int[] { 4 }));
            // Hotspot config for anim frames
            hotspotContainer = new HotspotContainer("Assets/SpriteConfs/knight-hotspots.cfg");
            // Rendering layer
            layer = 0;
            
            // Load sound effects
            loadSoundEffects();

            // Debug defs
            floater = false;
            draggable = true;

            /* Paremter defs */
            loadParameters();

            /* Parameter inits */
            initParameters();

            /* Init */
            facing = Dir.Right;
            spgraphic.play("idle");
        }

        protected void loadParameters()
        {
            exitAnimationWaitTime = 15;
            activationTime = 15;            
            hspeed = 1.5f;
            gravity = 0.6f;
            haccel = 0.2f;
            air_haccel = 0f;
            runSpeedFactor = 2;
            jumpPower = 3.75842f;
            deathFallThreshold = 48;

            weaponCatchMask = new bMask(x, y, 
                (int)(graphicWidth() * 1.5f),
                (int)(graphicHeight() * 1.25f), 
                -(int)(graphicWidth()*0.25f), 
                -(int)(graphicHeight()*0.25f));
            weaponCatchThreshold = 5;
        }

        protected void initStandingMask()
        {
            _mask.w = 16;
            _mask.h = 24;
            _mask.offsetx = 7;
            _mask.offsety = 8;
        }

        protected void initCrouchMask()
        {
            _mask.w = 16;
            _mask.h = 17;
            _mask.offsetx = 7;
            _mask.offsety = 15;
        }

        protected void initParameters()
        {
            initStandingMask();

            current_hspeed = 0;
            vspeed = 0f;
            isLanding = false;
            deathDelayTime = 0;
            playDeathAnim = false;
            state = MovementState.Idle;
            action = ActionState.None;
            deathCause = DeathState.None;
            bodyState = BodyState.Standing;
            fallingFrom = Vector2.Zero;
            jumpMaxSpeed = 3.0f;
            axeToCatch = null;
            actionPressedSteps = 0;

            deathByFireTime = 60;

            currentPlatformDelta = Vector2.Zero;
        }

        protected void loadSoundEffects()
        {
            sfxSteps = new List<SoundEffect>();
            sfxSteps.Add((game as AxeGame).res.sfxStepA);
            sfxSteps.Add((game as AxeGame).res.sfxStepB);
            sfxSteps.Add((game as AxeGame).res.sfxStepC);

            sfxLanded = (game as AxeGame).res.sfxLanded;
            sfxCharge = (game as AxeGame).res.sfxCharge;
            sfxHit = (game as AxeGame).res.sfxPlayerHit;
        }

        public override int graphicWidth()
        {
            return spgraphic.width;
        }

        public override int graphicHeight()
        {
            return spgraphic.height;
        }

        public override void onUpdateBegin()
        {
            base.onUpdateBegin();

            previousState = state;
        }

        override public void onUpdate()
        {
            base.onUpdate();

            weaponCatchMask.update(x, y);

            // Prepare step
            color = Color.White;

            stepInitialPosition = pos;
            moveTo = pos;

            float _hspeed = hspeed;
            float _haccel = haccel;

            // Debug
            handleDebugRoutines();

            handleSoundEffects();

            // Updated action button pressed steps counter
            if (mginput.pressed(PadButton.b))
                actionPressedSteps = 1;
            else if (mginput.check(PadButton.b))
                actionPressedSteps++;
            else
                actionPressedSteps = 0;

            // Check for outside playfield death
            if (state != MovementState.Death && y + mask.h / 2 > (world as LevelScreen).height)
            {
                onDeath(DeathState.Fall);
            }

            Stairs ladder = (Stairs) instancePlace(x, y, "stairs");
            bool onladder = ladder != null && !isLadderBlocked(ladder);
            bool toLadder = false;

            onair = !placeMeeting(x, y + 1, "solid");
            if (onair)
                onair = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);
            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    state = MovementState.Idle;

                    fallingToDeath = false;
                    fallingFrom = Vector2.Zero;

                    handleAcceleratedMovement(ref _haccel, ref _hspeed);

                    if (onair)
                    {
                        if (vspeed < 0)
                        {
                            state = MovementState.Jump;
                        }
                        else
                        {
                            state = MovementState.Jump;
                            fallingToDeath = false;
                            fallingFrom = pos;
                        }
                    }
                    else
                    {
                        vspeed = 0f;
                        
                        if (mginput.pressed(PadButton.a))
                        {
                            state = MovementState.Jump;
                            isLanding = false;
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

                        // Ladders
                        if (action == ActionState.None)
                        {
                            if (onladder)
                            {
                                if (mginput.check(PadButton.up) || 
                                    mginput.check(PadButton.down) 
                                        && !mginput.check(PadButton.left) 
                                        && !mginput.check(PadButton.right))
                                {
                                    state = MovementState.Ladder;
                                    isLanding = false;
                                    current_hspeed = 0;
                                }
                            }
                            else if (mginput.check(PadButton.down) && placeMeeting(x, y + 1, "stairs"))
                            {
                                moveTo.Y += 2;
                                bEntity g = (bEntity)instancePlace(moveTo, "stairs");
                                if (g != null)
                                    moveTo.X = g.x;
                                toLadder = true;
                                state = MovementState.Ladder;
                                isLanding = false;
                                current_hspeed = 0;
                            }
                            else if (mginput.check(PadButton.up) && placeMeeting(x, y, "items"))
                            {
                                bEntity door = (bEntity)instancePlace(x, y, "items");
                                if (door is ExitDoor)
                                {
                                    ExitDoor exitDoor = (door as ExitDoor);
                                    if (exitDoor.type == ExitDoor.Type.ExitOpen)
                                    {
                                        state = MovementState.Exit;
                                        timer[EXIT_ANIM_TIMER] = exitAnimationWaitTime;
                                    }
                                }
                            }

                            if (mginput.check(PadButton.down) && !toLadder)
                            {
                                if ((state == MovementState.Idle || state == MovementState.Walk) && bodyState != BodyState.Crouching)
                                {
                                    crouch();
                                }
                            }
                            else if (bodyState != BodyState.Standing)
                            {
                                standUp();
                            }
                        }
                    }

                    break;
                case MovementState.Ladder:
                    if (onladder)
                    {
                        // Choose one offset (doesn't matter which one, really, both should be good)
                        bMask currentMask = mask;
                        if (currentMask is bMaskList)
                        {
                            int maskXOffset = Math.Max((currentMask as bMaskList).masks[0].offsetx, (currentMask as bMaskList).masks[1].offsetx);
                            moveTo.X = ladder.x - maskXOffset;
                        }
                        else
                        {
                            moveTo.X = ladder.x - mask.offsetx;
                        }

                        if (mginput.check(PadButton.up))
                            moveTo.Y -= hspeed;
                        else if (mginput.check(PadButton.down))
                            moveTo.Y += hspeed;
                        if ((mginput.check(PadButton.left) || mginput.check(PadButton.right)) 
                            && !(mginput.check(PadButton.down) || mginput.check(PadButton.up)))
                            if (placeMeeting(x, y + 1, "solid") || placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition))
                                state = MovementState.Idle;
                    }
                    else
                    {
                        state = MovementState.Jump;
                        if (mginput.check(PadButton.up))
                            vspeed = -2.0f;
                        else
                            vspeed = 0;
                    }
                    break;
                case MovementState.Jump:
                    if (onair)
                    {
                        handleOnAirMovement();
                    }
                    // Go to standing state, acknowleding the case in which
                    // the player is moving through a one way platform
                    else if (vspeed >= 0)
                    {
                        bool dead = false;
                        if (fallingToDeath)
                        {
                            // Has fall-safe powerup?
                            if ((powerUps & PowerUpPickable.HIGHFALLGUARD_EFFECT) != 0)
                            {
                                // Consume powerup
                                powerUps &= ~PowerUpPickable.HIGHFALLGUARD_EFFECT;
                            }
                            else
                            {
                                dead = true;
                            }
                        }

                        if (dead)
                        {
                            vspeed = 0;
                            onDeath(DeathState.Fall);
                        }
                        else
                        {
                            state = MovementState.Idle;
                        }
                    }
                    else // going up but not on air, wtf dude?
                    {
                        handleOnAirMovement();
                    }
                    break;
                case MovementState.OnFire:
                    break;
                case MovementState.Death:
                    if (!waitingLanding)
                    {
                        if (data.alive)
                        {
                            vspeed = 0;
                            mask.w = 0;
                            mask.h = 0;
                            bool deathAnimationFinished = spgraphic.currentAnim.finished; // TODO
                            if (deathAnimationFinished && data.alive)
                            {
                                // Animation end, restart...
                                Controller.getInstance().handlePlayerDeath(data);
                            }
                        }
                    }
                    else
                    {
                        if (onair)
                        {
                            handleOnAirMovement(/*false*/);
                            fallingToDeath = false;
                        }
                        else if (vspeed >= 0)
                        {
                            waitingLanding = false;
                            setDeathAnim(deathCause);
                        }
                    }

                    break;
                case MovementState.Attacking:
                    if (spgraphic.currentAnim.finished)
                    {
                        if (weapon != null)
                        {
                            // Update the axe pos, just in case
                            Vector2 handPos = getHandPosition();
                            weapon.onThrow(10, facing, getHandPosition());
                            data.weapon = PlayerData.Weapons.None;
                            state = MovementState.Attacked;
                            if (!onair)
                                spgraphic.play("thrownweapon");
                            else
                                spgraphic.play("air-thrownweapon");
                        }
                    }
                    else
                    {
                        if (onair)
                        {
                            handleOnAirMovement();
                        }
                        // Go to standing state, acknowleding the case in which
                        // the player is moving through a one way platform
                        else if (vspeed > 0)
                        {
                            if (fallingToDeath)
                            {
                                vspeed = 0;
                                onDeath(DeathState.Fall);
                            }
                        }
                    }

                    break;
                case MovementState.Attacked:
                    if (spgraphic.currentAnim.finished)
                    {
                        if (!onair)
                            state = MovementState.Idle;
                        else
                            state = MovementState.Jump;
                    }
                    else
                    {
                        {
                            if (onair)
                            {
                                handleOnAirMovement();
                            }
                            // Go to standing state, acknowleding the case in which
                            // the player is moving through a one way platform
                            else if (vspeed > 0)
                            {
                                // state = MovementState.Attacked; // WUT??
                            }
                        }
                    }

                    break;
                case MovementState.Activate:
                    spgraphic.play("activate");
                    break;
                case MovementState.Revive:
                    if (spgraphic.currentAnim.finished)
                    {
                        // Restart!
                        initParameters();
                        if (weapon == null)
                            (world as LevelScreen).spawnPlayerWeapon(data, this);
                    }
                    break;
            }

            moveTo.Y += vspeed;

            if (toLadder)
                pos = moveTo;
            else
            {
                Vector2 remnant = moveToContactSafe(moveTo);
                
                // We have been stopped if no decimals? test
                if (Math.Floor(Math.Abs(remnant.X)) > 0)
                {
                    // Stop accelerating if we have stopped
                    current_hspeed = 0;
                }

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
                    isLanding = true;
                    sfxSteps[0].Play();
                    sfxSteps[1].Play();
                }
            }

        }

        public void standUp()
        {
            bodyState = BodyState.Standing;
            initStandingMask();
        }

        public void crouch()
        {
            bodyState = BodyState.Crouching;
            initCrouchMask();
        }

        public Vector2 moveToContactSafe(Vector2 targetPosition)
        {
            Vector2 remnant;
            // Check wether we collide first with a solid or a onewaysolid,
            // and use that data to position the player character.
            Vector2 oldPos = pos;
            Vector2 remnantOneWay = moveToContact(targetPosition, "onewaysolid", onewaysolidCondition);
            Vector2 posOneWay = pos;
            pos = oldPos;
            Vector2 remnantSolid = moveToContact(targetPosition, "solid");
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

            return remnant;
        }

        public void handleActionButton()
        {
            // Handle axe
            // TODO: Study if substitute this with a canAct
            if (canDie())
            {
                if (mginput.pressed(PadButton.b))
                {
                    if (weapon != null)
                    {
                        if (!onair)
                        {
                            if (state != MovementState.Attacking && state != MovementState.Attacked)
                            {
                                state = MovementState.Attacking;
                                spgraphic.play("readyweapon");
                                sfxCharge.Play();
                            }
                        }
                        else
                        {
                            if (state != MovementState.Attacking && state != MovementState.Attacked)
                            {
                                state = MovementState.Attacking;
                                spgraphic.play("air-readyweapon");
                                sfxCharge.Play();
                            }
                        }
                    }
                    else // No weapon, pick / activate
                    {
                        bool pickedWeapon = false;

                        if (state != MovementState.Attacking && state != MovementState.Attacked && state != MovementState.Activate)
                        {
                            // generateWrappedMask will give us our current mask or the wrapped one, depends on where we are
                            // TODO: to wrap or not to wrap is reused from the player mask (showWrapEffect var), and not from this one. Should we fix?
                            bMask wrappedMask = generateWrappedMask(weaponCatchMask);
                            bEntity entity = instancePlace(wrappedMask, "axe");
                            if (entity != null)
                            {
                                // Console.WriteLine("Got axe by handleActionButton");

                                // Can't steal from other player, you moron!
                                if (!((entity as Axe).holder is Player))
                                {
                                    if ((entity as Axe).holder != null)
                                    {
                                        ((entity as Axe).holder).onAxeStolen();
                                        (entity as Axe).holder = null;
                                    }
                                    (entity as Axe).onGrab(this);
                                    data.weapon = (entity as Axe).type;
                                    pickedWeapon = true;
                                }
                            }
                        }

                        if (!pickedWeapon)
                        {
                            if (state == MovementState.Idle || state == MovementState.Walk)
                            {
                                IActivable activable = (instancePlace(x, y, "contraptions", null, activableCondition) as IActivable);
                                if (activable != null)
                                {
                                    if (activable.activate(this))
                                    {
                                        state = MovementState.Activate;
                                        // Will wait for end notification
                                        // timer[ACTIVATION_TIME_TIMER] = activationTime;
                                    }
                                }
                                else
                                {
                                    // spawn dagger
                                    int spawnX = facing == Dir.Left ? 0 : _mask.offsetx + _mask.w;
                                    FlameSpiritBullet bullet =
                                        new FlameSpiritBullet(x + spawnX, y + 15, spgraphic.flipped);
                                    
                                    world.add(bullet, "hazard");
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void onUpdateEnd()
        {
            base.onUpdateEnd();

            currentPlatformDelta = Vector2.Zero;
            handleActionButton();

            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    spgraphic.color = Color.White;
                    if (state == MovementState.Idle)
                    {
                        if (bodyState == BodyState.Crouching)
                        {
                            spgraphic.play("crouch-idle");
                        }
                        else
                        {
                            spgraphic.play("idle");
                        }
                    }
                    else if (state == MovementState.Walk)
                    {
                        if (bodyState == BodyState.Crouching)
                        {
                            spgraphic.play("crouch-walk");
                        }
                        else
                        {
                            spgraphic.play("walk");
                        }
                    }
                    if (action == ActionState.Squid)
                    {
                        if (bodyState == BodyState.Crouching)
                        {
                            spgraphic.play("crouch-squid");
                        }
                        else
                        {
                            spgraphic.play("squid");
                        }
                    }

                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
                case MovementState.Jump:
                    spgraphic.color = Color.Red;
                    if (fallingToDeath)
                        spgraphic.play("fall");
                    else
                        spgraphic.play("jump");
                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
                case MovementState.Ladder:
                    spgraphic.play("ladder");
                    spgraphic.flipped = false;
                    if (moveTo.Y - previousPosition.Y != 0)
                        spgraphic.currentAnim.resume();
                    else
                        spgraphic.currentAnim.pause();

                    break;
                case MovementState.OnFire:
                    spgraphic.play("onfire");

                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
                case MovementState.Death:
                    break;
                case MovementState.Exit:
                    spgraphic.play("exit");
                    break;
                default:
                    // Assume flipping is needed for any other state
                    if (facing == Dir.Right)
                        spgraphic.flipped = false;
                    else
                        spgraphic.flipped = true;
                    break;
            }

            if (isLanding)
                spgraphic.color = Color.Yellow;


            graphic.color = Color.White;

            spgraphic.update();
        }

        public void handleAcceleratedMovement(ref float _haccel, ref float _hspeed)
        {
            float speed = _hspeed;
            float accel = _haccel;
            if (bodyState == BodyState.Crouching)
            {
                speed /= 2;
                accel /= 2;
            }

            if (mginput.check(PadButton.left))
            {
                // Going right - squid
                if (current_hspeed > 0)
                {
                    action = ActionState.Squid;
                    current_hspeed -= accel * 2;
                }
                else
                {
                    action = ActionState.None;
                    current_hspeed = Math.Max(current_hspeed - accel, -speed);
                }
                moveTo.X += current_hspeed;
                facing = Dir.Left;
                state = MovementState.Walk;
                // If has started moving, hasn't just landed
                isLanding = false;
            }
            else if (mginput.check(PadButton.right))
            {
                if (current_hspeed < 0)
                {
                    action = ActionState.Squid;
                    current_hspeed += accel * 2;
                }
                else
                {
                    action = ActionState.None;
                    current_hspeed = Math.Min(current_hspeed + accel, speed);
                }
                moveTo.X += current_hspeed;
                facing = Dir.Right;
                state = MovementState.Walk;
                // If has started moving, hasn't just landed
                isLanding = false;
            }
            else
            {
                action = ActionState.None;

                moveTo.X += current_hspeed;

                // Decelerate
                float temp_haccel = accel;
                if (isLanding)
                    temp_haccel = 2 * accel;
                if (Math.Abs(current_hspeed) > temp_haccel)
                {
                    if (current_hspeed > 0)
                        current_hspeed -= 2 * temp_haccel;
                    else if (current_hspeed < 0)
                        current_hspeed += 2 * temp_haccel;
                }
                else
                {
                    isLanding = false;
                    current_hspeed = 0;
                }
            }   
        }

        public void handleOnAirMovement(bool controlAvailable = true)
        {
            
            if (controlAvailable && mginput.released(PadButton.a) && vspeed < 0)
                vspeed /= 2;

            if (!floater)
                vspeed += gravity;

            if (controlAvailable)
            {
                if (mginput.check(PadButton.left))
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
                else if (mginput.check(PadButton.right))
                {
                    if (current_hspeed < 0)
                    {
                        action = ActionState.Squid;
                        current_hspeed += air_haccel/* * 2*/;
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
            }

            if (vspeed > 0 
                /*&& state != MovementState.Attacking && state != MovementState.Attacked */
                && fallingFrom == Vector2.Zero)
            {
                //state = MovementState.Jump;
                fallingToDeath = false;
                fallingFrom = pos;
            }

            if (vspeed > 0 && pos.Y - fallingFrom.Y >= deathFallThreshold)
            {
                fallingToDeath = true;
            }

            moveTo.X += current_hspeed;
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
                case EXIT_ANIM_TIMER:
                    ExitDoor door = (instancePlace(x, y, "items") as ExitDoor);
                    if (door != null)
                    {
                        if (door.onPlayerExit())
                        {
                            if (weapon != null)
                                world.remove((weapon as bEntity));
                            attributes.Remove(ATTR_SOLID);
                            collidable = false;
                            visible = false;
                        }
                    }
                    break;
                case ACTIVATION_TIME_TIMER:
                    state = MovementState.Idle;
                    break;
                case DEATH_BY_FIRE_TIMER:
                    onDeath(DeathState.Generic);
                    break;
            }
        }

        public override bool onHit(Entity other)
        {
            if (other == axeToCatch)
            {
                if (other.facing == Dir.Left)
                    other.facing = Dir.Right;
                else
                    other.facing = Dir.Left;

                return true;
            }

            return false;
        }

        public override void onCollision(string type, bEntity other)
        {
            if (!canDie())
                return;

            if (type == "solid")
                // This should not happen!
                color = Color.Turquoise;
            else if (type == "enemy" || type == "player")
            {
                if (type == "player")
                    Console.WriteLine("PVP " + this.data.id + " vs " + (other as Player).data.id);
                // First, reposition
                if (other.attributes.Contains(ATTR_SOLID))
                {
                    pos.X = stepInitialPosition.X;
                    // Allow ethereal entities
                    if (placeMeeting(x, y, type))
                    {
                        if (state == MovementState.Ladder || bodyState == BodyState.Crouching)
                        {
                            // Just ignore him when you're on a ladder for now
                            pos = stepInitialPosition;
                        }
                        else
                        {
                            // If it doesn't work (still colliding) jump or something
                            // Check first if the enemy is on top, so to not jump throw it
                            if (vspeed < 0 && other.y + (other as Entity).graphicHeight() / 2 < y)
                            {
                                if (!handleJumpHit())
                                {
                                    vspeed = 0;
                                    pos.Y = stepInitialPosition.Y;
                                }
                            }
                            else
                            {
                                state = MovementState.Jump;
                                vspeed = -jumpPower / 2;

                                // Wait: does the collision make sense
                                float otherEffectiveXPos = other.x + (other as Entity).graphicWidth() / 2;
                                float effectiveXPos = x + graphicWidth() / 2;
                                if (Math.Abs(other.x - x) > Math.Max((other as Entity).graphicWidth(), graphicWidth()))
                                {
                                    // They're actually really far appart, so only one has wrapped
                                    // Find it and get the pos on the other side of the screen
                                    if (otherEffectiveXPos < 0)
                                    {
                                        otherEffectiveXPos += (world as LevelScreen).width;
                                    }
                                    else if (other.x + (other as Entity).graphicWidth() > ((world as LevelScreen).width))
                                    {
                                        otherEffectiveXPos -= (world as LevelScreen).width;
                                    }
                                    else if (effectiveXPos < 0)
                                    {
                                        effectiveXPos += (world as LevelScreen).width;
                                    }
                                    else if (x + graphicWidth() > ((world as LevelScreen).width))
                                    {
                                        effectiveXPos -= (world as LevelScreen).width;
                                    }

                                }
 
                                // Bounce'im!
                                if (otherEffectiveXPos < effectiveXPos)
                                {
                                    jumpedFacing = Dir.Right;
                                    facing = Dir.Left;
                                    current_hspeed = getDirectionAsSign(Dir.Right) * hspeed;
                                }
                                else if (otherEffectiveXPos > effectiveXPos)
                                {
                                    jumpedFacing = Dir.Left;
                                    facing = Dir.Right;
                                    current_hspeed = getDirectionAsSign(Dir.Left) * hspeed;
                                }
                            }
                        }
                    }
                }
            }
            else if (type == "hazard")
            {
                if (other != null && other is IHazard)
                {
                    IHazard hazard = (other as IHazard);
                    IHazardProvider killer = hazard.getOwner();
                    if (killer != null)
                    {
                        killer.onSuccessfulHit(this);
                        hazard.onHit();
                        onDeath(hazard.getType());
                    }
                    else
                    {
                     //   hazard.onHit();
                     //   onDeath(DeathState.Generic);
                    }
                }
            }
            else if (type == "axe")
            {
                Axe axe = (other as Axe);
                if (axe.state == Axe.MovementState.Flying && !axe.justLaunched)
                {
                    axeToCatch = (other as Axe);
                    if (actionPressedSteps > 0 && actionPressedSteps < weaponCatchThreshold)
                    {
                        if (state != MovementState.Attacking && state != MovementState.Attacked && state != MovementState.Activate)
                        {
                            // Console.WriteLine("Got axe by onCollision");
                            bEntity entity = other;
                            if (entity != null)
                            {
                                if ((entity as Axe).holder != null)
                                {
                                    ((entity as Axe).holder).onAxeStolen();
                                    (entity as Axe).holder = null;
                                }
                                (entity as Axe).onGrab(this);
                                data.weapon = (entity as Axe).type;
                            }
                        }
                    }
                    else
                    {
                        axeToCatch.onHitSolid(this);
                        onDeath(DeathState.Generic);
                    }
                }
            }
        }

        /** Used when waiting for landing to play anim! **/
        public void setDeathAnim(DeathState type)
        {
            switch (type)
            {
                case DeathState.None:
                case DeathState.Generic:
                case DeathState.Fall:
                    spgraphic.play("death");
                    break;
                case DeathState.ForceHit:
                    spgraphic.play("death-forcehit");
                    break;
                case DeathState.Fire:
                    spgraphic.play("death-dissolve");
                    break;
            }
        }

        public void onDeath(DeathState type = DeathState.Generic)
        {
            if (canDie())
            {
                // TODO: Use type to change anim
                switch (type)
                {
                    case DeathState.None:
                    case DeathState.Generic:
                    case DeathState.Fall:
                    case DeathState.ForceHit:
                        state = MovementState.Death;

                        sfxHit.Play();
                        deathCause = type;
                        if (onair && type != DeathState.Fire)
                            waitingLanding = true;
                        else
                        {
                            waitingLanding = false;
                            setDeathAnim(type);
                            if (type == DeathState.Fire)
                            {
                                if (weapon != null)
                                {
                                    weapon.onDrop();
                                    weapon = null;
                                }
                            }
                        }
                        break;
                    case DeathState.DeferredBurning:
                        if (state != MovementState.OnFire && state != MovementState.Death)
                        {
                            timer[DEATH_BY_FIRE_TIMER] = deathByFireTime;
                            state = MovementState.OnFire;
                        }
                        break;
                }
            }
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            if (bConfig.DEBUG)
            {
                sb.Draw(bDummyRect.sharedDummyRect(game), weaponCatchMask.rect, new Color(0.2f, 0.2f, 0.2f, 0.2f));
            }
            base.render(dt, sb);
            spgraphic.render(sb, pos);
            Color c = spgraphic.color;
            spgraphic.color = c;
            // sb.DrawString(game.gameFont, debugText, new Vector2(x, y - 8), Colors.white);
        }

        public bool isFlipped()
        {
            return spgraphic.flipped;
        }

        public bool activableCondition(bEntity me, bEntity other)
        {
            return (other is IActivable);
        }

        public override void onActivationEndNotification()
        {
            state = MovementState.Idle;
        }

        public bool isLadderBlocked(Stairs ladder)
        {
            // Compute position of the player after grabbing the ladder
            int toX;
            bMask currentMask = mask;
            if (currentMask is bMaskList)
            {
                int maskXOffset = Math.Max((currentMask as bMaskList).masks[0].offsetx, (currentMask as bMaskList).masks[1].offsetx);
                toX = ladder.x - maskXOffset;
            }
            else
            {
                toX = ladder.x - mask.offsetx;
            }

            // is there anything on that pos?
            if (placeMeeting(new Vector2(toX, pos.Y), new String[] {"enemy", "solid"}))
            {
                return true;
            }
            else {
                return false;
            }
        }

        /* IWeaponHolder implementation */
        public void setWeapon(IWeapon weapon)
        {
            this.weapon = weapon;
        }

        public void removeWeapon()
        {
            this.weapon = null;
        }

        public Vector2 getPosition()
        {
            return pos;
        }

        public Vector2 getHandPosition()
        {
            Vector2 hand = new Vector2(x, y);

            int curFrame = spgraphic.currentAnim.frame;

            // Right and Left direction hotspots for the current frame
            if (!hotspotContainer.hotspots.ContainsKey(curFrame))
            {
                // default value
                curFrame = -1;
            }
            Vector2[] hotspotsPoints = hotspotContainer.hotspots[curFrame];

            if (facing == Dir.Right)
            {
                hand += hotspotsPoints[0];
            }
            else
            {
                hand += hotspotsPoints[1];
            }

            return hand;
        }

        public Dir getFacing()
        {
            return facing;
        }

        /** 20131102, RDLH: This should not be in the interface! **/
        /** Then why the fuck did you add it there D: **/
        public int getDirectionAsSign(Dir dir)
        {
            return directionToSign(dir);
        }

        public void onAxeStolen()
        {
            weapon = null;
            data.weapon = PlayerData.Weapons.None;
            if (state == MovementState.Attacking || state == MovementState.Attacked)
            {
                // Dude stop it
                state = MovementState.Idle;
            }
        }

        /* End of IWeaponHolder implementation */

        bool playedStepEffect = false;
        public void handleSoundEffects()
        {
            float relativeX = pos.X / (world as LevelScreen).width - 0.5f;
            switch (state)
            {
                case MovementState.Walk:
                    int currentFrame = spgraphic.currentAnim.frame;
                    if (currentFrame == 2 && !playedStepEffect)
                    {
                        playedStepEffect = true;
                        sfxSteps[random.Next(sfxSteps.Count)].Play(1f, 0.0f, relativeX);
                    }
                    else if (currentFrame != 2)
                        playedStepEffect = false;

                    break;
                default:
                    break;
            }
        }

        void handleDebugRoutines()
        {
            if (beingDragged)
            {
                if (canDie())
                {
                    state = MovementState.Jump;
                    fallingToDeath = false;
                    vspeed = 0;
                }
            }
        }

        public void onCollectItem(Item item)
        {
            if (item is Coin)
            {
                data.collectedCoins += (item as Coin).value;
            }
            else if (item is PowerUpPickable)
            {
                powerUps |= (item as PowerUpPickable).effect;
            }
        }

        public void revive()
        {
            spgraphic.play("revive");
            data.alive = true;
            state = MovementState.Revive;
            // Place him on ground
            initStandingMask();
            // moveToContactSafe(new Vector2(x, (world as LevelScreen).height));
            while (!placeMeeting(x, y + 1, new string[] { "onewaysolid", "solid" }))
                y++;
        }

        public bool canDie()
        {
            return (state != MovementState.Death &&
                state != MovementState.Exit &&
                state != MovementState.Revive);
        }

        /* IPlatformUser implementation */
        override public void onPlatformMovedWithDelta(Vector2 delta, Entity platform)
        {
            if (state != MovementState.Jump || (state == MovementState.Jump && vspeed > 0))
            {
                currentPlatformDelta = delta;
                base.onPlatformMovedWithDelta(delta, platform);
            }    
        }
    }
}
