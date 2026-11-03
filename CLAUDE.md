# Notes for Claude

Unity 2022.3.17f1 project, 2D racing game for Android. See README.md for layout.

## Working without the Unity Editor (cloud sessions)

- There is no Unity Editor in cloud sessions, so code can't be compiled or run there. CI
  (`.github/workflows/unity-build.yml`, GameCI Android build) is the compile check. Push a branch and read
  the workflow result.
- The network allows `packages.unity.com`, `download.packages.unity.com` and `cdn.packages.unity.com`, so
  package sources can be fetched to check APIs:
  `curl -s https://packages.unity.com/<pkg>` → `versions[<v>].dist.tarball` → download and untar into the scratchpad.
- `UnityEngine`/`UnityEditor` DLLs are not available. For a quick C# syntax check, use tree-sitter
  (`pip install tree-sitter tree-sitter-c-sharp` in a venv).

## Editing scenes/prefabs as text

- Scenes and prefabs are text YAML. Each object is a `--- !u!<classID> &<fileID>` document.
  Scripts are referenced by the GUID in their `.cs.meta`. Never change a script's `.meta` GUID, or every
  scene that uses it gets a "missing script".
- New scripts need a `.cs.meta` with a fresh random 32-hex GUID; new scenes need a `.unity.meta`.
- When removing a component, delete its document and its `- component: {fileID: …}` line on the
  GameObject, then check nothing else references that fileID.
- New scenes must also be added to `ProjectSettings/EditorBuildSettings.asset`.
- Keep each file's existing line endings and BOM (some `.cs` files are CRLF with BOM).

## Game conventions

- Saves are PlayerPrefs; all keys live in `Scripts/PrefsKeys.cs`. "Owned" is stored as the int `3`
  (`PlayerPrefsKeys.OwnedValue`).
- Levels load by name `"Level" + SelectedLevelIndex`. The active level menu is `ItemSelect` on
  `MainMenu → LevelsMenu` (`LevelSelect` on the same object is disabled). Both have `levelIcons` and price arrays.
- The car is spawned by `StartPoint` and found by tag `Player`. Ground must be tagged `Ground`, layer 10
  (`DeadTrigger` uses the tag). Coins and fuel are `ScoreItem` triggers hit by the car's `CoinTrigger` collider.
- `GameManager` drives fuel, coins, distance awards and win/lose. `endlessMode` switches it to endless rules.

## Known issues / decisions pending

- `MainMenu → Garage` has a missing script (old `Upgrade` component) whose 17 upgrade buttons do nothing.
  `EngineShop` replaced it. Decide whether to delete that old panel.
- `Prefabs/Item/Grass.prefab` has a missing sprite (it isn't used anywhere).
- The `UnityAds` scene's serialized Android Game ID (`1230822`) may be the original template author's.
  Your own IDs are in comments in `UnityAdsIntegration.cs`.
- The repo is public but contains paid Asset Store content (2D Terrain Editor and possibly the racing template).
