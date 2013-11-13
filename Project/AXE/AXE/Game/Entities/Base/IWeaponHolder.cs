using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using AXE.Game.Entities;

namespace AXE.Game.Entities.Base
{
    interface IWeaponHolder
    {
        void setWeapon(IWeapon weapon);
        void removeWeapon();
        Player.Dir getFacing();
        int getDirectionAsSign(Player.Dir dir);
        Vector2 getPosition();
        Vector2 getHandPosition();
        void onAxeStolen();
    }
}
