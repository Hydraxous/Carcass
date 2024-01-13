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

        [SerializeField] private Transform target;

        [SerializeField] private float healingRange = 3.5f;
        [SerializeField] private float healAmount = 1.5f;

        [SerializeField] private float flyAcceleration = 150f;
        [SerializeField] private float flySpeed = 30f;
        [SerializeField] private float randomSpawnForce = 15f;

        [SerializeField] private bool triggerEyeSpawnOnPickup = true;

        Vector3 velocity;

        private bool targetSet;

        public void SetSpawnEye(bool enabled)
        {
            this.triggerEyeSpawnOnPickup = enabled;
        }

        public void SetTarget(Transform target) 
        {
            targetSet = true;
            this.target = target;
        }

        public void SetOwner(Carcass owner)
        {
            this.owner = owner;

            if (owner != null)
            {
                SetTarget(owner.Components.CenterMass);
            }

            Vector3 direction = UnityEngine.Random.onUnitSphere * randomSpawnForce;
            direction.y = Mathf.Abs(direction.y);
            velocity = direction;
        }

        private void Update()
        {
            if(owner != null)
            {
                if(owner.Dead)
                {
                    Die();
                    return;
                }
            }

            if(targetSet && target == null)
            {
                Die();
                return;
            }

            if (target == null)
                return;

            Vector3 targetPos = target.position;
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
            if (owner != null)
            {
                owner.Heal(healAmount);
                if (triggerEyeSpawnOnPickup)
                    owner.SpawnEye();
            }
            
            Die();
        }
    }
}
