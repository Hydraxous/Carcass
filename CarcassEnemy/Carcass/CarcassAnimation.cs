using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassAnimation : MonoBehaviour
    {
        [SerializeField] private Carcass carcass;

        public Carcass Carcass 
        { 
            get
            {
                if(carcass == null)
                    carcass = GetComponentInParent<Carcass>();
                return carcass;
            } 
        }

        [SerializeField] private Animator animator;

        public Animator Animator 
        { 
            get
            {
                if(animator == null)
                    animator = GetComponent<Animator>();
                return animator;
            } 
        }

        //Animations
        private static readonly int dash = Animator.StringToHash("Dash");
        private static readonly int spin = Animator.StringToHash("Spin");
        private static readonly int dodge = Animator.StringToHash("Dodge");
        private static readonly int shake = Animator.StringToHash("Shake");
        private static readonly int stunned = Animator.StringToHash("Stunned");
        private static readonly int writhe = Animator.StringToHash("Writhe");
        private static readonly int summon = Animator.StringToHash("Summon");
        private static readonly int killEyes = Animator.StringToHash("KillEyes");

        public void FireExplosiveProjectile()
        {
            Carcass.FireExplosiveProjectile();
        }

        public void FireTrackingProjectile()
        {
            Carcass.FireTrackingProjectile();
        }

        public void SpawnSigil()
        {
            Carcass.SpawnSigil();
        }

        public void AttackDone()
        {
            Carcass.AttackDone();
        }

        public void Dash()
        {
            Animator.Play(dash, 0, 0);
        }

        public void Shake()
        {
            Animator.Play(shake, 0, 0);
        }

        public void Stunned()
        {
            Animator.Play(stunned, 0, 0);
        }

        public void Dodge()
        {
            Animator.Play(dodge, 0, 0);
        }

        public void Spin()
        {
            Animator.Play(spin, 0, 0);
        }

        public void Summon()
        {
            Animator.Play(summon, 0, 0);
        }

        public void Writhe()
        {
            Animator.Play(writhe, 0, 0);
        }

        public void KillEyes()
        {
            Animator.Play(killEyes, 0, 0);
        }

    }
}
