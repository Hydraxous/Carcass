using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField] private bool copyCameraRotation;

        private static Transform cameraTransform;

        private void LateUpdate()
        {
            Transform tf = GetCameraTransform();

            if(copyCameraRotation)
            {
                Quaternion rot = tf.rotation;
                transform.rotation = rot;
            }
            else
            {
                Vector3 camPosition = tf.position;
                Vector3 position = transform.position;
                Vector3 toCam = camPosition - position;
                transform.forward = toCam.normalized;
            }

        }

        private static Transform GetCameraTransform()
        {
            if(cameraTransform == null)
                if(Camera.main != null)
                    cameraTransform = Camera.main.transform;
         
            return cameraTransform;
        }

    }
}
