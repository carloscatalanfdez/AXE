using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXE.Game.Entities.Base
{
    interface IHazardProvider
    {
        void onSuccessfulHit(Player other);
    }
}
