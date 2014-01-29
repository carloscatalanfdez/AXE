using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE.Game;
using AXE.Common;
using AXE.Game.Screens;
using AXE.Game.Utils;
using bEngine.Helpers;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities.Enemies
{
    class FlameSpirit : Base.Enemy, IHazard, IHazardProvider
    {
        const int DECISION_TIMER = 0;
        const int COOLDOWN_TIMER = 1;

        const int BULLETS_PER_ATTACK = 1;

        public enum State { Invisible, In, Float, Attack, Out, Death };
        public State state;

        bSpritemap smgraphic;
        bSpritemap fgraphic;

        // Parameters
        Range waitToEnterTime;
        Range waitToStartMovingTime;
        Range floatAroundTime;
        int speed;
        Range attackCooldownTime;
        Range attackPaceTime;

        // Gameplay vars
        bool moving;
        int hspeed, vspeed;
        Vector2 moveTo;
        int attacks;
        Entity target;
        bool willAttack;

        int oscillation;

        // Debug
        // string label;
        bMask targettingMask;

        public FlameSpirit(int x, int y)
            : base(x, y)
        {
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            smgraphic.image = (game as AxeGame).res.sprFlamewrathSheet;
            fgraphic.image = (game as AxeGame).res.sprFlameSheet;
        }

        public override void init()
        {
            base.init();

            loadParameters();
            
            // Head
            smgraphic = new bSpritemap((game as AxeGame).res.sprFlamewrathSheet, 32, 32);
            smgraphic.add(new bAnim("invisible", new int[] { 17 }));
            smgraphic.add(new bAnim("in", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 
                9, 10, 11, 12, 13, 14, 15, 16}, 0.5f, false));
            smgraphic.add(new bAnim("float", new int[] { 20, 21, 22, 23 }, 0.3f));
            smgraphic.add(new bAnim("attack", new int[] { 30 }));
            smgraphic.add(new bAnim("out", new int[] { 16, 15, 14, 13, 12, 11, 10, 9,
                8, 7, 6, 5, 4, 3, 2, 1, 0}, 0.5f, false));

            // Flame
            fgraphic = new bSpritemap((game as AxeGame).res.sprFlameSheet, 32, 32);
            fgraphic.add(new bAnim("idle", new int[] { 0, 1, 2, 3 }, 0.3f));
            fgraphic.add(new bAnim("gone", new int[] { 4 }));

            smgraphic.color *= 0.8f;
            fgraphic.color *= 0.8f;

            initParameters();

            setTimer(DECISION_TIMER, waitToEnterTime);
        }

        protected void loadParameters()
        {
            waitToEnterTime = new Range(30, 320);
            waitToStartMovingTime = new Range(15, 30);
            floatAroundTime = new Range(180, 2048);
            attackPaceTime = new Range(15, 16);
            attackCooldownTime = new Range(60, 180);

            mask.w = 8;
            mask.h = 8;
            mask.offsetx = 8;
            mask.offsety = 8;

            speed = 3;
            oscillation = 0;
        }

        protected void initParameters()
        {
            state = State.Invisible;
            smgraphic.play("invisible");
            fgraphic.play("gone");
            hspeed = 0;
            vspeed = 0;
            moving = false;
            attacks = 0;
            willAttack = false;
            targettingMask = new bMask(0, 0, 0, 0);
            target = null;
        }

        public override void onTimer(int n)
        {
            switch (n)
            {
                case DECISION_TIMER:
                    switch (state)
                    {
                        case State.Invisible:
                            // Play fade in
                            // Choose facing
                            if (x < (world as LevelScreen).width / 2)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;

                            state = State.In;
                            smgraphic.play("in");
                            willAttack = true;
                            break;
                        case State.Float:
                            if (!moving)
                            {
                                // Start moving
                                moving = true;
                                // Choose if upwards or downwards
                                if (random.Next(10) < 5)
                                    vspeed = -speed;
                                else
                                    vspeed = speed;

                                setTimer(DECISION_TIMER, floatAroundTime);
                            }
                            else
                            {
                                // Disappear
                                moving = false;
                                hspeed = 0;
                                vspeed = 0;
                                state = State.Out;
                                smgraphic.play("out");
                                fgraphic.play("gone");
                            }
                        break;
                        case State.Attack:
                        if (attacks++ < BULLETS_PER_ATTACK)
                            {
                                // Spawn bullet
                                FlameSpiritBullet bullet = 
                                    new FlameSpiritBullet(x+16, y+16, smgraphic.flipped);
                                bullet.setOwner(this);
                                world.add(bullet, "hazard");
                                setTimer(DECISION_TIMER, attackPaceTime);
                            }
                            else
                            {
                                target = null;
                                willAttack = false;
                                state = State.Float;
                                setTimer(DECISION_TIMER, waitToStartMovingTime);
                                setTimer(COOLDOWN_TIMER, attackCooldownTime);
                            }
                        break;
                    }
                break;
                case COOLDOWN_TIMER:
                    willAttack = true;
                    target = null;
                break;
            }
        }

        public override void update()
        {
            base.update();

            moveTo = pos;

            switch (state)
            {
                case State.Invisible:
                    smgraphic.play("invisible");
                    fgraphic.play("gone");
                    break;
                case State.In:
                    if (smgraphic.currentAnim.finished)
                    {
                        // Anim finished, 
                        state = State.Float;
                        setTimer(DECISION_TIMER, waitToStartMovingTime);
                    }

                    break;
                case State.Float:
                    // Show yourself!
                    smgraphic.play("float");
                    fgraphic.play("idle");

                    // Oscillation
                    float yOscillation = (float) (Math.Sin(0.1 * oscillation));
                    oscillation++;
                    moveTo.Y += vspeed + yOscillation;

                    if (moving)
                    {
                        if (willAttack)
                        {
                            // Check for player & attack routines
                            Entity currentTarget = watchForTargets();
                            target = currentTarget;
                            if (target != null)
                            {
                                // Kill!
                                if (random.NextDouble() < 0.5)
                                {
                                    willAttack = false;
                                    setTimer(COOLDOWN_TIMER, attackCooldownTime);
                                }
                                else
                                {                                    
                                    state = State.Attack;
                                    smgraphic.play("attack");
                                    attacks = 0;
                                    setTimer(DECISION_TIMER, attackPaceTime);
                                }
                            }
                        }
                        
                        {
                            // Magic-like trail effect sad intent. Total failure.
                            // world.add(new FlameSpiritTrace(x, y, smgraphic.currentAnim.frame, smgraphic.flipped), "items");
                            if (facing == Dir.Left)
                                hspeed = -speed;
                            else
                                hspeed = speed;

                            moveTo.X += hspeed;
                            moveTo.Y += ((float)Math.Sin(MoreMath.DegToRad(timer[DECISION_TIMER])));

                            // Check for bounces!
                            if (moveTo.X < 0 && facing == Dir.Left)
                            {
                                moveTo.X = 0;
                                facing = Dir.Right;
                            }
                            else if ((moveTo.X + graphicWidth() > (world as LevelScreen).width) &&
                                facing == Dir.Right)
                            {
                                moveTo.X = (world as LevelScreen).width - graphicWidth();
                                facing = Dir.Left;
                            }

                            if (moveTo.Y < 0 && vspeed < 0)
                            {
                                moveTo.Y = 0;
                                vspeed = speed;
                            }
                            else if ((moveTo.Y + graphicHeight() > (world as LevelScreen).height) &&
                                vspeed > 0)
                            {
                                moveTo.Y = (world as LevelScreen).height - graphicHeight();
                                vspeed = -speed;
                            }
                        }
                    }

                    break;
                case State.Attack:
                    smgraphic.play("attack");
                    fgraphic.play("idle");
                    break;
                case State.Out:
                    if (smgraphic.currentAnim.finished)
                    {
                        // Should also move around or something
                        state = State.Invisible;
                        setTimer(DECISION_TIMER, waitToEnterTime);
                    }
                    break;
                case State.Death:
                    if (smgraphic.currentAnim.finished)
                    {
                        world.remove(this);
                    }
                    break;
            }

            pos = moveTo;

            smgraphic.flipped = (facing == Dir.Left);
            smgraphic.update();
            fgraphic.flipped = (facing == Dir.Left);
            fgraphic.update();
        }

        protected Entity watchForTargets()
        {
            if (AxeGame.input.check(Microsoft.Xna.Framework.Input.Keys.F))
                return (world as LevelScreen).playerA;

            Entity foundYou = null;

            // Will try to find a suitable target in the horizontal line in
            // which it is actually looking. Will we succeed?? We'll see...
            targettingMask = new bMask(x, y-graphicHeight()/2, 
                (int) ((4 / 6f) * (world as LevelScreen).width), graphicHeight() * 2);
            if (facing == Dir.Left)
            {
                targettingMask.x = x - targettingMask.w;
            }
            else
            {
                targettingMask.x = x + graphicWidth();
            }

            if (vspeed < 0)
                targettingMask.y += targettingMask.h / 2;
            else if (vspeed > 0)
                targettingMask.y -= targettingMask.h / 2;

            foundYou = (Entity) instancePlace(targettingMask, "player", null, alivePlayerCondition);

            return foundYou;
        }

        public override bool onHit(Entity other)
        {
            if (state != State.Death)
            {
                if (other is Axes.NormalAxe)
                {
                    state = State.Death;
                    smgraphic.play("out");
                    fgraphic.play("gone");
                }
                return true;
            }
            return false;
        }
        
        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            // label = state.ToString() + " [" + timer[DECISION_TIMER] + "]";
            // label = "" + (4 * ((float)Math.Sin(MoreMath.DegToRad(timer[DECISION_TIMER]))));
            // label = (willAttack ? "YES" : "NO");
            // sb.Draw(bDummyRect.sharedDummyRect(game), targettingMask.rect, new Color(199, 99, 10, 50));
            // smgraphic.color = Tools.RandomColor;
            fgraphic.render(sb, pos);
            smgraphic.render(sb, pos);
            // sb.DrawString(game.gameFont, label, new Vector2(x, y + graphicHeight()), Color.White);
        }

        public override int graphicWidth()
        {
            return smgraphic.width;
        }

        public override int graphicHeight()
        {
            return smgraphic.height;
        }


        public override void onCollision(string type, bEntity other)
        {
            if (state == State.Float || state == State.Attack)
            {
                if (type == "player")
                {
                    // disappear maybe?
                }
            }
        }

        public void setOwner(IHazardProvider owner)
        {
            ;
        }

        public IHazardProvider getOwner()
        {
            return this;
        }

        public Player.DeathState getType()
        {
            return Player.DeathState.Fire;
        }

        public virtual void onHit()
        {
        }

        public void onSuccessfulHit(Player other)
        {
            // HA!
        }
    }

    class FlameSpiritBullet : KillerRect
    {
        bSpritemap sprite;

        // Parameters
        int speed;

        // State vars
        bool flipped;
        

        public FlameSpiritBullet(int x, int y, bool flipped)
            : base(x, y, 24, 8, Player.DeathState.Fire)
        {
            this.flipped = flipped;
        }

        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprFlameBulletSheet;
        }

        public override void init()
        {
            base.init();

            mask.w = 15;
            mask.h = 3;
            mask.offsetx = 4;
            mask.offsety = 2;

            sprite = new bSpritemap((game as AxeGame).res.sprFlameBulletSheet, 24, 8);
            sprite.add(new bAnim("1", new int[] { Tools.random.Next(3) }));
            sprite.play("1");
            sprite.flipped = flipped;
            if (flipped)
            {
                x -= sprite.width;
            }

            speed = 5;

            if (y + sprite.height < 0 || y > (world as LevelScreen).height)
                world.remove(this);
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

            sprite.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            sprite.color = Tools.RandomColor;
            sprite.render(sb, pos);
        }
    }

    class FlameSpiritTrace : Entity
    {
        int frame;
        bool flipped;

        public FlameSpiritTrace(int x, int y, int frame, bool flipped) : base(x, y)
        {
            this.frame = frame;
            this.flipped = flipped;
            layer = 2;
        }

        override public void reloadContent()
        {
            (graphic as bSpritemap).image = (game as AxeGame).res.sprFlameBulletSheet;
        }

        public override void init()
        {
            base.init();

            collidable = false;

            graphic = new bSpritemap((game as AxeGame).res.sprFlamewrathSheet, 32, 32);
            (graphic as bSpritemap).add(new bAnim("1", new int[] { frame }));
            (graphic as bSpritemap).play("1");
            (graphic as bSpritemap).flipped = flipped;
            setTimer(0, 5, 6);
            (graphic as bSpritemap).alpha = 0.5f;
        }

        public override void onTimer(int n)
        {
            world.remove(this);
        }

        public override void update()
        {
            base.update();
            (graphic as bSpritemap).update();

            graphic.alpha = timer[0] / 5f;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.color = Tools.RandomColor;
            graphic.alpha = timer[0] / 5f;
            graphic.render(sb, pos);

            graphic.render(sb, pos);
        }
    }
}

