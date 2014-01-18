using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AXE.Game.Utils;
using AXE.Game.Entities.Base;
using Microsoft.Xna.Framework.Input;

namespace AXE.Game.Entities.Enemies
{
    class SineFloaterSpawner : Enemy
    {
        // Parameters
        public int height;
        public int spawnDelay;
        public Dir spawnDirection;
        public float critterhspeed;
        float angleDelta;

        // Gamestate vars

        public SineFloaterSpawner(int x, int y, int height, int delay, Dir direction, float hspeed, float angleDelta)
            : base(x, y)
        {
            this.height = height;
            this.spawnDelay = delay;
            this.spawnDirection = direction;
            this.critterhspeed = hspeed;
            this.angleDelta = angleDelta;
        }

        public override void init()
        {
            base.init();

            wrappable = false;

            setTimer(0, spawnDelay, spawnDelay);
        }

        public override void onUpdate()
        {
            if (input.pressed(Keys.F))
                setTimer(0, 0, 0);
        }

        public override void onTimer(int n)
        {
            int spawnY = y+height/2;

            world.add(new Enemies.SineFloater(x, spawnY, spawnDirection, height / 2, critterhspeed, angleDelta), "enemy");

            setTimer(0, spawnDelay, spawnDelay);
        }        
    }
}
