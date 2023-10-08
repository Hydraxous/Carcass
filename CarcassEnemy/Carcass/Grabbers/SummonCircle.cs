using System.Collections;
using UnityEngine;

namespace CarcassEnemy
{
    public class SummonCircle : MonoBehaviour
    {
        [SerializeField] private GameObject grabbyHandPrefab;

        private float armSpawnDelay = 0.12f;
        private static float groundOffset = 0.15f;
        private int grabbyCount = 6;
        private float radius = 3f;

        private float lifeTime = 6f;
        private float timeUntilAttack = 0.75f;

        private bool dying;
        private bool isAttacking;
        private bool activated;

        private Transform target;

        public void SetTarget(Transform target)
        {
            this.target = target;
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

            if(activated)
            {
                timeUntilAttack -= Time.deltaTime;
                if(timeUntilAttack <= 0f && !isAttacking)
                {
                    Attack();
                }
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

        private void Die()
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
            StartCoroutine(PerformAttack());
        }

        private IEnumerator PerformAttack()
        {
            for (int i = 0; i < grabbyCount; i++)
            {
                SpawnArm();
                yield return new WaitForSeconds(armSpawnDelay);
            }

            yield return new WaitForSeconds(0.25f);
            yield return Shrink();
        }

        private bool isAlive;

        private IEnumerator PerformAttackAlt()
        {
            while (isAlive)
            {
                yield return new WaitForSeconds(armSpawnDelay);

                if (targetInRange)
                    SpawnArm();
            }
        }

        private void SpawnArm()
        {

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


        private void OnTriggerEnter(Collider col)
        {
            if (!col.CompareTag("Player"))
                return;

            targetInRange = true;
            activated = true;
        }

        private void OnTriggerExit(Collider col)
        {
            if (!col.CompareTag("Player"))
                return;

            targetInRange = false;
        }

        private bool targetInRange;
    }
}