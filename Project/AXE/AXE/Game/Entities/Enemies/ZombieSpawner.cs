using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AXE.Game.Utils;
using AXE.Game.Entities.Base;

namespace AXE.Game.Entities.Enemies
{
    class ZombieSpawner : Enemy
    {
        const int COOL_DOWN_TIMER = 0;

        List<Zombie> spawnedZombies;
        int nSpawnableZombies;

        int spawnCoolDownBaseTime;
        int spawnCoolDownOptionalTime;

        public ZombieSpawner(int x, int y, int nSpawnableZombies = 1)
            : base(x, y)
        {
            collidable = false;
            spawnedZombies = new List<Zombie>();
            this.nSpawnableZombies = nSpawnableZombies;
        }

        public override void init()
        {
            base.init();

            spawnCoolDownBaseTime = 300;
            spawnCoolDownOptionalTime = 50;
        }

        public override void onUpdate()
        {
            base.onUpdate();

            // Clear dead zombies
            List<Zombie> deathRow = new List<Zombie>();
            foreach (Zombie zombie in spawnedZombies)
            {
                if (zombie.state == Zombie.State.Dead)
                {
                    deathRow.Add(zombie);
                }
            }

            if (deathRow.Count > 0)
            {
                // Don't spawn immediately
                coolDown();
            }

            foreach (Zombie zombie in deathRow)
            {
                spawnedZombies.Remove(zombie);
            }

            // Add new ones
            if (timer[COOL_DOWN_TIMER] < 0 && spawnedZombies.Count < nSpawnableZombies)
            {
                Zombie zombie = new Zombie(x, y);
                if (instancePlace(zombie.mask, "enemy") == null)
                {
                    spawnedZombies.Add(zombie);
                    world.add(zombie, "enemy");
                    coolDown();
                }
            }
        }

        protected void coolDown()
        {
            timer[COOL_DOWN_TIMER] = spawnCoolDownBaseTime + Tools.random.Next(spawnCoolDownOptionalTime) - spawnCoolDownOptionalTime / 2;
        }
    }
}
