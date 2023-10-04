using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public struct HurtEventData
    {
        public GameObject target;
        public Vector3 force;
        public float multiplier;
        public float critMultiplier; 
        public GameObject sourceWeapon;
    }
}
