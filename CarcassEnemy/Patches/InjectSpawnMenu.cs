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
        private static void AddEnemy(ref SpawnableObjectsDatabase ___objects)
        {
            //Only add our content once, since the ScriptableObject's data will persist between scene loads.
            if (injected)
                return;

            injected = true;

            SpawnableObject[] enemies = ___objects.enemies;
            SpawnableObject[] newEnemies = new SpawnableObject[enemies.Length + 1];
            Array.Copy(enemies, newEnemies, enemies.Length);
            newEnemies[newEnemies.Length - 1] = GetCarcassSpawnableObject();
            ___objects.enemies = newEnemies;
        }

        [HarmonyPatch(nameof(SpawnMenu.RebuildIcons)), HarmonyPostfix]
        private static void AddEnemyIcon(ref Dictionary<string,Sprite> ___spriteIcons)
        {
            Sprite icon = CarcassAssets.CarcassIcon;
            ___spriteIcons.Add(icon.name, icon);
        }

        private static SpawnableObject carcassSpawnable;

        private static SpawnableObject GetCarcassSpawnableObject()
        {
            if (carcassSpawnable == null)
                carcassSpawnable = BuildCarcassObject();
            return carcassSpawnable;
        }

        private static SpawnableObject BuildCarcassObject()
        {
            SpawnableObject spawnable = ScriptableObject.CreateInstance<SpawnableObject>();
            spawnable.name = "Carcass_SpawnableObject";
            spawnable.identifier = "Carcass";
            spawnable.objectName = "Carcass";
            spawnable.gameObject = CarcassAssets.Carcass;
            spawnable.spawnableObjectType = SpawnableObject.SpawnableObjectDataType.Enemy;
            spawnable.description = "Carcass Enemy";
            spawnable.enemyType = EnemyType.Filth; //EnemyType controls progress based sandbox unlocks. So this should be always unlocked.
            spawnable.iconKey = CarcassAssets.CarcassIcon.name;
            spawnable.backgroundColor = new Color(0.349f,0.349f,0.349f, 1f);
            //spawnable.preview = CarcassAssets.SpawnMenuPreview;

            return spawnable;
        }
    }
}
