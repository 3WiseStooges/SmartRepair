# SmartRepair

Quick and easy auto-repair for Valheim. Your newest crafting station repairs everything
from the older ones too.

Valheim repairs one item per button press, at the one station that made it. SmartRepair does
the whole job the moment you walk up to a station, and lets your newest station cover
everything the older ones could.

Built fresh against the current game, for the Unity 6 update.

Client-side only. Durability lives on your own character, so nothing here talks to the server
and nobody else in the world needs the mod.

## What it does

**Repairs everything when you open a station.** Interact with a workbench, forge, black forge
or galdr table and every repairable item in your inventory is fixed before the panel finishes
opening. One message tells you how many. Crafting skill goes up exactly as it would have if
you had pressed the button yourself, once per item.

**Later stations cover earlier ones.** Vanilla ties each item to the station that crafted it,
so a stone axe needs the workbench even when you are standing at a black forge. SmartRepair
orders the stations by progression instead:

| | Station | Repairs |
| :---: | --- | --- |
| 1 | Workbench | Workbench gear |
| 2 | Forge | Forge, Workbench |
| 3 | Artisan Table | Artisan, Forge, Workbench |
| 4 | Black Forge | Black Forge, Artisan, Forge, Workbench |
| 5 | Galdr Table | Everything |

This applies to the repair button too, not just the automatic pass — the button lights up for
anything the station now covers, so manual repair works the same way if you turn the automatic
pass off.

## Install

Through r2modman or Thunderstore Mod Manager, or drop `SmartRepair.dll` into
`BepInEx/plugins`.

Uninstall any other auto-repair mod first. Two of them patching the same repair path will
fight each other.

## Config

`BepInEx/config/com.ljindustries.valheim.smartrepair.cfg`, written on first launch.

| Setting | Default | What it does |
| --- | --- | --- |
| `Auto Repair / Enabled` | `true` | Repair on station use. Off leaves only the widened repair button. |
| `Auto Repair / ShowMessage` | `true` | Centre-screen message with the count |
| `Auto Repair / MessageText` | `Repaired {0} item(s)` | Text for that message; `{0}` is the count |
| `Cross-Station Repair / Enabled` | `true` | Later stations cover earlier ones |
| `Cross-Station Repair / StationOrder` | the five vanilla stations | Progression order, earliest first |
| `Cross-Station Repair / RequireStationLevel` | `false` | See below |
| `Advanced / VerboseLogging` | `false` | One log line per repaired item |

### StationOrder

Internal station names, comma separated, earliest first. The `$` is optional:

```
$piece_workbench, $piece_forge, $piece_artisanstation, $piece_blackforge, $piece_magetable
```

Anything not on this list keeps vanilla behaviour, so a modded station is unaffected until you
add it. To slot one in, put its name where it belongs in the progression — the order of the
list is the only thing that matters, not the position numbers.

### RequireStationLevel

Vanilla will not repair an item if the station's upgrade level is below the recipe's minimum.
SmartRepair skips that check when the station is further along the progression than the recipe
asked for, because station levels are not comparable across station types: a level 1 black
forge is further along than a level 4 workbench. Set this to `true` to enforce the level
requirement anyway. Repairs at the item's own station always use vanilla's rule either way.

## Notes

- Only items already in your inventory are repaired — the same set the repair button works on.
- Stations that cannot repair at all in vanilla still cannot. If there is no repair button,
  SmartRepair does not invent one.
- Roof and fire requirements still apply. Pressing use on a station that fails those checks
  repairs nothing, same as vanilla.
- Repair stays free, as it is in vanilla.
