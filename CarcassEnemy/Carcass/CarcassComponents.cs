using System;
using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassComponents : MonoBehaviour
    {
        [SerializeField] private Transform rootTransform;
        public Transform RootTransform => rootTransform;

        [SerializeField] private Transform projectileLobPoint;
        public Transform ProjectileLobPoint => projectileLobPoint;

        [SerializeField] private Transform centerMass;
        public Transform CenterMass => centerMass;

        [SerializeField] private HashedTrigger spinHitbox;
        public HashedTrigger SpinHitbox => spinHitbox;

        [SerializeField] private Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        [SerializeField] private Animator animator;
        public Animator Animator => animator;

        [SerializeField] private CarcassAnimation animation;
        public CarcassAnimation Animation => animation;

        [SerializeField] private Machine machine;
        public Machine Machine => machine;

        [SerializeField] private EnemyIdentifier eid;
        public EnemyIdentifier EnemyIdentifier => eid;

        [SerializeField] private GameObject[] hitboxes;
        public GameObject[] Hitboxes
        {
            get
            {
                if(hitboxes == null)
                    return Array.Empty<GameObject>();

                return hitboxes;
            }
        }

        [SerializeField] private GameObject summonCirclePrefab;
        public GameObject SummonCirclePrefab => summonCirclePrefab;

        [SerializeField] private ProjectileDetector projectileDetector;
        public ProjectileDetector ProjectileDetector => projectileDetector;

        [SerializeField] private HookDetector hookDetector;
        public HookDetector HookDetector => hookDetector;

        [SerializeField] private GameObject healOrbPrefab;
        public GameObject HealOrbPrefab => healOrbPrefab;
    }
}
