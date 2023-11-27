using CarcassEnemy.Assets;
using Logic;
using System.Collections;
using ULTRAKILL.Cheats;
using UnityEngine;

namespace CarcassEnemy
{
    public class SummonCircle : MonoBehaviour
    {
        [SerializeField] private GameObject grabbyHandPrefab;
        [SerializeField] private GameObject activateFX;
        [SerializeField] private Animator animator;

        private static string animatorActivatedName = "Activated";

        private float armSpawnDelay = 0.12f;
        private static float groundOffset = 0.15f;
        private int grabbyCount = 6;
        private float radius = 3f;

        private float lifeTime = 18f;
        private float timeUntilAttack = 0.75f;

        private bool dying;
        private bool isAttacking;
        private bool activated;

        private Transform target;
        private bool targetIsPlayer;

        public void SetTarget(Transform target)
        {
            this.target = target;
            targetIsPlayer = false;

            if(target != null)
                targetIsPlayer = target.GetComponentInParent<NewMovement>() != null;
        }

        public void SetLifeTime(float seconds)
        {
            this.lifeTime = seconds;
        }


        private void Update()
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Die();
            }
        }

        private void LateUpdate()
        {
            if (target == null || dying)
                return;

            Vector3 targetPosition = target.position;

            if (!Physics.Raycast(targetPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return;

            transform.position = hit.point + (hit.normal * groundOffset);
            transform.up = hit.normal;
        }

        public void Die()
        {
            if (dying)
                return;

            dying = true;
            StartCoroutine(Shrink());
        }

        private void Attack()
        {
            if (isAttacking)
                return;

            isAttacking = true;
            
            if(activateFX != null)
                GameObject.Instantiate(activateFX, transform);
            
            StartCoroutine(PerformAttack());
        }

        private IEnumerator PreAttackDelay()
        {
            float time = timeUntilAttack;
            float timer = time;
            while (timer > 0f)
            {
                float i = 1 - (timer / time);

                animator.SetFloat(animatorActivatedName, i);
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }

            animator.SetFloat(animatorActivatedName, 1f);
        }

        private IEnumerator PerformAttack()
        {
            yield return PreAttackDelay();

            for (int i = 0; i < grabbyCount; i++)
            {
                SpawnArm();
                yield return new WaitForSeconds(armSpawnDelay);
            }

            yield return new WaitForSeconds(0.25f);
            yield return Shrink();
        }

        private void SpawnArm()
        {
            if (dying)
                return;

            Vector3 targetPosition = target.position;
            Vector3 pos = transform.position;

            Vector3 spawnPosition = transform.position;

            //Player is standing within circle radius.
            if(Vector3.Magnitude(targetPosition.XZ()-pos.XZ()) < radius)
            {
                spawnPosition = new Vector3(targetPosition.x, pos.y, targetPosition.z);
            }
            else
            {
                Vector2 disc = Random.insideUnitCircle;
                Quaternion rot = transform.rotation;
                spawnPosition += rot * (new Vector3(disc.x, 0f, disc.y) * radius);
            }

            float randomAngle = UnityEngine.Random.value * 360f;
            Quaternion spawnRotation = Quaternion.Euler(0, randomAngle, 0);
            GameObject newArm = GameObject.Instantiate(grabbyHandPrefab, spawnPosition, spawnRotation);

            if(newArm.TryGetComponent<GrabbyArm>(out GrabbyArm arm))
            {
                arm.SetOwner(owner);
                arm.SetTarget(target);
            }
        }

        private IEnumerator Shrink()
        {
            Vector3 scale = transform.localScale;
            float time = 1.2f;
            float timer = time;
            while(timer > 0f)
            {
                float i = 1 - (timer / time);

                transform.localScale = Vector3.Lerp(scale,new Vector3(0,scale.y, 0f), i);
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }

            transform.localScale = Vector3.zero;
            Destroy(gameObject);
        }

        private Carcass owner;
        public Carcass Owner => owner;
        public void SetOwner(Carcass owner)
        {
            this.owner = owner;
        }

        private void OnTriggerEnter(Collider col)
        {
            if (!CheckForDamagable(col))
                return;

            activated = true;
            Attack();
        }

        private bool CheckForDamagable(Collider col)
        {
            if (col.CompareTag("Player"))
                return true;

            if (!targetIsPlayer && col.CompareTag("Enemy"))
                return true;

            return false;
        }
      
    }
}