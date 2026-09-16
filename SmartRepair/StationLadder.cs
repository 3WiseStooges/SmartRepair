using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartRepair
{
    /// <summary>
    /// Orders crafting stations by progression, so a station further along covers everything
    /// an earlier one could repair. Vanilla only ever compares the current station's name
    /// against the one named on the recipe, which is why a Black Forge refuses a stone axe.
    /// </summary>
    internal static class StationLadder
    {
        internal const string DefaultOrder =
            "$piece_workbench, $piece_forge, $piece_artisanstation, $piece_blackforge, $piece_magetable";

        private static readonly Dictionary<string, int> Tiers =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static string _parsedFrom;

        /// <summary>
        /// True when <paramref name="station"/> sits later in the progression than the earliest
        /// station the item's recipe accepts. Same-tier and lower-tier cases are left alone:
        /// vanilla has already ruled on those, including its station-level requirement.
        /// </summary>
        internal static bool AllowsCrossTierRepair(CraftingStation station, ItemDrop.ItemData item)
        {
            if (!ModConfig.CrossStationRepair.Value) return false;
            if (station == null || item == null || item.m_shared == null) return false;
            if (!item.m_shared.m_canBeReparied) return false;

            int stationTier = TierOf(station.m_name);
            if (stationTier < 0) return false;

            Recipe recipe = RecipeCache.GetRecipe(item);
            if (recipe == null) return false;

            int requiredTier = RequiredTier(recipe);
            if (requiredTier < 0) return false;
            if (stationTier <= requiredTier) return false;

            if (ModConfig.RequireStationLevel.Value &&
                Mathf.Min(station.GetLevel(), 4) < recipe.m_minStationLevel)
                return false;

            return true;
        }

        /// <summary>
        /// The earliest ladder position that satisfies the recipe. Vanilla accepts either the
        /// repair station or the crafting station, so the lower of the two is what has to be
        /// beaten. Stations outside the ladder are ignored; if neither is on it, returns -1.
        /// </summary>
        private static int RequiredTier(Recipe recipe)
        {
            int best = Lowest(-1, recipe.m_repairStation);
            best = Lowest(best, recipe.m_craftingStation);
            return best;
        }

        private static int Lowest(int best, CraftingStation station)
        {
            if (station == null) return best;
            int tier = TierOf(station.m_name);
            if (tier < 0) return best;
            return best < 0 || tier < best ? tier : best;
        }

        internal static int TierOf(string stationName)
        {
            if (string.IsNullOrEmpty(stationName)) return -1;
            EnsureParsed();
            return Tiers.TryGetValue(Normalize(stationName), out int tier) ? tier : -1;
        }

        private static void EnsureParsed()
        {
            string raw = ModConfig.StationOrder == null ? DefaultOrder : ModConfig.StationOrder.Value;
            if (raw == null) raw = DefaultOrder;
            if (raw == _parsedFrom) return;

            Tiers.Clear();
            int next = 0;
            foreach (string part in raw.Split(','))
            {
                string name = Normalize(part);
                if (name.Length == 0 || Tiers.ContainsKey(name)) continue;
                Tiers[name] = next++;
            }

            _parsedFrom = raw;
        }

        private static string Normalize(string name)
        {
            return name.Trim().TrimStart('$').Trim();
        }
    }
}
