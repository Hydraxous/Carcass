using CarcassEnemy.Assets;
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
            if (Input.GetKeyDown(KeyCode.J))
                SpawnCarcass();

            if (Input.GetKeyDown(KeyCode.Keypad1))
                ForEachCarcassDo((c) => c.SpinAttack());

            if (Input.GetKeyDown(KeyCode.Keypad2))
                ForEachCarcassDo((c) => c.ShakeAttack());

            if (Input.GetKeyDown(KeyCode.Keypad4))
                ForEachCarcassDo((c) => c.SpawnEyes());

            if (Input.GetKeyDown(KeyCode.Keypad5))
                ForEachCarcassDo((c) => c.HealAction());

            if (Input.GetKeyDown(KeyCode.Keypad6))
                ForEachCarcassDo((c) => c.SummonSigil());

            if (Input.GetKeyDown(KeyCode.Keypad7))
                ForEachCarcassDo((c) => c.Enrage());

            if (Input.GetKeyDown(KeyCode.Keypad3))
                Carcass.DisableActionTimer = !Carcass.DisableActionTimer;
        }

        private void SpawnCarcass()
        {
            Transform mainCam = CameraController.Instance.transform;
            Ray ray = new Ray(mainCam.position, mainCam.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return;

            GameObject.Instantiate(CarcassAssets.Carcass, hit.point + (hit.normal * 0.1f), Quaternion.identity);
        }

        private void ForEachCarcassDo(Action<Carcass> onInvoke)
        {
            foreach (Carcass c in GameObject.FindObjectsOfType<Carcass>())
            {
                onInvoke?.Invoke(c);
            }
        }
    }
}
