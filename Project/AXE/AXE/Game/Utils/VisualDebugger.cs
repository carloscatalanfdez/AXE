using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using AXE.Game.Entities;

namespace AXE.Game.Utils
{
    class VisualDebugger
    {
        bGame game;
        string baseText;
        string text;

        Entity target;

        static VisualDebugger _instance = null;
        public static VisualDebugger get(bGame game)
        {
            if (_instance == null)
            {
                _instance = new VisualDebugger();
                _instance.init(game);
            }
            return _instance;
        }

        VisualDebugger()
        {
            baseText = "VIZDEB\n======\n\n";
        }

        public void init(bGame game)
        {
            this.game = game;
        }

        public void update()
        {
            text = baseText;
            if (target != null)
            {
                Type t = target.GetType();
                text += "===== INSPECTING: " + t.Name + " =====";
                PropertyInfo[] propertyInfos;
                propertyInfos = t.GetProperties();
                // Sort by name
                /*Array.Sort(propertyInfos,
                        delegate(PropertyInfo propertyInfo1, PropertyInfo propertyInfo2)
                        { return propertyInfo1.Name.CompareTo(propertyInfo2.Name); });*/
                foreach (PropertyInfo info in propertyInfos)
                {
                    text += "\n" + info.Name;
                    text += ": " + info.GetValue(target, null);
                }
            }
            else
            {
                text += "(no entity)";
            }
        }

        public void render(SpriteBatch sb)
        {
            drawLabel(sb, text, new Vector2(0, 64));
            drawLabel(sb, "" + Tools.step, 
                new Vector2(0, game.GraphicsDevice.PresentationParameters.BackBufferHeight - 8));
        }

        void drawLabel(SpriteBatch sb, string label, Vector2 pos)
        {
            sb.DrawString(game.gameFont, label, new Vector2(pos.X+1, pos.Y+1), Color.Black);
            sb.DrawString(game.gameFont, label, pos, Color.White);
        }

        internal void setTarget(Entity entity)
        {
            target = entity;
        }
    }
}
