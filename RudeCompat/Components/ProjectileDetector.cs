using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace CarcassEnemy
{
    public class ProjectileDetector : MonoBehaviour
    {
        public event Action<Collider> OnProjectileDetected;

        private HashSet<Collider> detected = new HashSet<Collider>();
        
     
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 14)
                return;

            if (detected.Contains(other))
                return;

            detected.Add(other);
            ProcessCollider(other);
        }

        private void ProcessCollider(Collider other)
        {
            if(other.TryGetComponent<Grenade>(out Grenade grenade))
            {
                OnProjectileDetected?.Invoke(other);
            }

            if (other.TryGetComponent<Cannonball>(out Cannonball cannonBall))
            {
                OnProjectileDetected?.Invoke(other);
            }
        }
    }
}
