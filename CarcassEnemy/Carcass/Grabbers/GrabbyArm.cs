using CarcassEnemy.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class GrabbyArm : MonoBehaviour
    {

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject healOrbPrefab;
        [SerializeField] private GameObject spawnFX;
        [SerializeField] private HashedTrigger hurtBox;
        [SerializeField] private float normalizedStartTime = 0.25f;

        private static int[] attackAnimationHashes =
        {
            Animator.StringToHash("Scratch"),
            Animator.StringToHash("Punch"),
            Animator.StringToHash("Slash"),
            Animator.StringToHash("Grab"),
        };

        private Transform target;

        private void Start()
        {
            hurtBox.OnTriggerEntered += HurtBox_OnTriggerEntered;
            animator.Play(attackAnimationHashes[UnityEngine.Random.Range(0, attackAnimationHashes.Length)], 0, normalizedStartTime);
            
            if(spawnFX != null)
                GameObject.Instantiate(spawnFX, transform.position, Quaternion.identity);
        }

        private bool collected;

        private bool targetIsPlayer;

        public void SetTarget(Transform target)
        {
            this.target = target;
            targetIsPlayer = false;

            if (target != null)
                targetIsPlayer = target.GetComponentInParent<NewMovement>() != null;
        }

        private void HurtBox_OnTriggerEntered(Collider obj)
        {
            if (!ColliderIsTarget(obj))
                return;

            if (collected)
                return;

            collected = true;
            if(healOrbPrefab != null)
            {
                GameObject newOrb = GameObject.Instantiate(healOrbPrefab, transform.position, Quaternion.identity);

                if (owner != null)
                    if (newOrb.TryGetComponent<CarcassHealOrb>(out CarcassHealOrb orb))
                        orb.SetOwner(owner);
            }
        }

        private float timeLeft = 2f;

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0f && !dying)
            {
                Despawn();
            }
        }

        private bool dying;


        private void Despawn()
        {
            if (dying)
                return;

            dying = true;
            hurtBox.gameObject.SetActive(false);
            StartCoroutine(Death());
        }


        private IEnumerator Death()
        {
            yield return new WaitForSeconds(1f);
            GameObject.Destroy(gameObject);
        }


        private Carcass owner;
        public Carcass Owner => owner;
        public void SetOwner(Carcass carcass)
        {
            this.owner = carcass;
        }

        private bool ColliderIsTarget(Collider col)
        {
            if (col.CompareTag("Player"))
                return true;

            if (!targetIsPlayer && col.CompareTag("Enemy"))
                return true;

            return false;
        }

    }
}
