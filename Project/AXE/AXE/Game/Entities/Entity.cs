using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using bEngine;
using bEngine.Graphics;

using AXE.Game.Screens;

namespace AXE.Game.Entities
{
    class Entity : bEntity
    {
        public enum Dir { None, Left, Right };

        public bool visible = true;
        public Vector2 previousPosition;
        public Dir showWrapEffect;
        public Dir facing;

        public bGraphic _graphic;
        public virtual bGraphic graphic
        {
            get { return _graphic; }
            set { _graphic = value; }
        }

        public bool mouseHover;

        public override bMask mask
        {
            get 
            {
                if (showWrapEffect == Dir.Left)
                {
                    _mask.update(x, y);
                    int clippedSize =_mask.x + _mask.w - (world as LevelScreen).width;

                    if (clippedSize > 0)
                    {
                        int oppositeMaskSize = Math.Min(clippedSize, _mask.h);
                        // Mask on the other side will have increasing width as we wrap
                        // Starts at the beginning of the screen (taking into account mask offset)
                        bMask oppositeMask = new bMask(x, y, oppositeMaskSize, _mask.h, -Math.Min(_mask.x, (world as LevelScreen).width) + _mask.offsetx, _mask.offsety);
                        oppositeMask.game = game;
                        // Mask on current side will have decreasing width as we wrap
                        bMask currentClippedMask = new bMask(x, y, _mask.w - clippedSize, _mask.h, _mask.offsetx, _mask.offsety);
                        currentClippedMask.game = game;
                        bMask wrappedMask = new bMaskList(new bMask[] { currentClippedMask, oppositeMask }, x, y, false /* not connected */);
                        wrappedMask.game = game;
                        return wrappedMask;
                    }
                }
                else if (showWrapEffect == Dir.Right)
                {
                    _mask.update(x, y);
                    int clippedOffset = -_mask.x;

                    if (clippedOffset > 0)
                    {
                        // Mask on the other side will have increasing width as we wrap
                        // Starts at the end of the screen (taking into account mask offset)
                        int currentMaskOffset = _mask.x >= 0 ? 0 : _mask.offsetx;
                        int oppositeMaskSize = Math.Min(clippedOffset, _mask.w);
                        bMask oppositeMask = new bMask(x, y, oppositeMaskSize, _mask.h, _mask.offsetx + (world as LevelScreen).width, _mask.offsety);
                        oppositeMask.game = game;
                        // Mask on current side will have decreasing width as we wrap
                        bMask currentClippedMask = new bMask(0, y, _mask.w - clippedOffset, _mask.h, currentMaskOffset + clippedOffset, _mask.offsety);
                        currentClippedMask.game = game;
                        bMask wrappedMask = new bMaskList(new bMask[] { currentClippedMask, oppositeMask }, x, y, false /* not connected */);
                        wrappedMask.game = game;
                        return wrappedMask;
                    }
                }

                return _mask;
            }
            set { base.mask = value; }
        }

        public virtual int graphicWidth()
        {
            throw new NotImplementedException("Declare this method for this class!");
        }

        public virtual int graphicHeight()
        {
            throw new NotImplementedException("Declare this method for this class!");
        }

        public Entity(int x, int y)
            : base(x, y)
        {
        }

        override public void init()
        {
            base.init();

            showWrapEffect = Dir.None;
            mouseHover = false;
        }

        public virtual void onUpdateBegin()
        {
            previousPosition = pos;
        }

        public virtual void onUpdate()
        {
        }

        public virtual void onUpdateEnd()
        {
            // Wrap (effect)
            if (graphic != null)
            {
                if (x < 0)
                    showWrapEffect = Dir.Right;
                else if (x + graphic.width > (world as LevelScreen).width)
                    showWrapEffect = Dir.Left;
                else
                    showWrapEffect = Dir.None;
            }
        }

        public override void update()
        {
            if (!isPaused())
            {
                onUpdateBegin();

                mouseHover = mask.rect.Contains(bGame.input.mouseX, bGame.input.mouseY);

                onUpdate();
                base.update();

                onUpdateEnd();
            }
        }

        public bool isPaused()
        {
            return (world as LevelScreen).isPaused();
        }

        override public Vector2 moveToContact(Vector2 to, String category, Func<bEntity, bEntity, bool> condition = null)
        {
            Vector2 remnant = Vector2.Zero;

            to.X = (int)Math.Round(to.X);
            to.Y = (int)Math.Round(to.Y);

            // Move to contact in the X
            int s = Math.Sign(to.X - pos.X);
            bool found = false;
            Vector2 tp = pos;

            bool xWrapApplied = false;

            for (float i = to.X; i != pos.X; i -= s)
            {
                tp.X = i;
                if (tp.X < -(graphicWidth() / 2))
                {
                    tp.X = (world as LevelScreen).width + tp.X;
                    xWrapApplied = true;
                }
                else if (tp.X > (world as LevelScreen).width - (graphicWidth() / 2))
                {
                    tp.X = (tp.X - (world as LevelScreen).width);
                    xWrapApplied = true;
                }
                else
                    xWrapApplied = false;

                if (!placeMeeting(tp, category, condition))
                {
                    found = true;
                    break;
                }
            }

            if (found)
                pos.X = tp.X;

            // Move to contact in the Y
            s = Math.Sign(to.Y - pos.Y);
            found = false;
            tp = pos;
            for (float i = to.Y; i != pos.Y; i -= s)
            {
                tp.Y = i;
                if (!placeMeeting(tp, category, condition))
                {
                    found = true;
                    break;
                }
            }

            if (found)
                pos.Y = tp.Y;

            remnant = to - pos;
            if (xWrapApplied && remnant.X != 0)
                remnant.X = 0;
            return remnant;
        }

        public bool onewaysolidCondition(bEntity me, bEntity other)
        {
            if (me is Entity)
            {
                Entity p = me as Entity;

                if (me.mask is bMaskList)
                {
                    // Get mask at previous position
                    bMaskList masks = me.mask as bMaskList;
                    // Check if any mask is above the solid
                    bool collides = false;
                    foreach (bMask m in masks.masks)
                    {
                        collides = (p.previousPosition.Y + m.offsety + m.h <= other.mask.y);
                        if (collides)
                            break;
                    }
                    return collides;
                }
                else
                {
                    return (p.previousPosition.Y + p.mask.offsety + me.mask.h <= other.mask.y);
                }
            }
            else
                return true;
        }

        public int directionToSign(Dir dir)
        {
            if (dir == Dir.Left)
            {
                return -1;
            }
            else if (dir == Dir.Right)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public virtual void onHit(Entity other)
        {
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            if (graphic != null)
            {
                if (showWrapEffect == Dir.Left)
                {
                    //graphic.color = Color.Aqua;
                    graphic.render(sb, new Vector2(0 + (pos.X - (world as LevelScreen).width), pos.Y));
                }
                else if (showWrapEffect == Dir.Right)
                {
                    //graphic.color = Color.Aqua;
                    graphic.render(sb, new Vector2((world as LevelScreen).width + pos.X, pos.Y));
                }
            }

            if (mouseHover)
                sb.Draw(bDummyRect.sharedDummyRect(game), mask.rect, Color.Snow);
        }
    }
}
