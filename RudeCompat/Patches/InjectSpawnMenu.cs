using CarcassEnemy.Assets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Patches
{
    [HarmonyPatch(typeof(SpawnMenu))]
    public static class InjectSpawnMenu
    {
        private static bool injected;

        [HarmonyPatch("Awake"), HarmonyPrefix]
        private static void Start(ref SpawnableObjectsDatabase ___objects)
        {
            //Only add our content once, since the ScriptableObject's data will persist between scene loads.
            if (injected)
                return;

            SpawnableObject carcassObject = CarcassAssets.GetCarcassSpawnableObject();
            SpawnableObject[] enemies = ___objects.enemies;


            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                    continue;

                //If it exists in the database, dont need to add it.
                if (enemies[i] == carcassObject)
                    return;
            }

            SpawnableObject[] newEnemies = new SpawnableObject[enemies.Length + 1];
            Array.Copy(enemies, newEnemies, enemies.Length);
            newEnemies[newEnemies.Length - 1] = carcassObject;
            ___objects.enemies = newEnemies;
            injected = true;
        }

        [HarmonyPatch(nameof(SpawnMenu.RebuildIcons)), HarmonyPostfix]
        private static void AddEnemyIcon(ref Dictionary<string,Sprite> ___spriteIcons)
        {
            Sprite icon = CarcassAssets.CarcassIcon;
            ___spriteIcons.Add(icon.name, icon);
        }

        
    }
}
