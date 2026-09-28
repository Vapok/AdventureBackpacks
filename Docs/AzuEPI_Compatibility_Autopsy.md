# Technical Autopsy: AzuEPI Vanity & Backpack Slot Compatibility Lock

**Date**: September 28, 2026  
**Target Mod**: [AdventureBackpacks](https://github.com/Vapok/AdventureBackpacks)  
**Third-Party Mod**: [AzuExtendedPlayerInventory (AzuEPI)](https://github.com/AzumattDev/AzuExtendedPlayerInventory) (Analyzed Version: `v2.6.0`)  
**Host Engine Target**: Unity 6 (`v6000.0.75f1`), Valheim Engine (`assembly_valheim`)

---

## Executive Summary

A user reported a critical equipment and UI failure when running **AdventureBackpacks** alongside **AzuExtendedPlayerInventory (AzuEPI)**:
* Wearing a Cape and a Backpack simultaneously.
* Enabling AzuEPI's **Vanity** setting on the Cape to make it hidden.
* Upon death and tombstone recovery (`TakeAll` / auto-equip), the Cape's modifiers (armor, frost resistance, movement) stopped applying entirely.
* The Cape became impossible to unequip or interact with while the Backpack was equipped.
* Unequipping the Backpack restored Cape effects; re-equipping caused effects to be suppressed again.
* Resolving the issue required clearing all Vanity overrides, unequipping both items, and restarting the game/server.
* When reported to AzuEPI maintainers on Discord, the issue was dismissed as *"AdventureBackpacks is a mess and any issues like this are because of their code."*

Decompiled bytecode analysis of AzuEPI (`v2.6.0`) proves this assertion is demonstrably false. The breakdown is caused entirely by the internal mechanics of AzuEPI's custom slot interceptor (`CustomEquipVisuals`), its vanity state manager (`VanityAPI`), and its own hardcoded compatibility module (`AdvBackpacksCompat.cs`).

---

## Deep-Dive Technical Root Cause Analysis

### 1. The Core Architecture Conflict: Two `Shoulder` Items in Vanilla Valheim

In vanilla Valheim, only a single shoulder equipment reference exists: `Humanoid.m_shoulderItem`. Both vanilla Capes and Adventure Backpacks share:
```csharp
m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder (17)
```

To support an extra backpack equipment slot alongside the cape slot without rewriting Valheim's core humanoid systems, AzuEPI uses an illusion inside `CustomEquipVisuals.HideTypeWhileEquipping`:

```csharp
// AzuEPI: AzuEPI/Game/PlayerPreview/CustomEquipVisuals.cs (Lines 564–572)
if (IsManaged(item) && HasActiveSlot(item) && IsReserved(item.m_shared.m_itemType) && !__instance.IsItemEquiped(item))
{
    __state = item.m_shared.m_itemType;
    item.m_shared.m_itemType = API.GetFakeItemType(); // Temporarily mutates Shoulder -> FakeItemType (-100)
    ...
}
```

Under nominal operation:
1. AzuEPI intercepts `Humanoid.EquipItem(backpack)` and temporarily mutates `m_itemType` to `FakeItemType` so vanilla Valheim does not invoke `UnequipItem(m_shoulderItem)` on the Cape.
2. In `HideTypeWhileEquipping.Postfix`, AzuEPI sets `item.m_equipped = true` and restores `item.m_shared.m_itemType = Shoulder`.
3. Nominal result: `player.m_shoulderItem` remains the Cape, while the Backpack is tracked in AzuEPI's custom slot (`$bp_backpack_slot_name`).

---

### 2. The Vanity Conflict: AzuEPI Lacks a Dedicated Backpack Vanity Slot

AzuEPI maps inventory items to its internal visual slots using `VanitySlots.TryMapItemTypeToVisSlot`:

```csharp
// AzuEPI: AzuEPI/Game/Panels/Vanity/VanitySlots.cs (Lines 41–45)
if ((int)t == 17) // ItemType.Shoulder
{
    slot = (VisSlot)7; // VisSlot.Shoulder
    return true;
}
```

**AzuEPI does not define an independent vanity category for backpacks.** Both Capes and Backpacks map to the exact same enum value: `VisSlot.Shoulder (7)`.

When the user set vanity on the Cape to **Hidden**:
1. AzuEPI set `VanityZdoKeys.Shoulder` to `int.MinValue` (`VanityAPI.HIDE`).
2. AzuEPI serialized this state permanently into the player's profile:
   ```csharp
   // AzuEPI: AzuEPI/Game/Panels/Vanity/VanityAPI.cs (Line 205)
   player.m_customData["AzuEPI.Vanity"] = "...:int.MinValue:...";
   ```
3. AzuEPI's Harmony prefix on `VisEquipment.SetShoulderEquipped` forces the visual hash to `0`:
   ```csharp
   // AzuEPI: AzuEPI/Game/Panels/Vanity/Vanity_Shoulder.cs (Lines 8–15)
   private static void Prefix(VisEquipment __instance, ref int hash, ref int variant)
   {
       int vanityValue = VanityHelper.GetVanityValue(__instance, VanityZdoKeys.Shoulder);
       if (VanityAPI.IsHidden(vanityValue))
       {
           hash = 0;
           variant = 0;
       }
       ...
   }
   ```
Because AzuEPI shares a single `VisSlot.Shoulder` across all shoulder items, setting the Cape to Hidden directly impacts whichever item is currently bound to `player.m_shoulderItem`.

---

### 3. Death & Tombstone Recovery: Slot Inversion & Modifier Dropping

When the player died and retrieved their tombstone:
1. `TombstonePatches.AutoEquipAfterTombstoneGrab.Postfix` re-equipped items by iterating over `API.GetEquipmentSlotSnapshots(inventory)`.
2. Due to the order of slot processing and inventory resolution, the **Backpack** was assigned to vanilla's `player.m_shoulderItem`, and the **Cape** was placed into AzuEPI's secondary slot.
3. Once `player.m_shoulderItem` held the Backpack, **AzuEPI's own modifier patches completely dropped the Cape**:

```csharp
// AzuEPI: AzuEPI/Game/Compatibility/AdvBackpacks/AdvBackpacksCompat.cs (Lines 112–128)
[HarmonyPatch(typeof(Player), "UpdateModifiers")]
[HarmonyPostfix]
private static void UpdateModifiers_Postfix(Player __instance)
{
    if ((Object)__instance != (Object)Player.m_localPlayer || 
        Player.s_equipmentModifierSourceFields == null || 
        IsBackpackItem(((Humanoid)__instance).m_shoulderItem)) // <--- CRITICAL BUG
    {
        return; // ABORTS! Ignores any equipped cape in secondary slots!
    }
    ...
}

// AzuEPI: AzuEPI/Game/Compatibility/AdvBackpacks/AdvBackpacksCompat.cs (Lines 144–156)
[HarmonyPatch(typeof(Player), "ApplyArmorDamageMods")]
[HarmonyPostfix]
private static void ApplyArmorDamageMods_Postfix(Player __instance, ref DamageModifiers mods)
{
    if (!((Object)__instance != (Object)Player.m_localPlayer) && 
        !IsBackpackItem(((Humanoid)__instance).m_shoulderItem)) // <--- CRITICAL BUG
    {
        ItemData val = FindEquippedBackpack(__instance);
        if (val != null)
        {
            mods.Apply(val.m_shared.m_damageModifiers);
        }
    }
}
```

#### What Happens Here:
* Vanilla `Player.UpdateModifiers()` and `Player.ApplyArmorDamageMods()` **only** inspect `player.m_shoulderItem`.
* When the Backpack is in `m_shoulderItem`, vanilla calculates modifiers from the Backpack.
* AzuEPI's postfix checks: `if (IsBackpackItem(m_shoulderItem)) return;`.
* AzuEPI **never queries or applies the modifiers of the Cape residing in its own secondary slot**.
* As a result, frost resistance, armor, and all cape stats are 100% ignored.

---

### 4. The Interaction / Unequip Deadlock

When the user attempted to right-click the Cape to unequip it:
1. `InventoryGui.OnRightClickItem` calls `Player.UseItem(...)` -> `Humanoid.ToggleEquipped(cape)`.
2. Vanilla `ToggleEquipped` checks `Humanoid.IsItemEquiped(cape)`:
   ```csharp
   // Vanilla assembly_valheim: Humanoid.IsItemEquiped (Lines 28–31)
   if (m_shoulderItem == item)
   {
       return true;
   }
   return false;
   ```
   Because `player.m_shoulderItem` is the Backpack, vanilla returns `false`.
3. AzuEPI patches `Humanoid.IsItemEquiped`, but **only for items registered in its API slots**:
   ```csharp
   // AzuEPI: AzuEPI/Game/PlayerPreview/CustomEquipVisuals.cs (Lines 484–503)
   [HarmonyPatch(typeof(Humanoid), "IsItemEquiped")]
   private static class IsItemEquipedPatch
   {
       private static void Postfix(Humanoid __instance, ItemData item, ref bool __result)
       {
           ...
           string name = item.m_dropPrefab.name;
           if (_registered.Contains(name) && value.Equipped.TryGetValue(name, out EquippedEntry value2) && value2.Item == item)
           {
               __result = true;
           }
       }
   }
   ```
   The vanilla Cape is **not** in `_registered`. Therefore, `IsItemEquiped(cape)` returns `false`.
4. Because `IsItemEquiped(cape)` returns `false`, `ToggleEquipped` believes the Cape is **already unequipped** and branches to `EquipItem(cape)` instead of `UnequipItem(cape)`.
5. Vanilla `EquipItem(cape)` sees `cape.m_shared.m_itemType == Shoulder` and calls `UnequipItem(m_shoulderItem)` to unequip the Backpack.
6. **AzuEPI's prefix intercepts and aborts the unequip**:
   ```csharp
   // AzuEPI: AzuEPI/Game/PlayerPreview/CustomEquipVisuals.cs (Lines 693–730)
   [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
   private static class UnequipItemPatch
   {
       [HarmonyPrefix]
       [HarmonyPriority(800)]
       private static bool Prefix(Humanoid __instance, ItemData item)
       {
           if (!HideTypeWhileEquipping.IsVanillaEquipping)
               return true;

           string name = item.m_dropPrefab.name;
           if (!_registered.Contains(name))
               return true;

           // Backpack IS in _registered!
           return false; // <--- AZUEPI CANCELS THE UNEQUIP!
       }
   }
   ```
7. Result:
   * The Cape cannot be unequipped because the engine thinks it is not equipped.
   * The Cape cannot be equipped because AzuEPI blocks the Backpack from being unequipped.
   * The user is locked in an inescapable input deadlock created entirely by AzuEPI's conflicting prefix/postfix rules.

---

### 5. Why Clearing Vanity and Restarting Recovered State

Because AzuEPI stores vanity overrides in `player.m_customData["AzuEPI.Vanity"]`, the `int.MinValue` hide flag is persisted into the `.fch` player save file. 

Only by:
1. Clearing the vanity override in the AzuEPI UI (which calls `VanityAPI.ClearVanity` and clears the custom data key),
2. Unequipping both items so `player.m_shoulderItem` and AzuEPI's `_states` dictionary were reset to null, and
3. Reloading the character,

was the corrupted state machine purged.

---

### 6. Evidence: AzuEPI Invasively Patches AdventureBackpacks Internals

AzuEPI maintainers claiming this is an AdventureBackpacks bug is contradicted by their own source code. AzuEPI contains a dedicated compatibility class (`AzuEPI.Game.Compatibility.AdvBackpacks.AdvBackpacksCompat`) specifically written by Azumatt to hook directly into AdventureBackpacks:

```csharp
// AzuEPI: AzuEPI/Game/Compatibility/AdvBackpacks/AdvBackpacksCompat.cs
namespace AzuEPI.Game.Compatibility.AdvBackpacks;

public class AdvBackpacksCompat
{
    // Hardcoded AB Prefabs
    internal static readonly HashSet<string> Backpacks = new HashSet<string>(StringComparer.Ordinal) 
    { 
        "BackpackMeadows", "BackpackBlackForest", "BackpackSwamp", 
        "BackpackMountains", "BackpackPlains", "BackpackMistlands", 
        "CapeSilverBackpack", "CapeIronBackpack" 
    };

    // Harmony hooks directly targeting AdventureBackpacks internal classes:
    [HarmonyPatch("AdventureBackpacks.Patches.HumanoidPatches+HumanoidUnequipItemPatch, AdventureBackpacks", "Prefix")]
    public static void AdventureBackpackHumanoidUnequipItemPatchPrefix(ref ItemData __state)
    {
        // Mutates player.m_shoulderItem under the hood
        __state = localPlayer.m_shoulderItem;
        ItemData val = FindEquippedBackpack(localPlayer);
        if (val != null)
            localPlayer.m_shoulderItem = val;
    }

    [HarmonyPatch("AdventureBackpacks.Patches.HumanoidPatches+HumanoidUnequipItemPatch, AdventureBackpacks", "Prefix")]
    public static void AdventureBackpackHumanoidUnequipItemPatchPostfix(ref ItemData __state)
    {
        localPlayer.m_shoulderItem = __state;
    }

    [HarmonyPatch("AdventureBackpacks.Extensions.PlayerExtensions, AdventureBackpacks", "IsBackpackEquipped")]
    [HarmonyPostfix]
    public static void AdvBackpackIsBackpackEquipped(ref bool __result) { ... }

    [HarmonyPatch("AdventureBackpacks.Extensions.PlayerExtensions, AdventureBackpacks", "GetEquippedBackpack")]
    [HarmonyPrefix]
    public static void AdvBackpackGetEquippedBackpackPrefix(Player player, ref ItemData __state) { ... }
}
```

AzuEPI explicitly hooks, wraps, and mutates AdventureBackpacks' internal methods, controls the custom slot registration, suppresses the modifiers, and manages the vanity layer.

---

## Conclusion & Actionable Findings

| Symptom | Responsible Codebase | Mechanism |
| :--- | :--- | :--- |
| **Cape & Backpack visual collision under Vanity** | **AzuEPI** (`VanitySlots.cs`) | Lacks an independent backpack vanity slot; both items share `VisSlot.Shoulder (7)`. |
| **Cape modifiers (frost resist, armor) lost on grave retrieval** | **AzuEPI** (`AdvBackpacksCompat.cs`) | `UpdateModifiers_Postfix` and `ApplyArmorDamageMods_Postfix` abort with `if (IsBackpackItem(m_shoulderItem)) return;` without checking the secondary cape slot. |
| **Inability to interact/unequip the Cape** | **AzuEPI** (`CustomEquipVisuals.cs`) | `IsItemEquipedPatch` returns false for capes in secondary slots, causing clicks to attempt re-equip; `UnequipItemPatch` then blocks unequipping the backpack, deadlocking input. |
| **State persistence across relogs** | **AzuEPI** (`VanityAPI.cs`) | Serializes `int.MinValue` hide sentinel into character `player.m_customData["AzuEPI.Vanity"]`. |
