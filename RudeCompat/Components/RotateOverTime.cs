using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components
{
    public class RotateOverTime : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Vector3 localAxis;

        private void Update()
        {
            Quaternion rot = transform.localRotation;
            rot = rot * Quaternion.Euler(localAxis * speed * Time.deltaTime);
            transform.localRotation = rot;
        }
    }
}
