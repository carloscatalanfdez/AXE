using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

using AXE;
using AXE.Common;
using AXE.Game.Entities;
using AXE.Game.Entities.Base;
using AXE.Game.Screens;

namespace AXE.Game.Entities.Enemies
{
    class SineFloater : Enemy, IHazard, IHazardProvider
    {
        public bSpritemap sprite
        {
            get { return (graphic as bSpritemap); }
            set { graphic = value; }
        }

        // Parameters
        public float amplitude;
        public float initAngle;
        public float angleDelta;
        public float hspeed;

        // Gamestate vars
        protected int baseY;
        protected float angle;

        public SineFloater(int x, int y, Dir facing, float amplitude, float hspeed, 
            float angleDelta = 10f, float initAngle = 180.0f)
            : base(x, y)
        {
            this.facing = facing;
            this.hspeed = hspeed;
            this.angleDelta = angleDelta;
            this.initAngle = initAngle;
            this.amplitude = amplitude;
            this.baseY = y;
        }

        override public void reloadContent()
        {
            sprite.image = Game.res.sprSkullfloaterSheet;
        }

        public override void init()
        {
            base.init();

            wrappable = false;

            sprite = new bSpritemap(Game.res.sprSkullfloaterSheet, 16, 16);
            sprite.add(new bAnim("idle", new int[] { 0, 1 }, 0.3f));

            // For bigger skull size 24x24, frames 4,5
            /*sprite = new bSpritemap(Game.res.sprSkullfloaterSheet, 16, 16);
            sprite.add(new bAnim("idle", new int[] { 0, 1 }, 0.3f));*/

            sprite.play("idle");

            mask.w = 10;
            mask.h = 10;
            mask.offsetx = 3;
            mask.offsety = 3;

            y = y - graphicHeight() / 2;

            angle = initAngle;

            if (facing == Dir.Left)
                sprite.flipped = false;
            else
                sprite.flipped = true;
        }

        public override bool onHit(Entity other)
        {
            if (other is Axes.SmallAxe || other is Axes.NormalAxe)
            {
                world.remove(this);
            }
            return true;
        }

        public override void onUpdate()
        {
            base.onUpdate();

            angle += angleDelta;
            pos.Y = baseY - graphicHeight()/2 + amplitude * (float) Math.Sin(MathHelper.ToRadians(angle));
            pos.X += directionToSign(facing) * hspeed;

            sprite.update();
        }

        public override void onUpdateEnd()
        {
            base.onUpdateEnd();

            if (facing == Dir.Left)
            {
                if (x + graphicWidth() < 0)
                    world.remove(this);
            }
            else if (facing == Dir.Right)
            {
                if (x > (world as LevelScreen).width)
                    world.remove(this);
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }

        public override int graphicWidth()
        {
            return sprite.width;
        }

        public override int graphicHeight()
        {
            return sprite.height;
        }

        public override void onCollision(string type, bEntity other)
        {
            if (type == "player")
            {
                (other as Player).onCollision("hazard", this);
            }
        }

        /**
         * IHAZARD METHODS
         */
        public void setOwner(IHazardProvider owner)
        {
        }

        public IHazardProvider getOwner()
        {
            return this;
        }

        public Player.DeathState getType()
        {
            return Player.DeathState.Generic;
        }

        public virtual void onHit()
        {
        }

        /**
         * IHAZARDPROVIDER METHODS
         */
        public void onSuccessfulHit(Player other)
        {
        }
    }
}
