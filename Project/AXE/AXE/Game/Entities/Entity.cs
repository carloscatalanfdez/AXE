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

        public bool wrappable = true;
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
        public bool draggable = false;
        protected bool beingDragged;
        public Vector2 dragOffset;

        public bMaskList _wrappedMask;
        public override bMask mask
        {
            get 
            {
                if (wrappable)
                {
                    bool wrapMask = generateWrappedMask();
                    if (wrapMask)
                    {
                        return _wrappedMask;
                    }
                }

                return _mask;
            }
            set 
            {
                base.mask = value;
            }
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

            // create the object that will hold the masklist
            bMask maskL = new bMask(0, 0, 0, 0);
            maskL.game = game;
            bMask maskR = new bMask(0, 0, 0, 0);
            maskR.game = game;
            _wrappedMask = new bMaskList(new bMask[] { maskL, maskR }, 0, 0, false);
            _wrappedMask.game = game;
        }

        public virtual void onUpdateBegin()
        {
            previousPosition = pos;
            generateWrappedMask();
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
                {
                    showWrapEffect = Dir.Right;
                }
                else if (x + graphic.width > (world as LevelScreen).width)
                {
                    showWrapEffect = Dir.Left;
                }
                else
                {
                    showWrapEffect = Dir.None;
                }
            }
        }

        public override void update()
        {
            if (!isPaused())
            {
                onUpdateBegin();

                if (bConfig.DEBUG)
                {
                    mouseHover = mask.rect.Contains(bGame.input.mouseX, bGame.input.mouseY);
                    if (mouseHover && bGame.input.pressed(0))
                        onClick();
                }

                onUpdate();
                base.update();

                onUpdateEnd();

                if (draggable && beingDragged)
                {
                    if (input.released(0))
                    {
                        beingDragged = false;
                        dragOffset = Vector2.Zero;
                    }
                    else
                        pos = bGame.input.mousePosition - dragOffset;
                }

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

        protected bool generateWrappedMask()
        {
            _mask.update(x, y);
            if (showWrapEffect == Dir.Left)
            {
                int clippedSize = _mask.x + _mask.w - (world as LevelScreen).width;

                if (clippedSize > 0)
                {
                    int oppositeMaskSize = Math.Min(clippedSize, _mask.h);

                    bMask oppositeMask = _wrappedMask.masks[0];
                    bMask currentClippedMask = _wrappedMask.masks[1];
                    // Mask on the other side will have increasing width as we wrap
                    // Starts at the beginning of the screen (taking into account mask offset)
                    oppositeMask.w = oppositeMaskSize;
                    oppositeMask.h = _mask.h;
                    //old //oppositeMask.offsetx = -Math.Min(_mask.x, (world as LevelScreen).width) + _mask.offsetx;
                    oppositeMask.offsetx = -(world as LevelScreen).width + _mask.offsetx + _mask.w - oppositeMaskSize;
                    oppositeMask.offsety = _mask.offsety;
                    // Mask on current side will have decreasing width as we wrap
                    currentClippedMask.w = _mask.w - clippedSize;
                    currentClippedMask.h = _mask.h;
                    currentClippedMask.offsetx = _mask.offsetx;
                    currentClippedMask.offsety = _mask.offsety;

                    _wrappedMask.update(x, y);

                    return true;
                }
            }
            else if (showWrapEffect == Dir.Right)
            {
                int clippedOffset = -_mask.x;

                if (clippedOffset > 0)
                {
                    int currentMaskOffset = _mask.x >= 0 ? 0 : _mask.offsetx;
                    int oppositeMaskSize = Math.Min(clippedOffset, _mask.w);

                    bMask oppositeMask = _wrappedMask.masks[0];
                    bMask currentClippedMask = _wrappedMask.masks[1];
                    // Mask on the other side will have increasing width as we wrap
                    // Starts at the end of the screen (taking into account mask offset)
                    oppositeMask.w = oppositeMaskSize;
                    oppositeMask.h = _mask.h;
                    oppositeMask.offsetx = _mask.offsetx + (world as LevelScreen).width;
                    oppositeMask.offsety = _mask.offsety;
                    // Mask on current side will have decreasing width as we wrap
                    currentClippedMask.w = _mask.w - clippedOffset;
                    currentClippedMask.h = _mask.h;
                    currentClippedMask.offsetx = clippedOffset + _mask.offsetx;//old//currentMaskOffset + clippedOffset;
                    currentClippedMask.offsety = _mask.offsety;

                    _wrappedMask.update(x, y);

                    return true;
                }
            }

            return false;
        }

        protected bMask generateWrappedMask(bMask target)
        {
            int maskX = target.x - target.offsetx;
            int maskY = target.y - target.offsety;

            bMask wrappedMask = null;
            if (showWrapEffect == Dir.Left)
            {
                int clippedSize = target.x + target.w - (world as LevelScreen).width;

                if (clippedSize > 0)
                {
                    int oppositeMaskSize = Math.Min(clippedSize, target.h);

                    // Mask on the other side will have increasing width as we wrap
                    // Starts at the beginning of the screen (taking into account mask offset)
                    bMask oppositeMask = new bMask(
                        maskX, 
                        maskY, 
                        oppositeMaskSize,
                        target.h,
                        -(world as LevelScreen).width + target.offsetx + target.w - oppositeMaskSize,
                        target.offsety);

                    // Mask on current side will have decreasing width as we wrap
                    bMask currentMask = new bMask(
                        maskX,
                        maskY,
                        target.w - clippedSize,
                        target.h,
                        target.offsetx,
                        target.offsety);

                    wrappedMask = new bMaskList(new bMask[] { oppositeMask, currentMask }, maskX, maskY, false);
                }
            }
            else if (showWrapEffect == Dir.Right)
            {
                int clippedOffset = -target.x;

                if (clippedOffset > 0)
                {
                    int currentMaskOffset = target.x >= 0 ? 0 : target.offsetx;
                    int oppositeMaskSize = Math.Min(clippedOffset, target.w);

                    // Mask on the other side will have increasing width as we wrap
                    // Starts at the end of the screen (taking into account mask offset)
                    bMask oppositeMask = new bMask(
                        maskX,
                        maskY,
                        oppositeMaskSize,
                        target.h,
                        target.offsetx + (world as LevelScreen).width,
                        target.offsety);

                    // Mask on current side will have decreasing width as we wrap
                    bMask currentMask = new bMask(
                        maskX,
                        maskY,
                        target.w - clippedOffset,
                        target.h,
                        clippedOffset + target.offsetx,
                        target.offsety);

                    wrappedMask = new bMaskList(new bMask[] { oppositeMask, currentMask }, maskX, maskY, false);
                }
            }

            if (wrappedMask == null)
                return target;
            else
                return wrappedMask;
        }

        public virtual void onHit(Entity other)
        {
        }

        public virtual void onClick()
        {
            // This entity was clicked by the left mouse button
            if (draggable)
            {
                dragOffset = input.mousePosition - pos;
                beingDragged = true;
            }
        }

        public virtual void onActivationEndNotification()
        {
            // The activation you requested has completed. Finish your animation or something.
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            if (graphic != null)
            {
                if (wrappable)
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
            }

            if (mouseHover)
                sb.Draw(bDummyRect.sharedDummyRect(game), mask.rect, Color.Snow);
        }
    }
}
