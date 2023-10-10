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

        private const string animationIndexName = "AttackIndex";
        private static int attackAnimation = Animator.StringToHash("Attack");
        private static float smoothRotSpeed = 14f;

        private Transform target;

        private void Start()
        {
            hurtBox.OnTriggerEntered += HurtBox_OnTriggerEntered;
            target = PlayerTracker.Instance.GetTarget();
            animator.SetInteger(animationIndexName, UnityEngine.Random.Range(0,4));
            animator.Play(attackAnimation, 0, 0f);
            
            if(spawnFX != null)
                GameObject.Instantiate(spawnFX, transform.position, Quaternion.identity);
        }

        private bool collected;

        private void HurtBox_OnTriggerEntered(Collider obj)
        {
            if (!obj.CompareTag("Player"))
                return;

            if (collected)
                return;

            collected = true;
            GameObject newOrb = GameObject.Instantiate(healOrbPrefab, transform.position, Quaternion.identity);
            CarcassHealOrb orb = newOrb.GetComponent<CarcassHealOrb>();
            orb.SetOwner(owner);
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

        private void LateUpdate()
        {
            if (target == null)
                return;
            
            Vector3 tPos = target.position;
            Vector3 toTarget = tPos - transform.position;

            Vector3 cross = Vector3.Cross(toTarget,transform.right);
            Quaternion desiredRot = Quaternion.LookRotation(cross);

            Quaternion rot = transform.rotation;
            transform.rotation = Quaternion.RotateTowards(rot,desiredRot,Time.deltaTime * smoothRotSpeed);
        }

        private Carcass owner;
        public Carcass Owner => owner;
        public void SetOwner(Carcass carcass)
        {
            this.owner = carcass;
        }

    }
}
