using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXE.Game.Entities.Base
{
    interface IHazard
    {
        void setOwner(IHazardProvider owner);
        IHazardProvider getOwner();
        Player.DeathState getType();
        void onHit();
    }
}
