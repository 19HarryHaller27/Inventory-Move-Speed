# Inventory move speed

| | |
|-|-|
| **Mod id** | `inventorymovespeed` |
| **Version** | 0.1.0 |
| **Game** | Vintage Story 1.22.0+ |
| **.NET** | 10+ (`net10.0`) |

**Backpack** storage fullness changes **move speed** around a half-empty neutral point (roughly +25% max / −25% max; see `modinfo.json` and `MOD_PAGE.md` for the exact curve). **Server-only**; **character** screen shows the stat.

## Build

- **`Directory.Build.props`** in this directory sets `VintageStoryPath` (or override on the command line / env).
- `dotnet build InventoryMoveSpeed.csproj -c Release` or your usual script.

**Deploy** copies the DLL, `modinfo`, and `assets` to the project root and (Windows) to `%APPDATA%\Roaming\VintagestoryData\Mods\InventoryMoveSpeed\` unless `InventoryMoveSpeedNoDeploy=true` (e.g. CI).

## Layout

- `src/`, `assets/`, `modinfo.json`, `MOD_PAGE.md` (design notes; keep with the code)

## License

[MIT](LICENSE)

**Author:** adams.
