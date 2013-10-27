using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;

using bEngine;

namespace AXE.Common
{
    enum Pad { left, right, up, down, a, b, start, debug1, debug2 };

    class GameInput
    {
        static GameInput _instance;
        public static GameInput getInstance()
        {
            if (_instance == null)
                _instance = new GameInput();
            return _instance;
        }

        public bInput input = bGame.input;

        public float joyHThreshold = 0.3f;
        public float joyVThreshold = 0.3f;

        public Dictionary<Pad, bool> currentStickState, previousStickState;

        public GameInput()
        {
            currentStickState = new Dictionary<Pad, bool>();
            previousStickState = new Dictionary<Pad, bool>();
        }

        public void update()
        {
            foreach (Pad key in currentStickState.Keys)
                previousStickState[key] = currentStickState[key];

            currentStickState[Pad.left] = check(Pad.left);
            currentStickState[Pad.right] = check(Pad.right);
            currentStickState[Pad.up] = check(Pad.up);
            currentStickState[Pad.down] = check(Pad.down);
        }

        public bool check(Pad btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.check((Keys)i);
                else
                    result = input.check((Buttons)i);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                switch (btn)
                {
                    case Pad.left:
                        result = input.left();
                        break;
                    case Pad.right:
                        result = input.right();
                        break;
                    case Pad.up:
                        result = input.up();
                        break;
                    case Pad.down:
                        result = input.down();
                        break;
                }
            }

            return result;
        }

        public bool pressed(Pad btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.pressed((Keys)i);
                else
                    result = input.pressed((Buttons)i);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                return currentStickState[btn] && !previousStickState[btn];
            }

            return result;
        }

        public bool released(Pad btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.released((Keys)i);
                else
                    result = input.released((Buttons)i);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                return !currentStickState[btn] && previousStickState[btn];
            }

            return result;
        }

        bool isDir(Pad btn)
        {
            return btn == Pad.left || btn == Pad.right || btn == Pad.up || btn == Pad.down;
        }

        List<Object> getInputKeys(Pad btn)
        {
            List<Object> list = new List<Object>();
            switch (btn)
            {
                case Pad.left:
                    list.Add(Keys.Left);
                    break;
                case Pad.right:
                    list.Add(Keys.Right);
                    break;
                case Pad.up:
                    list.Add(Keys.Up);
                    break;
                case Pad.down:
                    list.Add(Keys.Down);
                    break;
                case Pad.a:
                    list.Add(Buttons.A);
                    list.Add(Keys.A);
                    list.Add(Keys.Z);
                    break;
                case Pad.b:
                    list.Add(Buttons.X);
                    list.Add(Keys.S);
                    list.Add(Keys.X);
                    break;
                case Pad.start:
                    list.Add(Buttons.Start);
                    list.Add(Keys.Enter);
                    break;
                case Pad.debug1:
                    list.Add(Buttons.B);
                    list.Add(Keys.Q);
                    break;
                case Pad.debug2:
                    list.Add(Buttons.Y);
                    list.Add(Keys.W);
                    break;
            }

            return list;
        }
    }
}
