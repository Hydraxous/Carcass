using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace CarcassEnemy
{
    public class CarcassComponents : MonoBehaviour
    {
        [SerializeField] private Transform rootTransform;
        public Transform RootTransform => rootTransform;

        [SerializeField] private GameObject spinHitbox;
        public GameObject SpinHitbox => spinHitbox;

        [SerializeField] private Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        [SerializeField] private Animator animator;
        public Animator Animator => animator;

        [SerializeField] private Machine machine;
        public Machine Machine => machine;

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
    }
}
