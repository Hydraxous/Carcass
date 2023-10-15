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
        public string hitter;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("multiplier: ");
            sb.Append(multiplier);
            sb.Append("\ncrit: ");
            sb.Append(critMultiplier);
            sb.Append("\nhitter: ");
            sb.Append(hitter);
            sb.Append("\nforce: ");
            sb.Append(force);
            if(target != null)
            {
                sb.Append("\ntarget: ");
                sb.Append(target.name);
            }

            if (sourceWeapon != null)
            {
                sb.Append("\nsrcWep: ");
                sb.Append(sourceWeapon.name);
            }

            return sb.ToString();
        }
    }
}
