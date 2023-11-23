using CarcassLoader.Assets;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarcassLoader.Patches
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

            SpawnableObject funkoObject = CarcassAssets.GetCarcassFunkoSpawnableObject();
            SpawnableObject carcassObject = CarcassAssets.GetCarcassSpawnableObject();


            SpawnableObject[] enemies = ___objects.enemies;
            SpawnableObject[] newEnemies = new SpawnableObject[enemies.Length + 1];
            Array.Copy(enemies, newEnemies, enemies.Length);
            newEnemies[newEnemies.Length - 1] = carcassObject;
            ___objects.enemies = newEnemies;

            SpawnableObject[] objects = ___objects.objects;
            SpawnableObject[] newObjects = new SpawnableObject[objects.Length + 1];
            Array.Copy(objects, newObjects, objects.Length);
            newObjects[newObjects.Length - 1] = funkoObject;
            ___objects.objects = newObjects;

            injected = true;
        }

        [HarmonyPatch(nameof(SpawnMenu.RebuildIcons)), HarmonyPostfix]
        private static void AddEnemyIcon(ref Dictionary<string,Sprite> ___spriteIcons)
        {
            ___spriteIcons.Add(CarcassAssets.CarcassIcon.name, CarcassAssets.CarcassIcon);
            ___spriteIcons.Add(CarcassAssets.FunkoSpawnIcon.name, CarcassAssets.FunkoSpawnIcon);
        }

        
    }
}
