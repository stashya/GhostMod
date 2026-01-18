using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

namespace GhostMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        internal static Plugin Instance { get; private set; }
        
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            // Initialize Harmony patches
            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
            Logger.LogInfo("Harmony patches applied");
            
            // Create the ghost racing manager
            GameObject manager = new GameObject("GhostRacingManager");
            manager.AddComponent<GhostRacingManager>();
            manager.AddComponent<GhostMod.UI.CoordinateDisplay>();
            DontDestroyOnLoad(manager);
            
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded - Press G to open menu");
        }
        
        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }

    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.stashya.ghostmod";
        public const string PLUGIN_NAME = "GhostMod";
        public const string PLUGIN_VERSION = "1.1.0";
    }
    
    /// <summary>
    /// Harmony patch to block game's cursor manager when ghost menu is open
    /// This is what WarmTofuMod does with On.SRCusorManager.Update hook
    /// </summary>
    [HarmonyPatch(typeof(SRCusorManager), "Update")]
    public class SRCusorManagerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // Check if ghost menu is open
            if (GhostRacingManager.Instance != null && 
                GhostRacingManager.Instance.CurrentState == GhostRacingManager.GhostRaceState.MenuOpen)
            {
                // Force cursor visible and unlocked
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                PlayerPrefs.SetInt("MenuOpen", 1);
                return false; // Skip original - don't let game touch cursor
            }
            return true; // Run original method normally
        }
    }
}