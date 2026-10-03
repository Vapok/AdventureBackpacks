# Adventure Backpacks API Integration Guide

This guide details how external mod developers can integrate with Adventure Backpacks.

Two integration workflows are supported:
1. **Option A: Repackable API Assembly (`AdventureBackpacksAPI.dll`)** — Compile against the standalone API assembly and merge it using `ILRepack`.
2. **Option B: Zero-Dependency Client Wrapper (`Docs/ABAPI_Client.cs`)** — Drop a single standalone C# file into your project with zero external DLL references.

For complete class, struct, and method signatures, consult [AdventureBackpacksAPI.md](file:///home/vapok/Modding/Valheim/AdventureBackpacks/Docs/AdventureBackpacksAPI.md).

---

## Option A: Repackable API Assembly (`AdventureBackpacksAPI.dll`)

This option is recommended if your mod already uses `ILRepack` to bundle dependencies and you prefer compile-time type safety directly against the API types.

### 1. Download the API Assembly
Download `AdventureBackpacksAPI-Vapok-<Version>.zip` from [GitHub Releases](https://github.com/Vapok/AdventureBackpacks/releases). Extract `AdventureBackpacksAPI.dll`.

### 2. Add Project Reference
Reference `AdventureBackpacksAPI.dll` in your `.csproj`:

```xml
<ItemGroup>
    <Reference Include="AdventureBackpacksAPI">
        <HintPath>path\to\AdventureBackpacksAPI.dll</HintPath>
        <Private>False</Private>
    </Reference>
</ItemGroup>
```

### 3. Configure ILRepack
Add `AdventureBackpacksAPI.dll` to your `ILRepack` inputs. Adventure Backpacks embeds `APIManager` at runtime, which intercepts BepInEx assembly loading and dynamically redirects all IL references from `AdventureBackpacks.API` to the live `AdventureBackpacks.dll` assembly.

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

## Option B: Zero-Dependency Client Wrapper (`ABAPI_Client.cs`)

This option is recommended for mods that integrate with Adventure Backpacks as an optional soft dependency and want to avoid adding extra assembly references or configuring ILRepack.

### 1. Add the Client File
Copy [Docs/ABAPI_Client.cs](file:///home/vapok/Modding/Valheim/AdventureBackpacks/Docs/ABAPI_Client.cs) directly into your mod's project source.

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

## Comparison Summary

| Characteristic | Option A (`AdventureBackpacksAPI.dll`) | Option B (`ABAPI_Client.cs`) |
| :--- | :--- | :--- |
| **Dependencies** | Requires `AdventureBackpacksAPI.dll` at compile time | Zero external dependencies |
| **Build Tooling** | Requires `ILRepack` | Standard compilation (drop-in `.cs`) |
| **Runtime Mechanism** | Bytecode redirection via `APIManager` | Cached reflection delegates via `ABAPIClient` |
| **Missing Mod Safety** | Executes repacked dummy stubs returning defaults | Returns safe defaults via null-checked delegates |
| **Recommended Use** | Hard or repacked integrations | Soft, optional integrations |
