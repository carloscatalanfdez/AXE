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

        bMask idleWatchMask;
        bMask sniffWatchMask;

        public State state;
        public State movementState { get { return state; } }

        public HeightLevel currHeight;

        int idleBaseTime, idleOptionalTime;
        int coolDownTime;

        int deathAnimDuration;

        public DragonBoss(int x, int y)
            : base(x, y)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprDragonBossSheet;
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap((game as AxeGame).res.sprDragonBossSheet, 136, 136);
            spgraphic.add(new bAnim("idle", new int[] { 0, 1, 2 }, 0.2f));
            spgraphic.add(new bAnim("attack-high", new int[] { 9 }, 0.3f, false));
            spgraphic.add(new bAnim("attack-mid", new int[] { 16 }, 0.3f, false));
            spgraphic.add(new bAnim("attack-low", new int[] { 23 }, 0.3f, false));
            spgraphic.add(new bAnim("idle-to-high", new int[] { 0, 7, 8 }, 0.4f, false));
            spgraphic.add(new bAnim("idle-to-mid", new int[] { 0, 14, 15 }, 0.4f, false));
            spgraphic.add(new bAnim("idle-to-low", new int[] { 0, 21, 22 }, 0.4f, false));
            spgraphic.add(new bAnim("high-to-idle", new int[] { 8, 7, 0 }, 0.3f, false));
            spgraphic.add(new bAnim("mid-to-idle", new int[] { 15, 14, 0 }, 0.3f, false));
            spgraphic.add(new bAnim("low-to-idle", new int[] { 22, 21, 0 }, 0.3f, false));
            spgraphic.add(new bAnim("death", new int[] { 0 }));
            spgraphic.play("idle");

            mask.w = 70;
            mask.h = 120;
            mask.offsetx = 23;
            mask.offsety = 16;

            idleWatchMask = new bMask(x, y, (world as LevelScreen).width, graphicWidth());
            idleWatchMask.game = game;

            sniffWatchMask = new bMask(x, y, (world as LevelScreen).width, 40);
            sniffWatchMask.game = game;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            deathAnimDuration = 50;

            coolDownTime = 30;

            facing = Dir.Right;

            currHeight = HeightLevel.None;
            state = State.None;

            changeState(State.Idle);
            
            attributes.Add(ATTR_SOLID);
        }

        public void changeState(State newState)
        {
            if (newState != state)
            {
                bool performChange = true;
                timer[CHANGE_STATE_TIMER] = -1;
                switch (newState)
                {
                    case State.Idle:
                        timer[CHANGE_STATE_TIMER] = idleBaseTime + Tools.random.Next(idleOptionalTime) - idleOptionalTime;
                        break;
                    case State.Attacked:
                        timer[CHANGE_STATE_TIMER] = coolDownTime;
                        break;
                    case State.IdleTransition:
                        currHeight = HeightLevel.None;
                        break;
                    default:
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
                            int random = Tools.random.Next(300);
                            switch (random)
                            {
                                case 0:
                                    moveToHeight(HeightLevel.High);
                                    break;
                                case 1:
                                    moveToHeight(HeightLevel.Mid);
                                    break;
                                case 2:
                                    moveToHeight(HeightLevel.Low);
                                    break;
                            }

                            break;
                        case State.Attacked:
                            HeightLevel resultHeight = sniff();
                            if (resultHeight != HeightLevel.None)
                            {
                                // Again!
                                changeState(State.Attacking);
                            }
                            else
                            {
                                moveToHeight(HeightLevel.None);
                            }
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
                    {
                        spgraphic.play("idle");

                        HeightLevel resultHeight = sniff();
                        if (resultHeight != HeightLevel.None)
                        {
                            moveToHeight(resultHeight);
                            changeState(State.AttackingTransition);
                        }
                        else
                        {
                            currHeight = HeightLevel.None;
                        }
                        break;
                    }
                case State.IdleTransition:
                    {
                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Idle);
                        }
                        break;
                    }
                case State.AttackingTransition:
                    {
                        if (spgraphic.currentAnim.finished)
                        {
                            changeState(State.Attacking);
                        }
                        break;
                    }
                case State.Attacking:
                    {
                        if (spgraphic.currentAnim.finished)
                        {
                            shoot();
                            changeState(State.Attacked);
                        }
                        break;
                    }
                case State.Attacked:
                    break;
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
                    switch (currHeight)
                    {
                        case HeightLevel.High:
                            spgraphic.play("attack-high");
                            break;
                        case HeightLevel.Mid:
                            spgraphic.play("attack-mid");
                            break;
                        case HeightLevel.Low:
                            spgraphic.play("attack-low");
                            break;
                    }
                    break;
                case State.AttackingTransition:
                    switch (currHeight)
                    {
                        case HeightLevel.High:
                            spgraphic.play("idle-to-high");
                            break;
                        case HeightLevel.Mid:
                            spgraphic.play("idle-to-mid");
                            break;
                        case HeightLevel.Low:
                            spgraphic.play("idle-to-low");
                            break;
                    }
                    break;
                case State.IdleTransition:
                    switch (currHeight)
                    {
                        case HeightLevel.High:
                            spgraphic.play("high-to-idle");
                            break;
                        case HeightLevel.Mid:
                            spgraphic.play("mid-to-idle");
                            break;
                        case HeightLevel.Low:
                            spgraphic.play("low-to-idle");
                            break;
                    }
                    break;
                case State.Dead:
                    spgraphic.play("death");
                    break;
                default:
                case State.Idle:
                    spgraphic.play("idle");
                    break;
            }
        }

        public void moveToHeight(HeightLevel height)
        {
            if (currHeight != HeightLevel.None)
            {
                if (height != HeightLevel.None)
                {
                    changeState(State.IdleTransition);
                }
                else
                {
                    changeState(State.Idle);
                }
            }
            else
            {
                if (height != HeightLevel.None)
                {
                    changeState(State.AttackingTransition);
                }
                else
                {
                    changeState(State.Idle);
                }
            }

            currHeight = height;
        }

        private HeightLevel sniff()
        {
            bMask holdMyMaskPlease = _mask;
            bMask watchMask;

            switch (currHeight)
            {
                case HeightLevel.High:
                    sniffWatchMask.y = 50;
                    watchMask = sniffWatchMask;
                    watchMask.y = 50;
                    break;
                case HeightLevel.Mid:
                    sniffWatchMask.y = 110;
                    watchMask = sniffWatchMask;
                    break;
                case HeightLevel.Low:
                    sniffWatchMask.y = 170;
                    watchMask = sniffWatchMask;
                    break;
                default:
                case HeightLevel.None:
                    watchMask = idleWatchMask;
                    break;
            }

            mask = watchMask;
            bEntity spottedEntity = instancePlace(x, y, "player", null, alivePlayerCondition);
            mask = holdMyMaskPlease; // thank you!

            if (spottedEntity != null && spottedEntity is Player)
            {
                if (spottedEntity.y < 72)
                {
                    return HeightLevel.High;
                }
                else if (spottedEntity.y < 130)
                {
                   return HeightLevel.Mid;
                }
                else if (spottedEntity.y < 190)
                {
                    return HeightLevel.Low;
                }
                else
                {
                    // no idea man
                }
            }

            return HeightLevel.None;
        }

        private void shoot()
        {
            // spawn dagger
            int spawnX =  _mask.offsetx + _mask.w;

            int spawnY;
            switch (currHeight)
            {
                case HeightLevel.High:
                    spawnY = 50;
                    break;
                case HeightLevel.Mid:
                    spawnY = 110;
                    break;
                default:
                case HeightLevel.Low:
                    spawnY = 170;
                    break;
            }

            FireBullet bullet = new FireBullet(x + spawnX, spawnY, false /* not flipped */);
            bullet.setOwner(this);
            world.add(bullet, "hazard");
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (bConfig.DEBUG)
            {
                //idleWatchMask.render(sb);
                sniffWatchMask.render(sb);
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
