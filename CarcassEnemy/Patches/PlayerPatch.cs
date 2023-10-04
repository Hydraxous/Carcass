using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarcassEnemy.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    public static class PlayerPatch
    {
        [HarmonyPatch("Start"), HarmonyPostfix]
        public static void Postfix(NewMovement __instance)
        {
            __instance.gameObject.AddComponent<DebugSpawner>();
        }
    }
}
