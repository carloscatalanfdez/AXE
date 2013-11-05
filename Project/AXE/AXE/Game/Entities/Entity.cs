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

        public override bMask mask
        {
            get 
            {
                if (showWrapEffect == Dir.Left)
                {
                    bMask oppositeMask = new bMask(x - (world as LevelScreen).width, y, _mask.w, _mask.h, _mask.offsetx, _mask.offsety);
                    oppositeMask.game = game;
                    _mask.update(x, y);
                    bMask wrappedMask = new bMaskList(new bMask[] { _mask, oppositeMask }, x, y, false /* not connected */);
                    wrappedMask.game = game;
                    return wrappedMask;
                }
                else if (showWrapEffect == Dir.Right)
                {
                    //graphic.color = Color.Aqua;
                    bMask oppositeMask = new bMask((world as LevelScreen).width + x, y, _mask.w, _mask.h, _mask.offsetx, _mask.offsety);
                    oppositeMask.game = game;
                    _mask.update(x, y);
                    bMask wrappedMask = new bMaskList(new bMask[] { _mask, oppositeMask }, x, y, false /* not connected */);
                    wrappedMask.game = game;
                    return wrappedMask;
                }
                else
                {
                    return _mask;
                }
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
                return (p.previousPosition.Y + p.mask.offsety + me.mask.h <= other.mask.y);
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
        }
    }
}
