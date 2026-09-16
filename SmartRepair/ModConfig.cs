using BepInEx.Configuration;

namespace SmartRepair
{
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> AutoRepair;
        internal static ConfigEntry<bool> ShowMessage;
        internal static ConfigEntry<string> MessageText;

        internal static ConfigEntry<bool> CrossStationRepair;
        internal static ConfigEntry<string> StationOrder;
        internal static ConfigEntry<bool> RequireStationLevel;

        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile config)
        {
            AutoRepair = config.Bind(
                "Auto Repair", "Enabled", true,
                "Repair everything the station can handle the moment you start using it.");

            ShowMessage = config.Bind(
                "Auto Repair", "ShowMessage", true,
                "Show a centre-screen message saying how many items were repaired.");

            MessageText = config.Bind(
                "Auto Repair", "MessageText", "Repaired {0} item(s)",
                "Text for that message. {0} is the number of items repaired.");

            CrossStationRepair = config.Bind(
                "Cross-Station Repair", "Enabled", true,
                "Let a station repair anything an earlier station in the progression could repair. " +
                "A Black Forge then covers Forge and Workbench gear as well as its own. " +
                "This also applies to the manual repair button, not just the automatic pass.");

            StationOrder = config.Bind(
                "Cross-Station Repair", "StationOrder", StationLadder.DefaultOrder,
                "Crafting stations in progression order, earliest first, comma separated. " +
                "These are the station's internal names (the localisation token, with or without the $). " +
                "Stations left off this list keep vanilla behaviour, so add modded stations here to " +
                "slot them into the progression.");

            RequireStationLevel = config.Bind(
                "Cross-Station Repair", "RequireStationLevel", false,
                "Also require the station's upgrade level to meet the recipe's minimum when repairing " +
                "gear from an earlier station. Off by default: station levels are not comparable across " +
                "station types, so a level 1 Black Forge is further along than a level 4 Workbench.");

            VerboseLogging = config.Bind(
                "Advanced", "VerboseLogging", false,
                "Log a line per repaired item to the BepInEx console.");
        }
    }
}
