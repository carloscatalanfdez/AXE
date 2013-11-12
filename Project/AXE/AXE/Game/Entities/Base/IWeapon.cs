using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Base
{
    interface IWeapon
    {
        Vector2 getGrabPosition();
        void onThrow(int force, Player.Dir dir);
        void onGrab(IWeaponHolder holder);
        void onDrop();
    }
}
