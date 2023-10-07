using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class GrabbyArm : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private const string animationIndexName = "AttackIndex";
        private Transform target;

        private void Start()
        {
            target = PlayerTracker.Instance.GetTarget();
            animator.SetInteger(animationIndexName, UnityEngine.Random.Range(0,4));
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

            return;
            Vector3 tPos = target.position;
            Vector3 toTarget = tPos - transform.position;

            transform.up = toTarget;
        }
    }
}
