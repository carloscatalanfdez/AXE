using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using bEngine;
using bEngine.Graphics;


namespace AXE.Game.UI
{
    class TimedLabel : bEntity
    {
        int fps;

        bool visible;
        int stepsToShow;
        public string label;
        public SoundEffect sound;

        public TimedLabel(int x, int y, string label, int steps,
            SoundEffect sound = null)
            : base(x, y)
        {
            setData(steps, label, sound, Color.White);
        }

        public TimedLabel(int x, int y, string label, int steps, 
            Color color, SoundEffect sound = null) : base(x, y)
        {
            setData(steps, label, sound, color);
        }

        void setData(int steps, string label, SoundEffect sfx, Color color)
        {
            stepsToShow = steps;
            this.label = label;
            this.sound = sfx;
            this.color = color;
        }

        public override void init()
        {
            base.init();

            fps = (game as AxeGame).FramesPerSecond;
            timer[0] = stepsToShow;
            visible = false;
        }

        public override void onTimer(int n)
        {
            if (sound != null)
                sound.Play();

            visible = true;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (visible)
                sb.DrawString(game.gameFont, label, pos, color);
        }

    }

    class TimedStamp : bEntity
    {
        int fps;

        bool visible;
        int stepsToShow;
        public Texture2D image;
        public SoundEffect sound;
        public bStamp graphic;

        public TimedStamp(int x, int y, Texture2D image, int steps,
            SoundEffect sound = null)
            : base(x, y)
        {
            setData(steps, image, sound, Color.White);
        }

        public TimedStamp(int x, int y, Texture2D image, int steps,
            Color color, SoundEffect sound = null)
            : base(x, y)
        {
            setData(steps, image, sound, color);
        }

        void setData(int steps, Texture2D img, SoundEffect sfx, Color color)
        {
            stepsToShow = steps;
            this.image = img;
            this.sound = sfx;
            this.color = color;
        }

        public override void init()
        {
            base.init();

            fps = (game as AxeGame).FramesPerSecond;
            timer[0] = stepsToShow;
            visible = false;

            graphic = new bStamp(image);
            graphic.color = color;
        }

        public override void onTimer(int n)
        {
            if (sound != null)
                sound.Play();

            visible = true;
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (visible)
                graphic.render(sb, pos);
        }
    }

    class IntermittentLabel : bEntity
    {
        public const int FLASH_TIMER = 1;

        public int flashSpeed;
        public string label;
        public SoundEffect sound;

        private bool visible = true;
        private bool _intermittent = false;
        public bool intermittent
        {
            get { return _intermittent; }
            set
            {
                if (value && !_intermittent) // go to intermittent
                {
                    visible = true;
                    timer[FLASH_TIMER] = flashSpeed;
                }
                else if (!value && _intermittent)  // stop intermittent
                {
                    visible = true;
                    timer[FLASH_TIMER] = -1;
                }

                _intermittent = value;
            }
        }

        public IntermittentLabel(int x, int y, string label, Color color, bool intermittent = false, int flashSpeed = 15, SoundEffect sound = null)
            : base(x, y)
        {
            this.flashSpeed = flashSpeed;
            this.label = label;
            this.sound = sound;
            this.color = color;

            _intermittent = intermittent;
        }

        public override void init()
        {
            base.init();

            if (intermittent)
                timer[FLASH_TIMER] = flashSpeed;
        }

        override public void onTimer(int n)
        {
            base.onTimer(n);

            switch (n)
            {
                case FLASH_TIMER:
                    visible = !visible;
                    timer[n] = flashSpeed;

                    if (visible && sound != null)
                        sound.Play();

                    break;
            }
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            if (visible)
                sb.DrawString(game.gameFont, label, pos, color);
        }
    }
}

