using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using Microsoft.Xna.Framework.Graphics;
using bEngine;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Contraptions
{
    class Door : Entity
    {
        bSpritemap sprite 
        { 
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        bool isOpen;

        public Door(int x, int y)
            : base(x+4, y)
        {
        }

        public override void init()
        {
            base.init();

            sprite = new bSpritemap(Game.res.sprDoorSheet, 40, 40);
            sprite.add(new bAnim("closed", new int[] { 1 }));
            sprite.add(new bAnim("open-left", new int[] { 0 }));
            sprite.add(new bAnim("open-right", new int[] { 2 }));
            sprite.play("closed");
            isOpen = false;

            sprite.offsetx = -16;

            mask.w = 8;
            mask.h = 40;

            // attributes.Add(ATTR_SOLID);
        }

        public override void update()
        {
            base.update();

            if (!isOpen)
            {
                if (placeMeeting(x - 1, y, "player"))
                    open(Dir.Right);
                else if (placeMeeting(x + 1, y, "player"))
                    open(Dir.Left);
            }
            else
            {
                collidable = true;
                mask.w += 3;
                if (!placeMeeting(x-1, y, "player"))
                {
                    isOpen = false;
                    sprite.play("closed");
                    Game.res.sfxCloseDoor.Play();
                }
                else
                    collidable = false;
                mask.w -= 3;
            }

            sprite.update();
        }

        void open(Dir towards)
        {
            Game.res.sfxOpenDoor.Play();
            isOpen = true;
            sprite.play("open-" + towards.ToString().ToLower());
            collidable = false;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }
    }
}
