using UnityEngine;

namespace CarcassEnemy
{
    public interface IEnemy
    {
        public void Instakill();
        public void GetHurt(HurtEventData hurtEventData);
        public void Knockback(Vector3 force);
        public float GetHealth();
        public bool IsAlive();
        public float GetLocationCritDamageMultiplier(string location);
        public EnemyIdentifier GetEnemyIdentifier();
    }
}
