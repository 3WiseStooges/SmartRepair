using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SmartRepair
{
    /// <summary>
    /// Client-side only. Durability lives on the local player's inventory, so nothing here
    /// needs to reach the server and the mod does not have to match between players.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.ljindustries.valheim.smartrepair";
        public const string ModName = "SmartRepair";
        public const string ModVersion = "1.0.1";

        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            ModConfig.Bind(Config);

            // Patch per type so one method the game has renamed cannot abort every other patch.
            _harmony = new Harmony(ModGuid);
            foreach (var type in typeof(Plugin).Assembly.GetTypes())
            {
                try
                {
                    _harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Harmony skip {type.FullName}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
