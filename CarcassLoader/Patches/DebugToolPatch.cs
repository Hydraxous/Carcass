using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarcassLoader.Patches
{
    [HarmonyPatch(typeof(NewMovement))]
    public static class DebugToolPatch
    {
        [HarmonyPatch("Start"), HarmonyPostfix]
        public static void OnStart(NewMovement __instance)
        {
            __instance.gameObject.AddComponent<DebugSpawner>();
        }
    }
}
