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

namespace AXE.Game.Entities.Enemies
{
    class Imp : Enemy
    {
        public enum State { None, Idle, Turn, Walk, Chase }
        public State state;

        bSpritemap graphic;

        Vector2 moveTo;

        int hspeed;
        int idleBaseTime, idleOptionalTime;
        int walkBaseTime, walkOptionalTime;
        int turnBaseTime, turnOptionalTime;

        Dir showWrapEffect;

        List<SoundEffect> sfxSteps;

        public Imp(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            graphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/imp-sheet"), 30, 32);
            graphic.add(new bAnim("idle", new int[] { 0 }));
            graphic.add(new bAnim("turn", new int[] { 9 }));
            graphic.add(new bAnim("walk", new int[] { 1, 2, 3, 2 }, 0.3f));
            graphic.add(new bAnim("jump", new int[] { 8 }));
            graphic.play("idle");

            mask.w = 16;
            mask.h = 21;
            mask.offsetx = 7;
            mask.offsety = 11;

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
                        break;
                }

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
                            break;
                    }
                break;
            }
        }

        public override void update()
        {
            base.update();

            graphic.update();

            moveTo = pos;
            bool onAir = !checkForGround(x, y);

            switch (state)
            {
                case State.Idle:
                    graphic.play("idle");
                    break;
                case State.Walk:
                    graphic.play("walk");

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
                    graphic.play("turn");
                    break;
                case State.Chase:
                    break;
            }

            if (state == State.Walk)
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
                else if (x + (graphic.width) > (world as LevelScreen).width)
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

            graphic.flipped = (facing == Dir.Left);

            handleSoundEffects();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos);
            if (showWrapEffect == Dir.Left)
            {
                graphic.render(sb, new Vector2(0 + (pos.X - (world as LevelScreen).width), pos.Y));
            }
            else if (showWrapEffect == Dir.Right)
            {
                graphic.render(sb, new Vector2((world as LevelScreen).width + pos.X, pos.Y));
            }

            if (bConfig.DEBUG)
                sb.DrawString(game.gameFont, state.ToString() + " [" + timer[0] + "]", new Vector2(x, y - 8), Color.White);
        }

        public override int graphicWidth()
        {
            return graphic.width;
        }

        public override int graphicHeight()
        {
            return graphic.height;
        }

        bool playedStepEffect = false;
        public void handleSoundEffects()
        {
            float relativeX = pos.X / (world as LevelScreen).width - 0.5f;
            switch (state)
            {
                case State.Walk:
                    int currentFrame = graphic.currentAnim.frame;
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
    }
}
