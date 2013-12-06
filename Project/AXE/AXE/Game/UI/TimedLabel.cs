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
        Color color;
        SoundEffect sound;

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
        Texture2D image;
        Color color;
        SoundEffect sound;
        bStamp graphic;

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
}

