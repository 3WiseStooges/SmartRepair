# Development

## Build

```bash
dotnet build SmartRepair/SmartRepair.csproj -c Release
```

Output goes to `package/SmartRepair.dll`, which is committed — the publish workflow ships that
file rather than building on CI.

The project finds Valheim at a standard Windows Steam layout. Override it:

```bash
dotnet build SmartRepair/SmartRepair.csproj -c Release -p:ValheimFolder="D:\Steam\steamapps\common\Valheim"
```

The build fails with a clear message if the game's assemblies are not where it looked.

BepInEx and HarmonyX come from NuGet (`BepInEx.Core`, `HarmonyX`), not from an r2modman
profile, so the build does not care which profiles exist. `BepInEx.Core` is only on BepInEx's
own feed — that is what `NuGet.config` is for.

## Where the game code lives

Valheim's own classes are in **`valheim_Data/Managed/assembly_valheim.dll`**, not
`Assembly-CSharp.dll`. Decompiling the wrong one turns up nothing:

```bash
ilspycmd -t InventoryGui "X:/SteamLibrary/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll"
```

## The two things this mod touches

`InventoryGui.CanRepair(ItemDrop.ItemData)` is the single gate for whether an item may be
repaired at the current station. It is **private**. Vanilla compares
`recipe.m_repairStation.m_name` and `recipe.m_craftingStation.m_name` against
`currentCraftingStation.m_name` — string equality, which is why a black forge refuses a stone
axe. `HaveRepairableItems`, `RepairOneItem` and the button's glow all route through it, so a
postfix there is enough to widen everything at once.

`CraftingStation.Interact` calls `SetCraftingStation(this)` and then
`InventoryGui.instance.Show(null, 3)`. A postfix runs after both, so the station and panel are
already live.

`RepairService` calls `CanRepair` through an open-instance delegate rather than reimplementing
it, so the automatic pass and the button can never disagree and other mods' patches of
`CanRepair` still apply. `FallbackCanRepair` is the escape hatch for a future rename and should
be kept in step with vanilla if that method changes.

## Testing

Install through r2modman so you exercise the real load order rather than hand-copying into a
profile.

Worth checking after any change:

- Stone axe at a forge and at a black forge (cross-station path)
- An item at its own station with the station under-levelled (must still refuse)
- A station with no roof or no fire (must repair nothing)
- `Cross-Station Repair / Enabled = false` (must behave exactly like vanilla plus auto-repair)

Set `Advanced / VerboseLogging = true` for a line per repaired item.

## Publish to Thunderstore

Pushes to `main` publish automatically. The workflow bumps the patch version if Thunderstore
already has the repo version.

Also available as **Actions → Publish to Thunderstore → Run workflow**, or tag `v1.x.y` and
push.

1. `thunderstore.toml` `namespace` must match the Thunderstore team (`LJIndustries`)
2. Repo secret: `THUNDERSTORE_API_KEY`
3. Commit the rebuilt `package/SmartRepair.dll`
