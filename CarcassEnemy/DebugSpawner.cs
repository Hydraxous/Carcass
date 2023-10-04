using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{

    //Easy way to spawn in the enemy for testing.
    //TODO add to spawn catalogue
    public class DebugSpawner : MonoBehaviour
    {
        private static GameObject carcassPrefab;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.J))
                return;

            Transform mainCam = CameraController.Instance.transform;
            Ray ray = new Ray(mainCam.position, mainCam.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return;

            GameObject.Instantiate(GetPrefab(), hit.point+(hit.normal*0.1f), Quaternion.identity);
        }

        private GameObject GetPrefab()
        {
            if(carcassPrefab == null)
            {
                carcassPrefab = Plugin.AssetLoader.LoadAsset<GameObject>("Carcass");
            }

            return carcassPrefab;
        }
    }
}
