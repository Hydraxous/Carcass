using System;
using System.Collections.Generic;
using System.Text;

namespace CarcassEnemy
{
    public class CarcassParameters
    {
        //Movement
        public float desiredFlightHeight = 13f;

        public float verticalFlySpeed = 14f;
        public float lateralFlySpeed = 9f;

        //Target-relative movement
        public float minTargetDistance = 12f;
        public float maxTargetDistance = 20f;
        public float positionalSlack = 1.5f;

        public int maxHealth = 65;

       

    }
}
