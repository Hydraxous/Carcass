using BepInEx;
using CarcassEnemy.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarcassEnemy
{
    public class Patcher : MonoBehaviour
    {
        private static Harmony harmony;

        private void Patch()
        {
            harmony = new Harmony(ConstInfo.GUID + ".harmony");
            harmony.PatchAll();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0 != SceneManager.GetActiveScene())
                return;

            MachineLinker.ClearCache();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            harmony.UnpatchSelf();
            instance = null;
        }

        private static Patcher instance;

        public static void QueryInstance()
        {
            if (instance != null)
                return;

            instance = new GameObject("CarcassPatcher").AddComponent<Patcher>();
            instance.Patch();
        }

    }
}
