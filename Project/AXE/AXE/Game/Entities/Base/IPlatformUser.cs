using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AXE.Game.Entities.Base
{
    interface IPlatformUser
    {
        void onPlatformMovedWithDelta(Vector2 delta, Entity platform);
    }
}
