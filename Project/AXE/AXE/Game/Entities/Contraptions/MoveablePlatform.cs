using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bEngine.Graphics;
using bEngine;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities.Contraptions
{
    class MoveablePlatform : Entity
    {
        int width;
        List<Vector2> nodes;
        int stepsBetweenNodes;
        bool cycleNodes;

        int currentNode;
        int nextNode;
        Vector2 delta;
        int direction;

        public MoveablePlatform(int x, int y, 
            int w, List<Vector2> nodes, int steps, bool cycle) 
            : base(x, y)
        {
            width = w;
            this.nodes = nodes;
            if (nodes == null || nodes.Count <= 0)
                nodes = new List<Vector2>(new Vector2[] { new Vector2(x, y) });
            stepsBetweenNodes = steps;
            cycleNodes = cycle;
        }

        public override void init()
        {
            base.init();

            pos = nodes[0];
            currentNode = 0;
            if (nodes.Count > 0)
                nextNode = 1;
            else
                nextNode = 0;

            direction = 1;

            calculateDelta();
            
            mask.w = width;
            mask.h = 8;
            mask.update(x, y);
        }

        public override void onUpdateBegin()
        {
            base.onUpdate();

            Vector2 oldPosition = pos;

            if (nextNode != currentNode)
            {
                if (Math.Abs((pos - nodes[nextNode]).Length()) > delta.Length())
                {
                    pos += delta;
                }
                else
                {
                    pos = nodes[nextNode];
                    nextNode += direction;

                    // Handle node list finish
                    if (direction > 0 && nextNode >= nodes.Count)
                    {
                        if (cycleNodes)
                        {
                            nextNode = 0;
                        }
                        else
                        {
                            nextNode--; // return to last node to start going back
                            currentNode = nextNode;
                            direction = -1;
                            nextNode += direction;
                        }

                        calculateDelta();
                    }
                    else if (direction < 0 && nextNode < 0)
                    {
                        if (cycleNodes)
                        {
                            nextNode = nodes.Count - 1;
                        }
                        else
                        {
                            nextNode++; // return to last node to start going back
                            currentNode = nextNode;
                            direction = 1;
                            nextNode += direction;
                        }

                        calculateDelta();
                    }
                    else
                    {
                        currentNode += direction;
                        calculateDelta();
                    }
                }
            }

            // Notify people up there!
            List<bEntity> cargo = instancesPlace(x, y - 1, "player", null, platformUserCondition);
            foreach (bEntity entity in cargo)
            {
                if (entity != null && entity is IPlatformUser)
                    (entity as IPlatformUser).onPlatformMovedWithDelta(pos - oldPosition, this);
            }
        }

        bool platformUserCondition(bEntity self, bEntity other)
        {
            return (other is IPlatformUser);
        }

        void calculateDelta()
        {
            Vector2 current = nodes[currentNode];
            Vector2 next = nodes[nextNode];

            if (current != next)
            {
                delta = (next - current) / stepsBetweenNodes;
                delta.X = (float)Math.Round(delta.X, 2);
                delta.Y = (float)Math.Round(delta.Y, 2);
            }
            else
                delta = Vector2.Zero;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            sb.Draw(bDummyRect.sharedDummyRect(game), mask.rect, Color.Plum);
        }
    }
}
