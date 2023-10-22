using CarcassEnemy.Components.FX;
using System;
using UnityEngine;

namespace CarcassEnemy
{
    [RequireComponent(typeof(Carcass))]
    public class CarcassComponents : MonoBehaviour
    {
        [Header("Component References")]

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

        [SerializeField] private ProjectileDetector projectileDetector;
        public ProjectileDetector ProjectileDetector => projectileDetector;

        [SerializeField] private HookDetector hookDetector;
        public HookDetector HookDetector => hookDetector;

        [SerializeField] private MaterialChanger materialChanger;
        public MaterialChanger MaterialChanger => materialChanger;

        [SerializeField] private GameObject[] hitboxes;
        public GameObject[] Hitboxes
        {
            get
            {
                if (hitboxes == null)
                    return Array.Empty<GameObject>();

                return hitboxes;
            }
        }


        [Header("Asset References")]

        [SerializeField] private GameObject healOrbPrefab;
        public GameObject HealOrbPrefab => healOrbPrefab;

        [SerializeField] private GameObject summonCirclePrefab;
        public GameObject SummonCirclePrefab => summonCirclePrefab;

        [SerializeField] private GameObject psychosisFXPrefab;
        public GameObject PsychosisFXPrefab => psychosisFXPrefab;

        [SerializeField] private GameObject healAuraFX;
        public GameObject HealAuraFX => healAuraFX;

        [SerializeField] private GameObject healFX;
        public GameObject HealFX => healFX;

        [SerializeField] private GameObject stunnedFX;
        public GameObject StunnedFX => stunnedFX;

        [SerializeField] private GameObject genericSpawnFX;
        public GameObject GenericSpawnFX => genericSpawnFX;

        [SerializeField] private GameObject hookSnapFX;
        public GameObject HookSnapFX => hookSnapFX;

        [SerializeField] private GameObject bloodSprayFX;
        public GameObject BloodSprayFX => bloodSprayFX;

        [SerializeField] private GameObject carcassScreamPrefab;
        public GameObject CarcassScreamPrefab => carcassScreamPrefab;

        [SerializeField] private GameObject carcassDeathFX;
        public GameObject CarcassDeathFX => carcassDeathFX;

        [SerializeField] private Material eyeMaterialOverride;
        public Material EyeMaterialOverride => eyeMaterialOverride;

        [SerializeField] private Material[] enragedMaterials;
        public Material[] EnragedMaterials => enragedMaterials;

        [Header("ULTRAKILL ASSETS - Will auto-populate if left null.")]

        [SerializeField] private GameObject droneFlesh;
        public GameObject DroneFlesh => droneFlesh;

        [SerializeField] private GameObject homingProjectilePrefab;
        public GameObject HomingProjectilePrefab => homingProjectilePrefab;

        [SerializeField] private GameObject lobbedExplosiveProjectilePrefab;
        public GameObject LobbedExplosiveProjectilePrefab => lobbedExplosiveProjectilePrefab;

        [SerializeField] private GameObject lightShaftFX;
        public GameObject LightShaftFX => lightShaftFX;
    }
}
