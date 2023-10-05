using HarmonyLib;

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
