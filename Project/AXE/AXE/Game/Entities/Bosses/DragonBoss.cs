using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using bEngine;
using AXE.Game.Entities.Enemies;
using Microsoft.Xna.Framework.Graphics;
using AXE.Game.Screens;
using AXE.Game.Utils;
using AXE.Game.Entities.Axes;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities.Bosses
{
    class DragonBoss : Boss, IHazardProvider
    {
        public enum State { None, Idle, IdleTransition, AttackingTransition, Attacking, Attacked, Dead }
        public enum HeightLevel { None, Low, Mid, High }
        const int CHANGE_STATE_TIMER = 0;
        const int DEAD_ANIM_TIMER = 1;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        bMask watchMask;

        public State state;
        public HeightLevel currHeight;
        public HeightLevel prevHeight;

        int attackTime;

        int idleBaseTime, idleOptionalTime;
        int throwIdleTime;

        int deathAnimDuration;

        public DragonBoss(int x, int y)
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

            spgraphic = new bSpritemap((game as AxeGame).res.sprDragonBossSheet, 116, 138);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("attack-high", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("attack-mid", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("attack-low", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("idle-high", new int[] { 0 }));
            spgraphic.add(new bAnim("idle-mid", new int[] { 0 }));
            spgraphic.add(new bAnim("idle-low", new int[] { 0 }));
            spgraphic.add(new bAnim("high-to-idle", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("mid-to-idle", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("low-to-idle", new int[] { 0 }, 1.0f, false));
            spgraphic.add(new bAnim("death", new int[] { 0 }));
            spgraphic.play("idle");

            mask.w = 100;
            mask.h = 8;
            mask.offsetx = 8;
            mask.offsety = 130;

            watchMask = new bMask(x, y, (world as LevelScreen).width, graphicWidth());
            watchMask.game = game;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            deathAnimDuration = 50;

            throwIdleTime = 160;

            facing = Dir.Right;

            attackTime = 8;

            currHeight = prevHeight = HeightLevel.None;
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
                        timer[CHANGE_STATE_TIMER] = throwIdleTime;
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
                            // Sniff random height
                            break;
                        case State.Attacked:
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

            switch (state)
            {
                case State.Idle:
                    spgraphic.play("idle");
                    break;
                case State.Attacking:
                    if (spgraphic.currentAnim.finished)
                    {
                        shoot();
                        changeState(State.Attacked);
                    }
                    break;
            }

            if (state == State.Idle)
            {
                if (isPlayerOnSight(facing, false, new String[] { "solid" }, watchMask, null))
                {
                    // Yeah, let's go
                    changeState(State.Attacking);
                }
            }

            handleSoundEffects();

            // Uberdebuggo temporal thingie!
            if (mouseHover && input.check(Microsoft.Xna.Framework.Input.Keys.D))
                world.remove(this);
        }

        public override void onUpdateEnd()
        {
            switch (state)
            {
                case State.Attacking:
                    // switch depending on height
                    spgraphic.play("attack-mid");
                    break;
                default:
                    spgraphic.play("idle");
                    break;
            }
        }

        public void sniffHeight(HeightLevel height)
        {
            prevHeight = currHeight;
            currHeight = height;
            state = State.IdleTransition;
        }

        private void shoot()
        {
            // spawn dagger
            int spawnX =  _mask.offsetx + _mask.w;
            FireBullet bullet = new FireBullet(x + spawnX, y + 15, false /* not flipped */);
            bullet.setOwner(this);
            world.add(bullet, "hazard");
        }

        private HeightLevel getHeightFromInt(int value)
        {
            switch (value)
            {
                case 0:
                    return HeightLevel.Low;
                case 1:
                    return HeightLevel.Mid;
                case 2:
                    return HeightLevel.High;
                default:
                    return HeightLevel.None;
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (bConfig.DEBUG)
            {
                watchMask.render(sb);
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

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
            // tamed = true;
        }

    }

     class FireBullet : KillerRect
    {
        bSpritemap sprite;

        // Parameters
        int speed;

        // State vars
        bool flipped;
        bool isBurningOut = false;

        public FireBullet(int x, int y, bool flipped)
            : base(x, y, 20, 16, Player.DeathState.DeferredBurning)
        {
            this.flipped = flipped;
        }

        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprFireBulletSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 15;
            mask.h = 3;
            mask.offsetx = 4;
            mask.offsety = 2;

            sprite = new bSpritemap((game as AxeGame).res.sprFireBulletSheet, 20, 16);
            sprite.add(new bAnim("idle", new int[] { 0, 1, 2, 3 }, 0.2f));
            sprite.add(new bAnim("burnout", new int[] { 4, 5, 6, 7 }, 0.3f, false));

            sprite.play("idle");
            sprite.flipped = flipped;
            if (flipped)
            {
                x -= sprite.width;
            }

            speed = 5;

            if (y + sprite.height < 0 || y > (world as LevelScreen).height)
                world.remove(this);
        }

        public override void onHit()
        {
            base.onHit();

            isBurningOut = true;
        }

        public override void update()
        {
            base.update();

            if (flipped)
                x -= speed;
            else
                x += speed;

            if (x + sprite.width < 0 ||
                x > (world as LevelScreen).width)
                world.remove(this);

            if (isBurningOut)
            {
                sprite.play("burnout");
                if (sprite.currentAnim.finished)
                {
                    world.remove(this);
                }
            }

            sprite.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            sprite.render(sb, pos);
        }
    }
}
