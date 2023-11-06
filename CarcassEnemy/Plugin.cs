using BepInEx;
using BepInEx.Configuration;
using CarcassEnemy.Assets;
using CarcassEnemy.Patches;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarcassEnemy
{
    [BepInPlugin(ConstInfo.GUID, ConstInfo.NAME, ConstInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetLoader AssetLoader { get; private set; }
        public static Plugin Instance { get; private set; }

        private static Harmony harmony;

        //private static ConfigBuilder config = new ConfigBuilder(ConstInfo.GUID, ConstInfo.NAME);
        public static ConfigEntry<bool> EnableInCyberGrind { get; private set; }

        private void Awake()
        {
            Instance = this;
            AssetLoader = new AssetLoader(Properties.Resources.Carcass);
            harmony = new Harmony(ConstInfo.GUID + ".harmony");
            harmony.PatchAll();
            
            //config.Build();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            
            Logger.LogInfo($"{ConstInfo.NAME} is loaded!");
            EnableInCyberGrind = Config.Bind<bool>("General", "EnableInCyberGrind", true, "Enables Carcass In Cybergrind");
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
