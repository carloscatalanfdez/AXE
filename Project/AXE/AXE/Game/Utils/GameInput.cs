using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Input;

using bEngine;
using Microsoft.Xna.Framework;
using AXE.Game.Control;

namespace AXE.Common
{
    enum PadButton { left, right, up, down, a, b, c, start, coin, debug };

    class GameInput
    {
        public static GameInput getInstance(PlayerIndex index = PlayerIndex.One)
        {
            switch (index)
            {
                default:
                case PlayerIndex.One:
                    return Controller.getInstance().playerAInput;
                case PlayerIndex.Two:
                    return Controller.getInstance().playerBInput;
            }
        }

        public PlayerIndex index;
        public bInput input = bGame.input;

        public float joyHThreshold = 0.3f;
        public float joyVThreshold = 0.3f;

        public Dictionary<PadButton, bool> currentStickState, previousStickState;
        private Dictionary<PadButton, List<Object>> mappingConf;

        public GameInput(PlayerIndex index)
        {
            currentStickState = new Dictionary<PadButton, bool>();
            previousStickState = new Dictionary<PadButton, bool>();

            this.index = index;
        }

        public void update()
        {
            foreach (PadButton key in currentStickState.Keys)
                previousStickState[key] = currentStickState[key];

            currentStickState[PadButton.left] = check(PadButton.left);
            currentStickState[PadButton.right] = check(PadButton.right);
            currentStickState[PadButton.up] = check(PadButton.up);
            currentStickState[PadButton.down] = check(PadButton.down);
        }

        public bool check(PadButton btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.check((Keys)i);
                else
                    result = input.check((Buttons)i, index);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                switch (btn)
                {
                    case PadButton.left:
                        result = input.left(index);
                        break;
                    case PadButton.right:
                        result = input.right(index);
                        break;
                    case PadButton.up:
                        result = input.up(index);
                        break;
                    case PadButton.down:
                        result = input.down(index);
                        break;
                }
            }

            return result;
        }

        public bool pressed(PadButton btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.pressed((Keys)i);
                else
                    result = input.pressed((Buttons)i, index);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                return currentStickState[btn] && !previousStickState[btn];
            }

            return result;
        }

        public bool released(PadButton btn)
        {
            List<Object> inputs = getInputKeys(btn);

            bool result = false;
            foreach (Object i in inputs)
            {
                if (i is Keys)
                    result = input.released((Keys)i);
                else
                    result = input.released((Buttons)i, index);

                if (result)
                    break;
            }

            if (!result && isDir(btn))
            {
                return !currentStickState[btn] && previousStickState[btn];
            }

            return result;
        }

        bool isDir(PadButton btn)
        {
            return btn == PadButton.left || btn == PadButton.right || btn == PadButton.up || btn == PadButton.down;
        }

        List<Object> getInputKeys(PadButton btn)
        {
            if (mappingConf == null)
            {
                mappingConf = getDefaultMappingConf();
            }

            return mappingConf[btn];
        }

        private Dictionary<PadButton, List<Object>> getDefaultMappingConf()
        {
            if (index == PlayerIndex.One)
                return new Dictionary<PadButton, List<Object>> 
                {
                    { PadButton.left, new List<Object> { Keys.Left } },
                    { PadButton.right, new List<Object> { Keys.Right } },
                    { PadButton.up, new List<Object> { Keys.Up } },
                    { PadButton.down, new List<Object> { Keys.Down } },
                    { PadButton.a, new List<Object> { Buttons.A, Keys.A, Keys.Z } },
                    { PadButton.b, new List<Object> { Buttons.X, Keys.S, Keys.X } },
                    { PadButton.start, new List<Object> { Buttons.Start, Keys.Enter, Keys.D1, Keys.D2 } },
                    { PadButton.coin, new List<Object> { Buttons.B, Keys.Q, Keys.D5, Keys.D6 } },
                    { PadButton.c, new List<Object> { Buttons.Y, Keys.W } },
                    { PadButton.debug, new List<Object> { Buttons.Back, Keys.Tab } }
                };
            else
                return new Dictionary<PadButton, List<Object>> 
                {
                    { PadButton.left, new List<Object> { Keys.Left } },
                    { PadButton.right, new List<Object> { Keys.Right } },
                    { PadButton.up, new List<Object> { Keys.Up } },
                    { PadButton.down, new List<Object> { Keys.Down } },
                    { PadButton.a, new List<Object> { Buttons.A, Keys.A, Keys.Z } },
                    { PadButton.b, new List<Object> { Buttons.X, Keys.S, Keys.X } },
                    { PadButton.start, new List<Object> { Buttons.Start, Keys.Enter, Keys.D1, Keys.D2 } },
                    { PadButton.coin, new List<Object> { Buttons.B, Keys.Q, Keys.D5, Keys.D6 } },
                    { PadButton.c, new List<Object> { Buttons.Y, Keys.W } },
                    { PadButton.debug, new List<Object> { Buttons.Back, Keys.Tab } }
                };
        }

        public void setMapping(Dictionary<PadButton, List<Object>> mappingConf)
        {
            this.mappingConf = mappingConf;
        }
    }
}
