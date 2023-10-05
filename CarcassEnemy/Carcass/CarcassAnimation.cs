using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassAnimation : MonoBehaviour
    {
        [SerializeField] private Carcass carcass;
        [SerializeField] private Animator animator;

        //Animations
        private static readonly int dash = Animator.StringToHash("Dash");
        private static readonly int spin = Animator.StringToHash("Spin");
        private static readonly int dodge = Animator.StringToHash("Dodge");
        private static readonly int shake = Animator.StringToHash("Shake");
        private static readonly int stunned = Animator.StringToHash("Stunned");

        public void FireExplosiveProjectile()
        {
            carcass.FireExplosiveProjectile();
        }

        public void FireTrackingProjectile()
        {
            carcass.FireTrackingProjectile();
        }

        public void AttackDone()
        {
            carcass.AttackDone();
        }

        public void Dash()
        {
            animator.Play(dash, 0, 0);
        }

        public void Shake()
        {
            animator.Play(shake, 0, 0);
        }

        public void Stunned()
        {
            animator.Play(stunned, 0, 0);
        }

        public void Dodge()
        {
            animator.Play(dodge, 0, 0);
        }

        public void Spin()
        {
            animator.Play(spin, 0, 0);
        }

    }
}
