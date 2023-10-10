using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassHealOrb : MonoBehaviour
    {
        private Carcass owner;
        public Carcass Owner => owner;

        [Configgy.Configgable("Carcass/HealOrb")] private static float healingRange = 3.5f;
        [Configgy.Configgable("Carcass/HealOrb")] private static float healAmount = 1.5f;

        [Configgy.Configgable("Carcass/HealOrb")] private static float flyAcceleration = 150f;
        [Configgy.Configgable("Carcass/HealOrb")] private static float flySpeed = 30f;
        [Configgy.Configgable("Carcass/HealOrb")] private static float randomSpawnForce = 15f;

        Vector3 velocity;
        private float lifeTime = 15f;

        public void SetOwner(Carcass owner)
        {
            if (this.owner != null)
                owner.OnDeath -= Owner_OnDeath;

            this.owner = owner;

            if (owner != null)
                owner.OnDeath += Owner_OnDeath;

            Vector3 direction = UnityEngine.Random.onUnitSphere * randomSpawnForce;
            direction.y = Mathf.Abs(direction.y);
            velocity = direction;
        }

        private void Owner_OnDeath(Carcass obj)
        {
            Die();
        }

        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if(lifeTime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (owner == null)
                return;

            Vector3 targetPos = owner.Components.CenterMass.position;
            Vector3 pos = transform.position;

            Vector3 toTarget = targetPos - pos;

            float targetDistance = toTarget.magnitude;
            if(targetDistance < healingRange)
            {
                Collected();
                return;
            }

            Vector3 direction = toTarget.normalized;
            velocity += direction * flyAcceleration * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, flySpeed);

            transform.position = pos + (velocity * Time.deltaTime);
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        private void Collected()
        {
            owner.Heal(healAmount);
            Die();
        }

        private void OnDestroy()
        {
            if(owner != null)
            {
                owner.OnDeath -= Owner_OnDeath;
            }
        }

    }
}
