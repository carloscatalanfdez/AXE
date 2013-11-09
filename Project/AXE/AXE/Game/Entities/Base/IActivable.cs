using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXE.Game.Entities.Base
{
    interface IActivable
    {
        bool activate(Entity agent);    // Agent activates you
        void notifyAgent();             // Notify agent of activation action end (for anim purposes)
    }

    interface ISwitch : IActivable
    {
        // Heredates activate
        void on(Entity agent);
        void off(Entity agent);
        bool isOn();
    }
}
