using UnityEngine;

namespace CarcassEnemy
{
    public interface ICustomEnemy
    {
        public void Instakill();
        public void GetHurt(HurtEventData hurtEventData);
        public void Knockback(Vector3 force);
    }
}
