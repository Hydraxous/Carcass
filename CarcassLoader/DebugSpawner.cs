using CarcassEnemy;
using CarcassLoader.Assets;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace CarcassLoader
{
    //Easy way to spawn in the enemy for testing.
    public class DebugSpawner : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
                SpawnCarcass();

            if (Input.GetKeyDown(KeyCode.Keypad1))
                ForEachCarcassDo((c) => c.SpinAttack());

            if (Input.GetKeyDown(KeyCode.Keypad2))
                ForEachCarcassDo((c) => c.ShakeAttack());

            if (Input.GetKeyDown(KeyCode.Keypad4))
                ForEachCarcassDo((c) => c.SummonEyes());

            if (Input.GetKeyDown(KeyCode.Keypad5))
                ForEachCarcassDo((c) => c.StartHealing());

            if (Input.GetKeyDown(KeyCode.Keypad6))
                ForEachCarcassDo((c) => c.SummonSigil());

            if (Input.GetKeyDown(KeyCode.Keypad7))
                ForEachCarcassDo((c) => c.Enrage());

            if (Input.GetKeyDown(KeyCode.Keypad8))
                ForEachCarcassDo((c) => 
                {
                    IEnumerable<EnemyIdentifier> eids = EnemyTracker.Instance.GetCurrentEnemies().Where(x=>x!=c.GetEnemyIdentifier());
                    EnemyIdentifier target = eids.ElementAt(UnityEngine.Random.Range(0, eids.Count()));
                    if(target != null)
                        c.SetTarget(target.transform);
                });

            if (Input.GetKeyDown(KeyCode.Keypad9))
                ForEachCarcassDo((c) => c.SetShouldAttackPlayer(false));
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
