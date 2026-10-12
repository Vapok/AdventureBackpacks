# Adventure Backpacks API Integration Guide

This guide details how external mod developers and AI coding agents can safely integrate with Adventure Backpacks.

---

## ⚠️ Non-Negotiable Invariants & Ground Rules

1. **Strictly 1 Backpack Equipped Invariant**:
   - Only **one** backpack may ever be equipped at any time per player.
   - **Do NOT** attempt to bypass `ABAPI.IsBackpackEquipped()`, manipulate item equip slots to allow multiple backpacks, or create pseudo-backpack storage. Multi-backpack equipping compromises game balance, world save persistence, and triggers inventory desynchronization.

2. **No Backpacks in Backpacks Invariant (Thor's Inventory Guard / Anti-Inception)**:
   - Backpacks **cannot** be placed, stored, or auto-routed inside other backpacks under any circumstances.
   - `Inventory.CanAddItem` rejects adding any item where `item.IsBackpack()` into a backpack container. Never attempt to bypass this. Nested backpacks cause recursive serialization crashes, save-file bloating, and duplication exploits.

3. **Item Protection & Inventory Failsafes (`RemoveItem` & `PerformYardSale`)**:
   - **Comprehensive `RemoveItem` Monitoring**: Adventure Backpacks actively intercepts all overloads of `Inventory.RemoveItem` (`RemoveItem(ItemData)`, `RemoveItem(ItemData, int)`, and `RemoveOneItem(ItemData)`) across crafting consumption (`CraftingContext`), container transfers (`_movingItemBetweenContainers`), world drops (`_droppingOutside`), quick-dropping, and player death to safeguard worn gear and backpack contents.
   - **Sanctity of `PerformYardSale` (NEVER Reflect or Bypass)**:
     - When an active backpack containing items is unequipped, removed, downgraded, or detached, Adventure Backpacks executes `PerformYardSale` to eject stored items onto the ground at the player's feet rather than deleting them from memory.
     - **NEVER** use reflection or Harmony patches to modify, intercept, suppress, or replace `PerformYardSale`. Doing so circumvents critical anti-duplication locks and directly leads to permanent player item deletion or infinite duplication glitches.

4. **Strict Prohibition on Reflection & Internal Harmony Patches**:
   - **DO NOT** use reflection (`AccessTools`, `Traverse`, `Type.GetType`, `GetField`, `GetMethod`) to access private fields or internal methods in `AdventureBackpacks`.
   - **DO NOT** create Harmony patches targeting internal classes (such as `BackpackManager`, `InventoryGuiPatches`, `ItemDropPatches`, or `EquipmentPatches`).
   - Internal classes change across releases without warning. The public API surface in `ABAPI` is the only supported and stable interface.

---

## Integration Workflows

Two integration workflows are supported:
1. **Option A: Repackable API Assembly ([`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases))** — Compile against the standalone API assembly and merge it using `ILRepack`.
2. **Option B: Zero-Dependency Client Wrapper ([`Docs/ABAPI_Client.cs`](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs))** — Drop a single standalone C# file into your project with zero external DLL references.

For complete class, struct, and method signatures, consult [AdventureBackpacksAPI.md](AdventureBackpacksAPI.md).

---

## Option A: Repackable API Assembly ([`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases))

This option is recommended if your mod already uses `ILRepack` to bundle dependencies and you prefer compile-time type safety directly against the API types.

### 1. Download the API Assembly
Download `AdventureBackpacksAPI-Vapok-<Version>.zip` from [GitHub Releases](https://github.com/Vapok/AdventureBackpacks/releases). Extract [`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases).

### 2. Add Project Reference
Reference [`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases) in your `.csproj`:

```xml
<ItemGroup>
    <Reference Include="AdventureBackpacksAPI">
        <HintPath>path\to\AdventureBackpacksAPI.dll</HintPath>
        <Private>False</Private>
    </Reference>
</ItemGroup>
```

### 3. Configure ILRepack
Add [`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases) to your `ILRepack` inputs. Adventure Backpacks dynamically intercepts assembly loading and redirects all API references to the live `AdventureBackpacks.dll` assembly at runtime.

When Adventure Backpacks is not installed, the repacked stub methods execute safely and return default values (`false`, `null`, empty collections) without causing runtime exceptions.

### 4. Implementation Example

```csharp
using System.Collections.Generic;
using AdventureBackpacks.API;
using UnityEngine;

public class BackpackAuditIntegration
{
    public void CheckPlayerBackpacks(Player player)
    {
        if (!ABAPI.IsLoaded())
        {
            return;
        }

        if (ABAPI.IsBackpackEquipped(player))
        {
            Inventory equippedInv = ABAPI.GetEquippedBackpackInventory(player);
            if (equippedInv != null)
            {
                Debug.Log($"Equipped backpack contains {equippedInv.GetAllItems().Count} items.");
            }
        }

        List<Inventory> allPacks = ABAPI.GetAllBackpackInventories(player);
        Debug.Log($"Player has {allPacks.Count} total backpack inventories accessible.");
    }
}
```

---

## Option B: Zero-Dependency Client Wrapper ([`Docs/ABAPI_Client.cs`](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs))

This option is recommended for mods that integrate with Adventure Backpacks as an optional soft dependency and want to avoid adding extra assembly references or configuring ILRepack.

### 1. Add the Client File
Copy [`Docs/ABAPI_Client.cs`](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs) directly into your mod's project source.

### 2. How It Works
The `ABAPIClient` class inspects the current `AppDomain` for the loaded `"AdventureBackpacks"` assembly and binds typed reflection delegates. 
- If Adventure Backpacks is not installed, all methods return safe defaults (`false`, `null`, empty collections) with zero missing-assembly exceptions.
- If Adventure Backpacks is installed, calls execute directly against the live mod with cached reflection delegates.

### 3. Implementation Example

```csharp
using System.Collections.Generic;
using AdventureBackpacks.API.Client;
using UnityEngine;

public class SoftBackpackIntegration
{
    public void InspectBackpacks(Player player)
    {
        if (!ABAPIClient.IsAvailable || !ABAPIClient.IsLoaded())
        {
            return;
        }

        if (ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory equippedInventory))
        {
            Debug.Log($"Equipped backpack has {equippedInventory.GetAllItems().Count} items.");
        }

        List<Inventory> allInventories = ABAPIClient.GetAllBackpackInventories(player);
        foreach (Inventory inv in allInventories)
        {
            Debug.Log($"Found backpack inventory with capacity {inv.GetWidth()}x{inv.GetHeight()}.");
        }
    }
}
```

---

## Remediation Guide: Replacing Reflection with the API

If your codebase or AI assistant previously attempted to access Adventure Backpacks state via reflection, replace it immediately:

| ❌ Unsafe Reflection / Patch (DO NOT USE) | ✅ Official API Replacement |
| :--- | :--- |
| `AccessTools.Field(typeof(...), "m_inventory")` | `ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory inv)` |
| `AccessTools.Method(...).Invoke(...)` for equip state | `ABAPIClient.IsBackpackEquipped(player)` |
| Checking item names for `"Backpack"` | `ABAPIClient.IsBackpack(itemData)` |
| Searching player inventory slots for backpacks | `ABAPIClient.GetEquippedBackpack(player)` |
| Accessing all carried/equipped backpacks | `ABAPIClient.GetAllBackpackInventories(player)` |
| Checking if backpack UI is currently open | `ABAPIClient.IsBackpackOpen()` |
| Toggling or opening backpack GUI | `ABAPIClient.OpenBackpack(player, inventoryGui)` |

---

## How to Request Missing API Functionality

If your mod requires functionality that the current API does not expose:
1. **DO NOT** attempt to reverse-engineer or reflect into internal classes.
2. Submit a feature request or discuss it with the team:
   - **GitHub Feature Request**: [Open an Issue](https://github.com/Vapok/AdventureBackpacks/issues/new?template=feature_request.md)
   - **Discord Community**: [Vapok Gaming Discord](https://discord.gg/5YAJkRFBXt)

---

## Comparison Summary

| Characteristic | Option A ([`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases)) | Option B ([`Docs/ABAPI_Client.cs`](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs)) |
| :--- | :--- | :--- |
| **Dependencies** | Requires [`AdventureBackpacksAPI.dll`](https://github.com/Vapok/AdventureBackpacks/releases) at compile time | Zero external dependencies |
| **Build Tooling** | Requires `ILRepack` | Standard compilation (drop-in `.cs`) |
| **Runtime Mechanism** | Dynamic assembly redirection | Cached reflection delegates via `ABAPIClient` |
| **Missing Mod Safety** | Executes repacked dummy stubs returning defaults | Returns safe defaults via null-checked delegates |
| **Recommended Use** | Hard or repacked integrations | Soft, optional integrations |
