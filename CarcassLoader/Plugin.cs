using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace CarcassLoader
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_NAME = "CarcassLoader";
        public const string PLUGIN_GUID = "Hydraxous.ULTRAKILL.CarcassLoader";
        public const string PLUGIN_VERSION = "1.0.7";

        const string CARCASS_RUNTIME = "CarcassEnemy.dll";

        public static AssetLoader AssetLoader { get; private set; }
        public static Plugin Instance { get; private set; }

        private static Harmony harmony;
        public static ConfigEntry<bool> EnableInCyberGrind { get; private set; }
        public static ConfigEntry<bool> EnableDebugTool { get; private set; }

        private void Awake()
        {
            if (!LoadRuntime())
            {
                this.enabled = false;
                Logger.LogFatal("Carcass failed to load.");
                return;
            }

            Logger.LogInfo(CARCASS_RUNTIME + " loaded successfully.");

            Instance = this;
            AssetLoader = new AssetLoader(Properties.Resources.Carcass);
            harmony = new Harmony(PLUGIN_GUID + ".harmony");
            harmony.PatchAll();

            Logger.LogInfo($"{PLUGIN_NAME} is loaded!");
            EnableInCyberGrind = Config.Bind<bool>("General", "EnableInCyberGrind", true, "Enables Carcass In Cybergrind");
            EnableDebugTool = Config.Bind<bool>("General", "EnableDebugTool", false, "Enables debug tool binds");
        }

    
        private void OnDestroy()
        {
            harmony.UnpatchSelf();
            AssetLoader.Unload();
        }

        private bool LoadRuntime()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dllFilePath = Path.Combine(dir, CARCASS_RUNTIME);

            if(!File.Exists(dllFilePath))
            {
                Logger.LogFatal("CarcassEnemy.dll runtime could not be found!");
                return false;
            }

            try
            {
                Assembly asm = Assembly.LoadFile(dllFilePath);

                if(asm == null)
                {
                    Logger.LogFatal("CarcassEnemy.dll runtime is null! Your dll may be corrupted.");
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Logger.LogFatal(ex);
            }

            return false;
        }
    }
}
