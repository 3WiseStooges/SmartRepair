using HarmonyLib;

namespace SmartRepair.Patches
{
    /// <summary>
    /// Vanilla allows a repair only when the item's recipe names the station you are standing
    /// at. Widening that gate here, rather than inside the repair loop, keeps the manual repair
    /// button, its glow, and SmartRepair's automatic pass in agreement.
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), "CanRepair")]
    internal static class InventoryGuiCanRepairPatch
    {
        private static void Postfix(ItemDrop.ItemData item, ref bool __result)
        {
            if (__result) return;

            Player player = Player.m_localPlayer;
            if (player == null) return;

            CraftingStation station = player.GetCurrentCraftingStation();
            if (station == null) return;

            __result = StationLadder.AllowsCrossTierRepair(station, item);
        }
    }
}
