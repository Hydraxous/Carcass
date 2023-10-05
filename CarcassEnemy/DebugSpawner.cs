using System;
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
            HandleSpawn();

            if (Input.GetKeyDown(KeyCode.Keypad1))
                PeformOnAll((c) => c.SpinAttack());

            if (Input.GetKeyDown(KeyCode.Keypad2))
                PeformOnAll((c) => c.ShakeAttack());

            if(Input.GetKeyDown(KeyCode.Keypad3))
                Carcass.DisableAttackTimer = !Carcass.DisableAttackTimer;
        }

        private void HandleSpawn()
        {
            if (!Input.GetKeyDown(KeyCode.J))
                return;

            Transform mainCam = CameraController.Instance.transform;
            Ray ray = new Ray(mainCam.position, mainCam.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return;

            GameObject.Instantiate(GetPrefab(), hit.point + (hit.normal * 0.1f), Quaternion.identity);
        }

        private void PeformOnAll(Action<Carcass> onInvoke)
        {
            foreach (Carcass c in GameObject.FindObjectsOfType<Carcass>())
            {
                onInvoke?.Invoke(c);
            }
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
