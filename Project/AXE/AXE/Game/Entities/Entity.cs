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
        public bool visible = true;

        public virtual int graphicWidth()
        {
            throw new NotImplementedException("Declare this method for this class!");
        }

        public Entity(int x, int y)
            : base(x, y)
        {
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
    }
}
