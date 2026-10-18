# AdventureBackpacks HD

Visual companion pack for **Adventure Backpacks** providing high-resolution 2K upscaled textures and normal maps for both Modern and Classic progression backpacks.

---

## Overview

Adventure Backpacks uses retro, pixel-dense textures by default to match Valheim's vanilla aesthetic. This companion pack supplies high-resolution 2K textures with normal maps for players using HD texture overhauls.

Both **Modern** and **Classic** model styles are fully supported with dedicated 2K asset bundles.

> **Notice: AI-Generated Textures & Reference Implementation**
>
> The high-definition textures in this companion pack were AI-created and upscaled. This mod is intended as an official reference example demonstrating how community artists can create, package, and distribute their own custom texture packs for Adventure Backpacks.
>
> If you are an artist interested in creating your own visual overhaul, see the [Creating Visual & Texture Packs Guide](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/CREATING_TEXTURE_PACKS.md). The companion mod requires fewer than 40 lines of boilerplate C# code.

### Modern Backpack Overrides

| Backpack | Biome | Asset Bundle | Default Texture Pack |
|:---|:---|:---|:---|
| Meadows Satchel | Meadows | `hd_backpack_meadows` | Native HD |
| Rugged Backpack | Black Forest | `hd_backpack_black_forest` | Native HD |
| Wetpack | Swamp | `hd_backpack_swamp` | Native HD |
| Sherpa Pack | Mountains | `hd_backpack_mountains` | Native HD |
| Lox Pack | Plains | `hd_backpack_plains` | Native HD |
| Wisp Pack | Mistlands | `hd_backpack_mistlands` | Native HD |
| Cinder Pack | Ashlands | `hd_backpack_ashlands` | Native HD |
| Frost Pack | Deep North | `hd_backpack_deepnorth` | Native HD |

### Classic Backpack Overrides

| Backpack | Biome | Asset Bundle | Default Texture Pack |
|:---|:---|:---|:---|
| Classic Meadows | Meadows | `hd_classic_backpack_meadows` | Native HD |
| Classic Black Forest | Black Forest | `hd_classic_backpack_black_forest` | Native HD |
| Classic Swamp | Swamp | `hd_classic_backpack_swamp` | Native HD |
| Classic Mountains | Mountains | `hd_classic_backpack_mountains` | Native HD |
| Classic Plains | Plains | `hd_classic_backpack_plains` | Native HD |
| Classic Mistlands | Mistlands | `hd_classic_backpack_mistlands` | Native HD |

---

## Requirements

- **BepInExPack Valheim** (v5.4.2351 or newer)
- **Adventure Backpacks** (v2.4.4 or newer)

---

## Configuration & Mix-and-Match

Adventure Backpacks exposes a **Texture Pack** setting for each individual backpack in the BepInEx Configuration Manager (under `[Backpack: <Name>]`):

| Setting | Default | Description |
|:---|:---|:---|
| `Texture Pack` | `Native HD` (when installed) | Selects the active visual pack for this specific backpack (`Native`, `Native HD`, or third-party packs). |
| `Model Style` | `Modern` | Toggles between `Modern` and `Classic` backpack geometries. |

Because `Native HD` provides assets for both `Modern` and `Classic` styles, you can switch model styles freely without losing high-definition textures. If you prefer low-res retro textures on specific bags while running HD on others, adjust the `Texture Pack` dropdown per backpack. A game restart is required when changing visual bundles.

---

## For Artists & Mod Creators

This mod serves as an open-source reference for the Adventure Backpacks Visual Pack API. The entire companion plugin is under 40 lines of code and requires zero hard dependencies on the core mod assembly.

If you are an artist or texture creator interested in releasing your own visual packs:
- **Full Guide**: [Creating Visual & Texture Packs Guide](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/CREATING_TEXTURE_PACKS.md)
- **Source Code**: [AdventureBackpacks.HD Source](https://github.com/Vapok/AdventureBackpacks/tree/main/AdventureBackpacks.HD)
- **Client Wrapper**: [ABAPI_Client.cs](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs) (drop-in zero-dependency reflection client)

