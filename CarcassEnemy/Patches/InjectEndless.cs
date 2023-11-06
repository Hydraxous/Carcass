using CarcassEnemy.Assets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Patches
{
    [HarmonyPatch(typeof(EndlessGrid))]
    public static class InjectEndless
    {

        private static bool injected;

        [HarmonyPatch("Start"), HarmonyPostfix]
        public static void OnStart(EndlessGrid __instance, ref PrefabDatabase ___prefabs)
        {
            if (injected || !Plugin.EnableInCyberGrind.Value)
                return;

            injected = true;

            EndlessEnemy[] oldSpecial = ___prefabs.specialEnemies;
            EndlessEnemy[] newSpecial = new EndlessEnemy[oldSpecial.Length + 1];

            Array.Copy(oldSpecial, newSpecial, oldSpecial.Length);

            EndlessEnemy carcass = ScriptableObject.CreateInstance<EndlessEnemy>();
            carcass.name = "CarcassEndlessData";
            carcass.spawnCost = 60;
            carcass.spawnWave = 16;
            carcass.costIncreasePerSpawn = 30;
            carcass.enemyType = EnemyType.Mindflayer;
            carcass.prefab = CarcassAssets.Carcass;

            newSpecial[newSpecial.Length - 1] = carcass;

            ___prefabs.specialEnemies = newSpecial;
        }
    }
}
