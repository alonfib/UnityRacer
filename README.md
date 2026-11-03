# UnityRacer

A 2D hill-climb style racing game for Android, built with Unity.

- **Unity version:** 2022.3.17f1 (see `ProjectSettings/ProjectVersion.txt`). Install exactly this version from Unity Hub.
- **Platform:** Android (`com.ALIyerEdon.RacingKit`), mobile UI with on-screen throttle and brake.

## Getting started

1. Clone the repo and open the folder in Unity Hub with **2022.3.17f1**.
2. Unity downloads the packages in `Packages/manifest.json` on first open. This takes a few minutes.
3. Open `Assets/2D Racing Game/Scenes/MainMenu.unity` and press Play.

## Project layout

| Path | What's there |
|---|---|
| `Assets/2D Racing Game/Scenes/` | `MainMenu`, `Level0`…`Level6` (fixed tracks), `Level7` (endless mode), `UnityAds` |
| `Assets/2D Racing Game/Scripts/Gameplay/` | Car physics (`CarController`), HUD, fuel, coins and distance awards (`GameManager`), endless terrain (`EndlessTerrain`) |
| `Assets/2D Racing Game/Scripts/Car/` | Car prefabs, owned cars and upgrades (`Car`, `CarsManager`) |
| `Assets/2D Racing Game/Scripts/Shop/` | Garage shops: engine, body, wheels, colors |
| `Assets/2D Racing Game/Scripts/Menu/` | Main menu, level select (`ItemSelect`), settings, ads |
| `Assets/2D Racing Game/Scripts/PrefsKeys.cs` | Every PlayerPrefs key used for saves |
| `Assets/2D Terrain Editor/` | Third-party 2D terrain mesh tool (Asset Store) used by endless mode |

Levels are loaded by name: `"Level" + PlayerPrefs[SelectedLevelIndex]`. To add a level, add the scene to
*File → Build Settings* and add an icon + price to the `ItemSelect` component on `MainMenu → LevelsMenu`.

## Endless mode (`Level7`)

`EndlessTerrain` builds ground in 80 m chunks ahead of the car using the 2D Terrain Editor runtime, places
coins and fuel, and deletes chunks far behind. Hills get taller and fuel gets rarer with distance.
`GameManager.endlessMode` turns off the finish line, gives coins every 500 m, and saves the best distance
(`EndlessBestDistance`). Tune it from the inspector on the `EndlessTerrain` and `GameManager` objects.

## CI

`.github/workflows/unity-build.yml` builds the Android APK with [GameCI](https://game-ci.com) on every push
and pull request. It needs three repository secrets (*Settings → Secrets and variables → Actions*):

- `UNITY_EMAIL`, `UNITY_PASSWORD`: your Unity account
- `UNITY_LICENSE`: contents of your `.ulf` licence file ([how to get it](https://game-ci.com/docs/github/activation))

The built APK is attached to each run as an artifact.

## Merging scenes and prefabs

`.gitattributes` routes Unity YAML files to Unity's Smart Merge. Enable it once per clone:

```sh
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'<Unity install>/Editor/Data/Tools/UnityYAMLMerge' merge -p %O %B %A %A"
```

Without this, git falls back to a normal text merge.
