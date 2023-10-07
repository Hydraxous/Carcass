using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace CarcassEnemy
{
    public class ProjectileDetector : MonoBehaviour
    {
        public event Action<Ray> OnProjectileDetected;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 14)
                return;

            ProcessCollider(other);
        }

        private void ProcessCollider(Collider other)
        {
            if(other.TryGetComponent<Grenade>(out Grenade grenade))
            {
                OnProjectileDetected?.Invoke(new Ray(grenade.transform.position, grenade.transform.forward));
            }

            if (other.TryGetComponent<Cannonball>(out Cannonball cannonBall))
            {
                OnProjectileDetected?.Invoke(new Ray(cannonBall.transform.position, cannonBall.transform.forward));
            }
        }
    }
}
