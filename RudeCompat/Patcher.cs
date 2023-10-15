using BepInEx;
using CarcassEnemy.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarcassEnemy
{
    public class Patcher : MonoBehaviour
    {
        public static AssetLoader AssetLoader { get; private set; }
        private static Harmony harmony;

        private void Patch()
        {
            if(AssetLoader == null)
                AssetLoader = new AssetLoader(Properties.Resources.Carcass);

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
            AssetLoader.Unload();
            AssetLoader = null;
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
