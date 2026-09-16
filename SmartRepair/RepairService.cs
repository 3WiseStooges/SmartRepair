using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SmartRepair
{
    /// <summary>
    /// The automatic pass: what pressing the repair button until it greys out would do,
    /// minus the one message per item.
    /// </summary>
    internal static class RepairService
    {
        private static readonly List<ItemDrop.ItemData> WornBuffer = new List<ItemDrop.ItemData>();

        private static Func<InventoryGui, ItemDrop.ItemData, bool> _vanillaCanRepair;
        private static bool _canRepairResolved;

        internal static void RepairAllAt(CraftingStation station)
        {
            if (!ModConfig.AutoRepair.Value) return;

            Player player = Player.m_localPlayer;
            if (player == null || station == null) return;

            // Interact only sets the station once CheckUsable has passed, so this also covers
            // "you pressed E but the roof or fire check failed".
            if (player.GetCurrentCraftingStation() != station) return;

            // No repair button on this station means there is nothing to press.
            if (!station.m_canRepair) return;
            if (!station.CheckUsable(player, false)) return;

            Inventory inventory = player.GetInventory();
            if (inventory == null) return;

            int repaired = 0;
            WornBuffer.Clear();
            try
            {
                inventory.GetWornItems(WornBuffer);

                foreach (ItemDrop.ItemData item in WornBuffer)
                {
                    if (item == null || item.m_shared == null) continue;
                    if (!CanRepair(item)) continue;

                    float max = item.GetMaxDurability();
                    if (max <= 0f || item.m_durability >= max) continue;

                    player.RaiseSkill(Skills.SkillType.Crafting, 1f - item.m_durability / max);
                    item.m_durability = max;
                    repaired++;

                    if (ModConfig.VerboseLogging.Value)
                        Plugin.Log.LogInfo($"Repaired {item.m_shared.m_name} at {station.m_name}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Auto repair failed: {ex}");
            }
            finally
            {
                WornBuffer.Clear();
            }

            if (repaired == 0) return;

            if (station.m_repairItemDoneEffects != null)
                station.m_repairItemDoneEffects.Create(station.transform.position, Quaternion.identity);

            if (ModConfig.ShowMessage.Value)
            {
                string text = ModConfig.MessageText.Value;
                if (string.IsNullOrEmpty(text)) text = "Repaired {0} item(s)";
                player.Message(MessageHud.MessageType.Center, SafeFormat(text, repaired));
            }
        }

        private static string SafeFormat(string format, int count)
        {
            try
            {
                return string.Format(format, count);
            }
            catch (FormatException)
            {
                // A hand-edited MessageText with a stray brace should not cost you the repair.
                return format;
            }
        }

        /// <summary>
        /// Asks the game's own gate, so the automatic pass agrees with the repair button
        /// exactly - including SmartRepair's own widening and any other mod's patch of it.
        /// </summary>
        private static bool CanRepair(ItemDrop.ItemData item)
        {
            Func<InventoryGui, ItemDrop.ItemData, bool> vanilla = ResolveVanillaCanRepair();
            InventoryGui gui = InventoryGui.instance;

            if (vanilla != null && gui != null) return vanilla(gui, item);
            return FallbackCanRepair(item);
        }

        private static Func<InventoryGui, ItemDrop.ItemData, bool> ResolveVanillaCanRepair()
        {
            if (_canRepairResolved) return _vanillaCanRepair;
            _canRepairResolved = true;

            MethodInfo method = AccessTools.Method(
                typeof(InventoryGui), "CanRepair", new[] { typeof(ItemDrop.ItemData) });

            if (method == null)
            {
                Plugin.Log.LogWarning(
                    "InventoryGui.CanRepair not found - falling back to SmartRepair's own repair rules. " +
                    "A Valheim update most likely renamed it; please report this.");
                return null;
            }

            try
            {
                // CanRepair is private, so bind it through Harmony's generated method rather than
                // Delegate.CreateDelegate, which applies visibility checks. Either way Harmony
                // detours the method body, so this still runs whatever patches are installed.
                _vanillaCanRepair = AccessTools.MethodDelegate<Func<InventoryGui, ItemDrop.ItemData, bool>>(
                    method, null, false);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(
                    $"Binding InventoryGui.CanRepair as a delegate failed ({ex.GetType().Name}: " +
                    $"{ex.Message}); using plain reflection instead.");
                _vanillaCanRepair = (gui, item) => (bool)method.Invoke(gui, new object[] { item });
            }

            return _vanillaCanRepair;
        }

        /// <summary>
        /// Mirrors InventoryGui.CanRepair, for the day the game renames it. Kept deliberately
        /// close to the original; the only addition is the cross-station rule.
        /// </summary>
        private static bool FallbackCanRepair(ItemDrop.ItemData item)
        {
            Player player = Player.m_localPlayer;
            if (player == null || item == null || item.m_shared == null) return false;
            if (!item.m_shared.m_canBeReparied) return false;
            if (player.NoCostCheat()) return true;

            CraftingStation station = player.GetCurrentCraftingStation();
            if (station == null) return false;

            Recipe recipe = RecipeCache.GetRecipe(item);
            if (recipe == null) return false;
            if (recipe.m_craftingStation == null && recipe.m_repairStation == null) return false;

            bool stationAccepted =
                (recipe.m_repairStation != null && recipe.m_repairStation.m_name == station.m_name) ||
                (recipe.m_craftingStation != null && recipe.m_craftingStation.m_name == station.m_name) ||
                item.m_worldLevel < Game.m_worldLevel;

            if (stationAccepted)
                return Mathf.Min(station.GetLevel(), 4) >= recipe.m_minStationLevel;

            return StationLadder.AllowsCrossTierRepair(station, item);
        }
    }
}
