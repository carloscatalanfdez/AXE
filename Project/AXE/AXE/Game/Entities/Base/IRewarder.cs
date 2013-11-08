using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AXE.Game.Entities.Contraptions
{
    interface IRewarder
    {
        void onReward(IContraption contraption);
    }
}
