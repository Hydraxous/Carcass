using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarcassEnemy
{
    public class CarcassAnimation : MonoBehaviour
    {
        [SerializeField] private Carcass carcass;
        [SerializeField] private Transform vibrationTarget;
        [SerializeField] private Animator animator;

        //Animations
        private static readonly int dash = Animator.StringToHash("Dash");
        private static readonly int spin = Animator.StringToHash("Spin");
        private static readonly int dodge = Animator.StringToHash("Dodge");
        private static readonly int shake = Animator.StringToHash("Shake");
        private static readonly int stunned = Animator.StringToHash("Stunned");
        private static readonly int writhe = Animator.StringToHash("Writhe");
        private static readonly int summon = Animator.StringToHash("Summon");
        private static readonly int killEyes = Animator.StringToHash("KillEyes");
        private static readonly int death = Animator.StringToHash("Death");

        private static readonly string retreatingID = "Retreating";

        public Carcass Carcass
        {
            get
            {
                if(carcass == null)
                    carcass = GetComponentInParent<Carcass>();
                return carcass;
            }
        }


        public Animator Animator 
        { 
            get
            {
                if(animator == null)
                    animator = GetComponent<Animator>();
                return animator;
            } 
        }

        [SerializeField] private bool vibrate;
        public bool IsVibrating => vibrate;

        [SerializeField] private float vibrationRange = 0.15f;

        private Vector3 vibrationStartPosition;

        public void SetVibrationRange(float value)
        {
            this.vibrationRange = value;
        }

        public void SetVisible(bool visible)
        {
            if (vibrationTarget == null)
                return;

            vibrationTarget.gameObject.SetActive(visible);
        }

        public void SetVibrating(bool shaking)
        {
            this.vibrate = shaking;

            if(vibrationTarget != null)
            {
                if (shaking)
                    vibrationStartPosition = vibrationTarget.localPosition;
                else
                    vibrationTarget.localPosition = vibrationStartPosition;
            }
        }


        private void SetRetreatingBlend(float normalizedBlend)
        {
            Animator.SetFloat(retreatingID, normalizedBlend);
        }

        private void Update()
        {
            ResolveVibrate();
            ResolveRetreatBlend();
        }

        private void ResolveRetreatBlend()
        {
            Vector3 velocity = Carcass.Components.Rigidbody.velocity;
            float speedModifier = Mathf.Min(1, velocity.magnitude/3f);
            Vector3 carcassForward = Carcass.transform.forward;

            float velocityDirectionDot = Vector3.Dot(velocity.XZ(), -carcassForward.XZ());
            velocityDirectionDot = Mathf.Max(0, velocityDirectionDot);

            SetRetreatingBlend(velocityDirectionDot*speedModifier);
        }

        private void ResolveVibrate()
        {
            if (!vibrate)
                return;

            if (vibrationTarget == null)
                return;

            vibrationTarget.localPosition = vibrationStartPosition + UnityEngine.Random.onUnitSphere*vibrationRange;
        }

        public UnityEvent OnFireExplosiveProjectile;

        public void FireExplosiveProjectile()
        {
            OnFireExplosiveProjectile?.Invoke();
        }

        public UnityEvent OnSummonSigil;

        public void SpawnSigil()
        {
            OnSummonSigil?.Invoke();
        }

        public UnityEvent OnActionDone;

        public void ActionDone()
        {
            OnActionDone?.Invoke();
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

        public UnityEvent OnDodge;

        public void Dodge()
        {
            Animator.Play(dodge, 0, 0);
            OnDodge?.Invoke();
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

        public void Death()
        {
            Animator.Play(death, 0, 0);
        }

    }
}
