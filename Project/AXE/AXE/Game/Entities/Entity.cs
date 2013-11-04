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
        public Dir facing;

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

        public virtual void onUpdateBegin()
        {
            previousPosition = pos;
        }

        public virtual void onUpdate()
        {
        }

        public virtual void onUpdateEnd()
        {
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
    }
}
