using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bEngine.Graphics;
using AXE.Game.Utils;

namespace AXE.Game.Entities.Decoration
{
    class Decoration : Entity
    {
        public bSpritemap sprite 
        {
            get { return graphic as bSpritemap; }
            set { graphic = value; }
        }

        public Decoration(int x, int y)
            : base(x, y)
        {

        }

        public override void init()
        {
            base.init();

            initSprite();

            layer = 19;
        }

        protected virtual void initSprite()
        {
            sprite = new bSpritemap(bDummyRect.sharedDummyRect(game), 8, 8);
            sprite.add(new bAnim("idle", new int[] { 0 }));
            sprite.play("idle");
        }

        public override void update()
        {
            base.update();

            sprite.update();
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);

            sprite.render(sb, pos);
        }
    }

    class FireBasedDecoration : Decoration
    {
        protected Range ANIM_HOLD_RANGE;
        public FireBasedDecoration(int x, int y)
            : base(x, y)
        {
            ANIM_HOLD_RANGE = new Range(10, 120);
            setTimer(0, ANIM_HOLD_RANGE);
        }

        public override void onTimer(int n)
        {
            if (n == 0)
            {
                sprite.currentAnim.speed = getFireSpeed();
                setTimer(0, ANIM_HOLD_RANGE);
            }
        }

        protected float getFireSpeed()
        {
            return (float) Math.Max(0.2f, Math.Min((float)Utils.Tools.random.NextDouble(), 0.8f));
        }

    }

    class DecoTorch : FireBasedDecoration
    {
        public DecoTorch(int x, int y)
            : base(x, y)
        {
        }

        protected override void initSprite()
        {
            sprite = new bSpritemap(Game.res.sprTorchSheet, 8, 32);
            sprite.add(new bAnim("idle", new int[] { 0, 1, 2, 3 }, getFireSpeed()));
            sprite.play("idle");
        }
    }

    class DecoCandle : FireBasedDecoration
    { 
        public DecoCandle(int x, int y)
            : base(x, y)
        {
        }

        protected override void initSprite()
        {
            sprite = new bSpritemap(Game.res.sprCandleSheet, 16, 16);
            sprite.add(new bAnim("idle", new int[] { 0, 1, 2, 3 }, (float)Utils.Tools.random.NextDouble()));
            sprite.play("idle");
        }
    }

    class DecoCandlestick : FireBasedDecoration
    {
        public DecoCandlestick(int x, int y)
            : base(x, y)
        {
        }

        protected override void initSprite()
        {
            sprite = new bSpritemap(Game.res.sprCandlestickSheet, 16, 40);
            sprite.add(new bAnim("idle", new int[] { 0, 1, 2, 3 }, (float)Utils.Tools.random.NextDouble()));
            sprite.play("idle");
        }
    }
}
