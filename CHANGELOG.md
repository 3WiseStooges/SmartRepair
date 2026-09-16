# Changelog

## 1.0.1

- New icon.
- Trimmed the readme.

## 1.0.0

First release.

- **Repairs everything on station use.** Postfix on `CraftingStation.Interact`, which is where
  the game sets the current station and opens the crafting panel, so the station is live by the
  time the pass runs. One message with a count instead of vanilla's one message per item.
- **Later stations repair earlier stations' gear.** Workbench → Forge → Artisan Table →
  Black Forge → Galdr Table, configurable. Implemented as a postfix on `InventoryGui.CanRepair`
  so the repair button and its glow agree with the automatic pass, and so the widening also
  applies to manual repair.
- Station-level requirements are skipped for cross-station repairs, since levels do not compare
  across station types. `RequireStationLevel` turns that back on.
- Recipe lookups are cached per item name. `ObjectDB.GetRecipe` walks the whole recipe list
  comparing display names and the repair UI asks once per worn item per frame, so widening the
  rules would otherwise double an already hot scan. The cache expires when `ObjectDB` is
  replaced or grows, which covers mods appending recipes.
- Falls back to SmartRepair's own copy of the vanilla repair rules if a future update renames
  `InventoryGui.CanRepair`, and logs a warning when it does. `CanRepair`, `HaveRepairableItems`
  and `RepairOneItem` are all private, so nothing here calls them by hand.
