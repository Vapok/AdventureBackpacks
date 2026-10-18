# Creating Visual & Texture Packs for Adventure Backpacks

Adventure Backpacks includes a decoupled visual pack registration API. Companion mods and community texture packs can override backpack model prefabs, textures, and materials without needing direct assembly dependencies or Harmony patches.

Players can select their preferred texture pack on a per-backpack basis directly in the in-game Configuration Manager, allowing them to mix and match packs or use packs that only overhaul a single backpack.

An official reference implementation is available in the repository:
[`AdventureBackpacks.HD`](https://github.com/Vapok/AdventureBackpacks/tree/main/AdventureBackpacks.HD)

---

## Architecture Overview

- **Zero Direct Assembly Dependencies**: Companion mods do not need to reference `AdventureBackpacks.dll`. Simply drop the standalone [`ABAPI_Client.cs`](ABAPI_Client.cs) file into your project. It communicates with Adventure Backpacks via reflection.
- **Per-Backpack & Model Style Scoping**: You can provide overrides for Modern models, Classic models, or both. Overrides are registered against a clean backpack identifier and a `ModelStyle` enum (`Modern` or `Classic`).
- **Dynamic Configuration Injection**: When your mod registers an override under a pack name (e.g. `"Willybach HD"` or `"Native HD"`), that pack name automatically appears in the player's in-game config dropdown for that backpack.
- **Fail-Safe Fallback**: If your pack is uninstalled or does not provide an override for a specific backpack/style combination, the game automatically falls back to `Native` assets.

---

## Backpack Identifiers & Model Styles

When registering an AssetBundle override, you specify the backpack ID and the target `ModelStyle`:

| Backpack Name | Backpack Identifier | Supports Modern | Supports Classic |
| :--- | :--- | :---: | :---: |
| **Meadows Backpack** | `backpack_meadows` | Yes | Yes |
| **Black Forest Backpack** | `backpack_black_forest` | Yes | Yes |
| **Swamp Backpack** | `backpack_swamp` | Yes | Yes |
| **Mountains Backpack** | `backpack_mountains` | Yes | Yes |
| **Plains Backpack** | `backpack_plains` | Yes | Yes |
| **Mistlands Backpack** | `backpack_mistlands` | Yes | Yes |
| **Ashlands Backpack** | `backpack_ashlands` | Yes | No |
| **Deep North Backpack** | `backpack_deepnorth` | Yes | No |

### Model Style Options
- `ABAPIClient.ModelStyle.Modern`: Overrides the detailed modern model geometry and textures.
- `ABAPIClient.ModelStyle.Classic`: Overrides the original classic backpack model geometry and textures.

If your pack only provides Modern textures, you only register `ModelStyle.Modern`. Your pack will then only appear as an option in the player's config when they have "Modern" selected for that backpack.

---

## The Code (Under 40 Lines)

Creating a companion pack requires almost no code. 

### 1. Add `ABAPI_Client.cs` to Your Project
Copy [`Docs/ABAPI_Client.cs`](ABAPI_Client.cs) into your mod project.

### 2. Implement Your Plugin
Here is a complete, production-ready plugin class:

```csharp
using System;
using System.IO;
using AdventureBackpacks.API.Client;
using BepInEx;
using UnityEngine;

namespace MyBackpackTexturePack;

[BepInPlugin("author.mybackpacktexturepack", "My Backpack Texture Pack", "1.0.0")]
[BepInDependency("vapok.mods.adventurebackpacks", BepInDependency.DependencyFlags.HardDependency)]
public class MyTexturePackPlugin : BaseUnityPlugin
{
    public const string PackName = "My Custom Pack";

    private void Awake()
    {
        string assetsDirectory = Path.Combine(Path.GetDirectoryName(Info.Location) ?? string.Empty, "Assets");

        // Register any bundles you provide using strongly-typed BackpackName:
        RegisterBundle(ABAPIClient.BackpackName.Meadows, ABAPIClient.ModelStyle.Modern, "my_meadows_bundle", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.BlackForest, ABAPIClient.ModelStyle.Modern, "my_blackforest_bundle", assetsDirectory);
        
        // If you also support Classic models:
        RegisterBundle(ABAPIClient.BackpackName.Meadows, ABAPIClient.ModelStyle.Classic, "my_classic_meadows_bundle", assetsDirectory);
    }

    private void RegisterBundle(ABAPIClient.BackpackName backpack, ABAPIClient.ModelStyle modelStyle, string bundleFileName, string assetsDirectory)
    {
        string bundlePath = Path.Combine(assetsDirectory, bundleFileName);
        if (!File.Exists(bundlePath))
            return;

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle != null)
            {
                // Set isDefault to true if you want your pack selected by default when first installed
                ABAPIClient.RegisterVisualBundle(PackName, backpackId, modelStyle, bundle, isDefault: true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to load bundle '{bundleFileName}': {ex.Message}");
        }
    }
}
```

---

## Building Your AssetBundles

1. **Prefab Names**:
   Inside your AssetBundle, the prefab name must match what Adventure Backpacks expects:
   - Meadows: `BackpackMeadows`
   - Black Forest: `BackpackBlackForest`
   - Swamp: `BackpackSwamp`
   - Mountains: `BackpackMountains`
   - Plains: `BackpackPlains`
   - Mistlands: `BackpackMistlands`
   - Ashlands: `BackpackAshlands`
   - Deep North: `BackpackDeepNorth`

2. **Unity Engine Target**:
   Valheim runs on **Unity 6** (`v6000.0.75f1`). Ensure your AssetBundles are compiled for `StandaloneWindows64`.

3. **Shader Compatibility**:
   Ensure your materials use Valheim-compatible shaders (such as standard double-sided shaders or standard Lit shaders) so textures render cleanly under in-game lighting.

---

## In-Game Configuration & Hot-Swapping

- When your plugin loads in `Awake()`, it registers your bundles with `ABAPI`.
- When Adventure Backpacks initializes its assets, it reads the active config:
  - If a player selects `"My Custom Pack"` in the in-game Configuration Manager under `Backpack: <Backpack Name>` -> `Texture Pack`, that backpack will load your AssetBundle.
  - Toggling texture packs in-game saves the configuration immediately, but because Valheim registers item prefabs into `ObjectDB` at startup, a game restart is required for the visual changes to take effect.
