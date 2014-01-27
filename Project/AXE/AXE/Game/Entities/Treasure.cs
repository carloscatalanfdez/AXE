using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Entities.Base;
using bEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bEngine;

namespace AXE.Game.Entities
{
    class Treasure : Item
    {
        public int value;

        public Treasure(int x, int y, int value = 1)
            : base(x, y)
        {
            this.value = value;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            spgraphic.image = (game as AxeGame).res.sprCoinSheet;
        }

        public override int graphicWidth()
        {
            return 9;
        }

        public override int graphicHeight()
        {
            return 9;
        }

        public override void initParams()
        {
            initGraphic();

            mask.w = 7;
            mask.h = 7;
            mask.offsetx = 1;
            mask.offsety = 1;

            state = State.Idle;

            layer = 11;
        }

        public void initGraphic()
        {
            spgraphic = new bSpritemap((game as AxeGame).res.sprCoinSheet, graphicWidth(), graphicHeight());
            spgraphic.add(new bAnim("idle", new int[] { 
                                                        0, 0, 0, 0,
                                                        0, 0, 0, 0,
                                                        0, 0, 0, 0,
                                                        0, 0, 0, 0,
                                                        0, 0, 0, 0, 
                                                        0, 0, 0, 0,
                                                        1, 2, 3 }, 0.8f));
            spgraphic.play("idle");

            if (value >= 0 && value < 10)
            {
                spgraphic.color = Color.Aquamarine;
            }
            else if (value >= 10 && value < 20)
            {
                spgraphic.color = Color.PeachPuff;
            }
            else if (value >= 20 && value < 50)
            {
                spgraphic.color = Color.Chocolate;
            }
            else if (value >= 50 && value < 100)
            {
                spgraphic.color = Color.Gainsboro;
            }
            else if (value >= 100)
            {
                spgraphic.color = Color.ForestGreen;
            }
        }

        public override void onCollected(Player collector)
        {
            collector.data.treausures += value;

            state = State.Taken;
            timer[0] = 10;
        }

        public override void onTimer(int n)
        {
            if (n == 0 && state == State.Taken)
            {
                onDisappear();
            }
        }

        public override void onUpdate()
        {
            base.onUpdate();

            if (state == State.Taken)
            {
                pos.Y -= 5;
                graphic.color *= 0.8f;
            }
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);
            if (bConfig.DEBUG)
                sb.DrawString(game.gameFont, value.ToString(), new Vector2(x, y + 8), Color.White);
        }
    }
}
