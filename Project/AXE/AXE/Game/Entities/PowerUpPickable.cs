using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace AXE.Game.Entities.Base
{
    class PowerUpPickable : Item
    {
        public static int HIGHFALLGUARD_EFFECT = 0x01;

        public enum Type { HighFallGuard };
        public Type type;
        public int effect;

        public PowerUpPickable(int x, int y, string type)
            : this(x, y, PowerUpPickable.getTypeFromString(type))
        {
        }

        public PowerUpPickable(int x, int y, Type type)
            : base(x, y)
        {
            this.type = type;
        }

        public override void initParams()
        {
            switch (type)
            {
                default:
                case Type.HighFallGuard:
                    spgraphic = new bSpritemap(game.Content.Load<Texture2D>("Assets/Sprites/highfallguard-sheet"), 16, 16);
                    spgraphic.add(new bAnim("idle", new int[] { 0 }));
                    spgraphic.play("idle");

                    mask.w = 10;
                    mask.h = 15;
                    mask.offsetx = 2;
                    mask.offsety = 0;

                    effect = HIGHFALLGUARD_EFFECT;

                    break;
            }

            state = State.Idle;

            layer = 11;
        }

        public override void onCollected()
        {
            state = State.Taken;
            onDisappear();
        }

        public static Type getTypeFromString(string type)
        {
            switch (type)
            {
                default:
                case "HighFallGuard":
                    return Type.HighFallGuard;
            }
        }
    }
}
