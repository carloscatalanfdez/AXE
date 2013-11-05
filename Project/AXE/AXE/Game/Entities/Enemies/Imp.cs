using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE.Game;
using AXE.Game.Screens;
using AXE.Game.Entities.Base;
using AXE.Game.Utils;
using AXE.Game.Entities.Axes;

namespace AXE.Game.Entities.Enemies
{
    class Imp : Enemy
    {
        public enum State { None, Idle, Turn, Walk, Chase, ChaseRunning, Attacking, Attacked }
        public State state;

        public bSpritemap spgraphic
        {
            get { return (_graphic as bSpritemap); }
            set { _graphic = value; }
        }

        Vector2 moveTo;
        bMask watchMask;

        bool beginChase;
        int chaseReactionTime;

        int attackThreshold;
        int attackChargeTime;
        int attackTime;
        KillerRect weaponHitZone;
        bStamp weaponHitImage;

        int hspeed;
        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int turnBaseTime, turnOptionalTime;

        List<SoundEffect> sfxSteps;

        public Imp(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/imp-sheet"), 30, 32);
            spgraphic.add(new bAnim("idle", new int[] { 0 }));
            spgraphic.add(new bAnim("turn", new int[] { 9 }));
            spgraphic.add(new bAnim("walk", new int[] { 1, 2, 3, 2 }, 0.3f));
            spgraphic.add(new bAnim("chase-reacting", new int[] { 4 }));
            spgraphic.add(new bAnim("chase", new int[] { 1, 2, 3, 2 }, 0.5f));
            spgraphic.add(new bAnim("chase-running-reacting", new int[] { 10 }));
            spgraphic.add(new bAnim("chase-running", new int[] { 11, 12 }, 0.5f));
            spgraphic.add(new bAnim("attack-charge", new int[] { 16, 17, 17, 17 }, 0.4f, false));
            spgraphic.add(new bAnim("attacked", new int[] { 18 }));
            spgraphic.add(new bAnim("jump", new int[] { 8 }));
            spgraphic.play("idle");

            mask.w = 16;
            mask.h = 21;
            mask.offsetx = 7;
            mask.offsety = 11;

            watchMask = new bMask(x, y, 90, 24);

            hspeed = 1;

            idleBaseTime = 80;
            idleOptionalTime = 80;
            walkBaseTime = 30;
            walkOptionalTime = 30;
            turnBaseTime = 60;
            turnOptionalTime = 60;

            if (Tools.random.Next(2) < 1)
                facing = Dir.Right;
            else
                facing = Dir.Left;

            beginChase = false;
            chaseReactionTime = 15;

            attackThreshold = 30;
            attackChargeTime = 10;
            attackTime = 8;
            weaponHitImage = new bStamp(spgraphic.image, new Rectangle(90, 64, 30, 32));

            sfxSteps = new List<SoundEffect>();
            sfxSteps.Add(game.Content.Load<SoundEffect>("Assets/Sfx/sfx-dirtstep.1"));
            sfxSteps.Add(game.Content.Load<SoundEffect>("Assets/Sfx/sfx-dirtstep.2"));
            sfxSteps.Add(game.Content.Load<SoundEffect>("Assets/Sfx/sfx-dirtstep.3"));

            state = State.None;
            changeState(State.Idle);
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
                    case State.Turn:
                        timer[0] = turnBaseTime + Tools.random.Next(turnOptionalTime) - turnOptionalTime;
                        break;
                    case State.Chase:
                        beginChase = false;
                        timer[1] = chaseReactionTime;
                        break;
                    case State.ChaseRunning:
                        beginChase = false;
                        timer[1] = (int) (chaseReactionTime * 1.5f);
                        break;
                    case State.Attacking:
                        timer[0] = attackChargeTime;
                        break;
                    case State.Attacked:
                        int xx, yy = 4;
                        if (facing == Dir.Right)
                            xx = 20;
                        else
                            xx = -10;
                        weaponHitZone = new KillerRect(x+xx, y+yy, 20, 27);
                        world.add(weaponHitZone, "enemy");
                        timer[0] = attackTime;
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
                case 0:
                    switch (state)
                    {
                        case State.Idle:
                            if (Tools.random.Next(2) < 1)
                                changeState(State.Turn);
                            else
                                changeState(State.Walk);
                            break;
                        case State.Walk:
                            if (Tools.random.Next(2) < 1)
                                changeState(State.Turn);
                            else
                                changeState(State.Idle);
                            break;
                        case State.Turn:
                            if (facing == Dir.Left)
                                facing = Dir.Right;
                            else
                                facing = Dir.Left;

                            changeState(State.Walk);
                            break;
                        case State.Chase:
                        case State.ChaseRunning:
                            break;
                        case State.Attacking:
                            changeState(State.Attacked);
                            // Sound!
                            break;
                        case State.Attacked:
                            changeState(State.Idle);
                            if (weaponHitZone != null)
                            {
                                world.remove(weaponHitZone);
                                weaponHitZone = null;
                            }

                            break;
                    }
                break;
                case 1:
                    if (state == State.Chase || state == State.ChaseRunning)
                        beginChase = true;
                break;
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            spgraphic.update();

            moveTo = pos;
            bool onAir = !checkForGround(x, y);

            switch (state)
            {
                case State.Idle:
                    spgraphic.play("idle");
                    break;
                case State.Walk:
                    spgraphic.play("walk");

                    Vector2 nextPosition = new Vector2(x + directionToSign(facing) * hspeed, y);
                    bool wontFall = checkForGround(
                            (int) (nextPosition.X + directionToSign(facing) * graphicWidth()/2), 
                            (int) nextPosition.Y);
                    bool wontCollide = !placeMeeting(
                            (int) nextPosition.X, 
                            (int) nextPosition.Y, new String[] {"player", "solid"});
                    if (wontFall && wontCollide)
                        moveTo.X += directionToSign(facing) * hspeed;
                    else if (!wontFall)
                        changeState(State.Idle);
                    else if (!wontCollide)
                        changeState(State.Turn);

                    break;
                case State.Turn:
                    spgraphic.play("turn");
                    break;
                case State.Chase:
                case State.ChaseRunning:
                    if (beginChase)
                    {
                        if (state == State.Chase)
                            spgraphic.play("chase");
                        else
                            spgraphic.play("chase-running");

                        int hsp = (int)(hspeed * 2 * (state == State.ChaseRunning ? 1.5 : 1));
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
                        if (state == State.Chase)
                            spgraphic.play("chase-reacting");
                        else
                            spgraphic.play("chase-running-reacting");
                    }
                    break;
                case State.Attacking:
                    spgraphic.play("attack-charge");
                    break;
                case State.Attacked:
                    spgraphic.play("attacked");
                    break;
            }

            if (state == State.Idle || state == State.Walk || state == State.Turn)
            {
                Dir facingDir = facing;
                if (state == State.Turn)
                    if (facingDir == Dir.Left) facingDir = Dir.Right;
                    else facingDir = Dir.Left;
                if (facingDir == Dir.Left)
                    watchMask.offsetx = -watchMask.w;
                else
                    watchMask.offsetx = graphicWidth();
                watchMask.offsety = (graphicHeight() - watchMask.h);

                bMask holdMyMaskPlease = mask;
                mask = watchMask;

                bool sawYou = placeMeeting(x, y, "player");
                mask = holdMyMaskPlease; // thank you!

                if (sawYou)
                {
                    facing = facingDir;
                    changeState(State.Chase);
                }
            }
            else if (state == State.Chase)
            {
                Player[] players = (world as LevelScreen).players;
                foreach (Player player in players)
                {
                    if (player != null && (player.pos - pos).Length() < attackThreshold)
                    {
                        changeState(State.Attacking);
                    }
                }

            }

            if (state == State.Walk || state == State.Chase || state == State.ChaseRunning)
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

                // Wrap (effect)
                if (x < 0)
                    showWrapEffect = Dir.Right;
                else if (x + (spgraphic.width) > (world as LevelScreen).width)
                    showWrapEffect = Dir.Left;
                else
                    showWrapEffect = Dir.None;

                /*// The y movement was stopped
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
                }*/
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

            spgraphic.render(sb, pos);
            if (state == State.Attacked)
                if (facing == Dir.Left)
                {
                    weaponHitImage.flipped = true;
                    weaponHitImage.render(sb, new Vector2(x - weaponHitImage.width, y));
                }
                else
                {
                    weaponHitImage.flipped = false;
                    weaponHitImage.render(sb, new Vector2(x + graphicWidth(), y));
                }

            if (showWrapEffect == Dir.Left)
            {
                spgraphic.render(sb, new Vector2(0 + (pos.X - (world as LevelScreen).width), pos.Y));
            }
            else if (showWrapEffect == Dir.Right)
            {
                spgraphic.render(sb, new Vector2((world as LevelScreen).width + pos.X, pos.Y));
            }

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

        bool playedStepEffect = false;
        public void handleSoundEffects()
        {
            float relativeX = pos.X / (world as LevelScreen).width - 0.5f;
            switch (state)
            {
                case State.Walk:
                case State.Chase:
                case State.ChaseRunning:
                    int currentFrame = spgraphic.currentAnim.frame;
                    if (currentFrame == 2 && !playedStepEffect)
                    {
                        playedStepEffect = true;
                        sfxSteps[Utils.Tools.random.Next(sfxSteps.Count)].Play(0.5f, 0.0f, relativeX);
                    }
                    else if (currentFrame != 2)
                        playedStepEffect = false;

                    break;
                default:
                    break;
            }
        }

        public override void onHit(Entity other)
        {
            base.onHit(other);

            if (other is NormalAxe); // Kill me here or whatuverr
            else if (other is Axe)
            {
                // Get MaD!
                if (other.x + other.graphicWidth() / 2 < x + graphicWidth()/2)
                    facing = Dir.Left;
                else facing = Dir.Right;
                changeState(State.ChaseRunning);
            }
        }
    }
}
