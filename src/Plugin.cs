using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace GhostMod
{
    /// <summary>
    /// GhostMod - Ghost Racing for Initial Drift Online
    /// Race against your own best times or challenge other players' ghosts
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        internal static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            // Create the ghost racing manager
            GameObject manager = new GameObject("GhostRacingManager");
            manager.AddComponent<GhostRacingManager>();
            DontDestroyOnLoad(manager);
            
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded - Press G to open menu");
        }
    }

    /// <summary>
    /// Plugin metadata
    /// </summary>
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.stashya.ghostmod";
        public const string PLUGIN_NAME = "GhostMod";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}
