using HarmonyLib;

namespace SmartRepair.Patches
{
    /// <summary>
    /// Interact is where the game sets the current station and opens the crafting panel, so by
    /// the time this postfix runs the station is live and the panel is up - exactly the moment
    /// the player starts using a crafting station.
    /// </summary>
    [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.Interact))]
    internal static class CraftingStationInteractPatch
    {
        private static void Postfix(CraftingStation __instance, Humanoid user, bool repeat)
        {
            if (repeat) return;

            Player player = Player.m_localPlayer;
            if (player == null || user != player) return;

            RepairService.RepairAllAt(__instance);
        }
    }
}
