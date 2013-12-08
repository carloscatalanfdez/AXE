using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using AXE.Game.Control;

namespace AXE.Game.Entities.Contraptions
{
    class Key : Entity
    {
        bSpritemap sprite
        {
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        int type;
        public bool collected;

        public Key(int x, int y, int type)
            : base(x, y)
        {
            this.type = type;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprKeysSheet;
        }

        public override void init()
        {
            base.init();

            sprite = new bSpritemap(Game.res.sprKeysSheet, 8, 8);
            sprite.add(new bAnim("idle", new int[] { type-1 }));
            sprite.play("idle");

            mask.w = 8;
            mask.h = 8;

            collected = false;
        }

        public override void update()
        {
            base.update();

            sprite.update();
        }

        public override void onCollision(string type, bEngine.bEntity other)
        {
            if (type == "player")
            {
                onCollected(other as Player);
            }
        }

        public void onCollected(Entity other)
        {
            if (collected)
                return;

            if (other is Player)
            {
                PlayerData data = (other as Player).data;
                if (data.keys[type] < 9)
                {
                    data.keys[type]++;
                    if (type == 0)
                        Game.res.sfxKeyA.Play();
                    else if (type == 1)
                        Game.res.sfxKeyB.Play();
                    else
                        Game.res.sfxKeyC.Play();
                    collected = true;
                    world.remove(this);
                }
            }
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }
    }
}
