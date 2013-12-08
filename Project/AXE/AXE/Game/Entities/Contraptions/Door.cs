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
        protected bSpritemap sprite 
        { 
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        protected DoorLock.Type lockType;
        protected DoorLock lockedBy;
        protected bool isOpen;

        public Door(int x, int y, DoorLock.Type lockType)
            : base(x+4, y)
        {
            this.lockType = lockType;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprDoorSheet;
        }

        public override void init()
        {
            base.init();

            layer = 5;

            sprite = new bSpritemap(Game.res.sprDoorSheet, 40, 40);
            sprite.add(new bAnim("closed", new int[] { 1 }));
            sprite.add(new bAnim("open-left", new int[] { 0 }));
            sprite.add(new bAnim("open-right", new int[] { 2 }));
            sprite.play("closed");
            isOpen = false;

            sprite.offsetx = -16;

            mask.w = 8;
            mask.h = 40;

            lockedBy = null;
        }

        public override void update()
        {
            base.update();

            if (!isOpen)
            {
                if (lockedBy == null || (lockedBy != null && !lockedBy.isLocked))
                {
                    if (placeMeeting(x - 1, y, "player"))
                        open(Dir.Right);
                    else if (placeMeeting(x + 1, y, "player"))
                        open(Dir.Left);
                }
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

        protected void open(Dir towards)
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

    class KeyDoor : Door
    {
        public int key;

        public KeyDoor(int x, int y, int key)
            : base(x, y, DoorLock.Type.Key)
        { 
            this.key = key;
        }

        public override void init()
        {
            base.init();
            Color[] colors = new Color[] { Color.FloralWhite, Color.LightGoldenrodYellow, Color.IndianRed, Color.DodgerBlue };
            lockedBy = new DoorLock(x, y, lockType);
            lockedBy.color = colors[key];
            world.add(lockedBy, "contraptions");
        }

        public override void update()
        {
            base.update();

            if (lockedBy.isLocked)
            {
                // This allows doors to be opened with the correct key
                mask.w += 3;
                Player player = instancePlace(x - 1, y, "player") as Player;
                if (player != null)
                {
                    if (player.data.keys[key] > 0)
                    {
                        player.data.keys[key]--;
                        lockedBy.unlock();
                    }
                }
                mask.w -= 3;
            }
        }
    }

    class DoorLock : Entity
    {
        public enum Type { None, Key, Contraption };
        bSpritemap sprite
        {
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        public Type type;

        public bool isLocked;

        public DoorLock(int x, int y, Type type)
            : base(x, y)
        {
            this.type = type;
        }

        /* IReloadable implementation */
        override public void reloadContent()
        {
            sprite.image = (game as AxeGame).res.sprLocksSheet;
        }

        public override void init()
        {
            base.init();

            layer = 1;

            if (type == Type.None)
            {
                sprite = null;
                isLocked = false;
            }
            else
            {
                sprite = new bSpritemap(Game.res.sprLocksSheet, 8, 40);
                int[] frames = new int[1];
                switch (type)
                {
                    case Type.Key:
                        frames[0] = 0;
                        break;
                    case Type.Contraption:
                        frames[0] = 1;
                        break;
                }

                sprite.add(new bAnim("locked", frames));
                sprite.play("locked");

                isLocked = true;
            }
        }

        public void unlock()
        {
            isLocked = false;
            visible = false;
            Game.res.sfxUnlock.Play();
        }

        public override void update()
        {
            sprite.color = color;
            
            base.update();

            sprite.update();
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }
    }
}
