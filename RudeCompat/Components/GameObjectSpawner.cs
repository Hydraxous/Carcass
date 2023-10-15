using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components
{
    public class GameObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        [SerializeField] private Transform spawnGuide;

        [SerializeField] private bool copyPosition = true;
        [SerializeField] private bool copyRotation = true;
        [SerializeField] private bool resetLocalScale = false;
        [SerializeField] private bool parentToGuide = true;

        private GameObject spawnedObject;

        public void DestroySpawnedObject()
        {
            if (spawnedObject == null)
                return;

            GameObject destroyObj = spawnedObject;
            GameObject.Destroy(destroyObj);
        }

        public void Spawn()
        {
            Transform tf = GetSpawnGuide();
            if(resetLocalScale && copyRotation && copyPosition && parentToGuide)
            {
                spawnedObject = GameObject.Instantiate(prefab, tf);
                return;
            }

            spawnedObject = GameObject.Instantiate(prefab);

            Vector3 localScale = spawnedObject.transform.localScale;

            if (copyPosition)
                spawnedObject.transform.position = tf.position;

            if (copyRotation)
                spawnedObject.transform.rotation = tf.rotation;

            if (parentToGuide)
                spawnedObject.transform.parent = tf;

            if (resetLocalScale)
                spawnedObject.transform.localScale = localScale;

        }

        private Transform GetSpawnGuide()
        {
            if (spawnGuide == null)
                return transform;

            return spawnGuide;
        }
    }
}
