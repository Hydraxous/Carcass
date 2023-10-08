using BepInEx;
using CarcassEnemy.Patches;
using Configgy;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace CarcassEnemy
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    [BepInDependency("Hydraxous.HydraDynamics", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Hydraxous.ULTRAKILL.Configgy", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetLoader AssetLoader { get; private set; }
        public static Plugin Instance { get; private set; }

        private static Harmony harmony;

        private static ConfigBuilder config = new ConfigBuilder(ConstInfo.GUID, ConstInfo.NAME);

        private void Awake()
        {
            Instance = this;
            AssetLoader = new AssetLoader(Properties.Resources.Carcass);
            harmony = new Harmony(ConstInfo.GUID + ".harmony");
            harmony.PatchAll();
            config.Build();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            
            Logger.LogInfo($"{ConstInfo.NAME} is loaded!");
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
        }
    }
}
