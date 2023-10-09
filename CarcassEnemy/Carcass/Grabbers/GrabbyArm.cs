using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class GrabbyArm : MonoBehaviour
    {

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject healOrbPrefab;
        [SerializeField] private HashedTrigger hurtBox;

        private const string animationIndexName = "AttackIndex";

        private Transform target;

        
        private static float smoothRotSpeed = 14f;

        private void Start()
        {
            hurtBox.OnTriggerEntered += HurtBox_OnTriggerEntered;
            target = PlayerTracker.Instance.GetTarget();
            animator.SetInteger(animationIndexName, UnityEngine.Random.Range(0,4));
        }

        private bool collected;

        private void HurtBox_OnTriggerEntered(Collider obj)
        {
            if (!obj.CompareTag("Player"))
                return;

            if (collected)
                return;

            if (!CarcassHealOrb.EnableOrbs)
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
            if(timeLeft <= 0f)
            {
                GameObject.Destroy(gameObject);
            }
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
