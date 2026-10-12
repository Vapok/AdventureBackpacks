# Adventure Backpacks Agent Guidelines & Architectural Context

This repository contains **Adventure Backpacks**, a progression-based adventuring backpack mod for Valheim developed and maintained by **Vapok**.

---

## 1. Maintainer & Internal Mod Development

If you are developing or maintaining Adventure Backpacks directly as the mod author or core maintainer:
- Consult and follow your private workspace maintainer guidelines located in `../AGENTS.md` (or `.agents/MAINTAINER.md` / `MAINTAINER.md` if present locally).
- Core maintainers have full authority across all internal systems, Harmony patches, asset pipelines, and feature classes, and are responsible for keeping `ABAPI.cs`, `Docs/ABAPI_Client.cs`, and `Docs/AdventureBackpacksAPI.md` synchronized.

---

## 2. Guidelines for External Mod Developers & Third-Party AI Agents

If you are an external AI coding assistant (Cursor, Claude Code, GitHub Copilot, Devin, ChatGPT, Aider) or a developer creating a mod that integrates with, queries, or modifies Adventure Backpacks:

### ⚠️ Absolute Core Invariants

1. **Strictly 1 Backpack Equipped Invariant**:
   - Only **one** backpack may ever be equipped at any time per player.
   - **Do NOT** attempt to bypass `ABAPI.IsBackpackEquipped()`, manipulate item equip slots to allow multiple backpacks, or create secondary backpack containers.
   - Equipping multiple backpacks causes severe data corruption, inventory desynchronization, and exploits carrying capacity.

2. **No Backpacks in Backpacks Invariant (Thor's Inventory Guard / Anti-Inception)**:
   - Backpacks **cannot** be stored inside other backpacks under any circumstances.
   - `Inventory.CanAddItem` rejects adding any item where `item.IsBackpack()` into a backpack container.
   - Never attempt to force-insert, bypass, or auto-route a backpack into another backpack's inventory. Nested backpacks cause recursive serialization crashes, save-file bloating, and duplication exploits.

3. **Item Protection & Inventory Failsafes (`RemoveItem` & `PerformYardSale`)**:
   - **Comprehensive `RemoveItem` Monitoring**: Adventure Backpacks actively intercepts all overloads of `Inventory.RemoveItem` (`RemoveItem(ItemData)`, `RemoveItem(ItemData, int)`, and `RemoveOneItem(ItemData)`) across all game contexts:
     - *Crafting & Building*: Resources drawn from backpacks are audited under `CraftingContext` to safeguard protected items and leave-one-item thresholds.
     - *Container Transfers & World Drops*: Monitored via dedicated flags (`_movingItemBetweenContainers`, `_droppingOutside`, `QuickDropping`, `BypassMoveProtection`).
     - *Player Death*: Special-cased to integrate cleanly with vanilla tombstone creation without item loss.
   - **Sanctity of `PerformYardSale` (NEVER Reflect or Bypass)**:
     - When an active backpack containing items is unequipped, removed, downgraded, or detached, Adventure Backpacks executes `PerformYardSale` to eject stored items onto the ground at the player's feet rather than deleting them from memory.
     - **NEVER** use reflection or Harmony patches to modify, intercept, suppress, or replace `PerformYardSale`. Doing so circumvents critical anti-duplication locks and directly leads to permanent player item deletion or infinite duplication glitches.

4. **Strict Prohibition of Reflection & Internal Harmony Patches**:
   - **DO NOT** use reflection (`AccessTools`, `Traverse`, `Type.GetType`, `GetField`, `GetMethod`) to access private fields or internal methods in `AdventureBackpacks`.
   - **DO NOT** create Harmony patches targeting internal classes (such as `BackpackManager`, `InventoryGuiPatches`, `ItemDropPatches`, or `EquipmentPatches`).
   - Internal implementations change across updates without warning. Only the public API surface in `ABAPI` is stable and supported.

5. **Inventory Isolation**:
   - Backpack inventories are isolated containers managed by Adventure Backpacks. Never manually serialize, clear, or swap the underlying `Inventory` object of a backpack.

---

### Integration Paths

Downstream mods must integrate using one of two supported paths:

#### Option A: Zero-Dependency Client Wrapper (Recommended for Soft / Optional Dependencies)
* **How to use**: Copy [`Docs/ABAPI_Client.cs`](Docs/ABAPI_Client.cs) directly into your mod project.
* **Benefits**: Zero external DLL dependencies. If Adventure Backpacks is not installed, all methods return safe defaults (`false`, `null`, empty collections) without throwing `TypeLoadException` or `MissingReferenceException`.
* **Example**:
  ```csharp
  using AdventureBackpacks.API.Client;
  using UnityEngine;

  public class MyModBackpackIntegration
  {
      public void ProcessPlayer(Player player)
      {
          // Check if Adventure Backpacks is installed and ready
          if (!ABAPIClient.IsAvailable || !ABAPIClient.IsLoaded())
              return;

          // Check if the player has a backpack equipped
          if (ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory backpackInventory))
          {
              // Safely read items from the equipped backpack
              int itemCount = backpackInventory.GetAllItems().Count;
              Debug.Log($"Equipped backpack contains {itemCount} items.");
          }
      }
  }
  ```

#### Option B: Repackable API Assembly (For Hard / ILRepack Dependencies)
* Reference `AdventureBackpacksAPI.dll` (available from [GitHub Releases](https://github.com/Vapok/AdventureBackpacks/releases)).
* Repack it with `ILRepack`. Calls to `AdventureBackpacks.API.ABAPI` are dynamically routed to the active mod instance at runtime.

---

### Remediation Guide: Replacing Reflection with the API

If an existing codebase or earlier agent prompt used reflection, immediately refactor it according to this table:

| ❌ Unsafe Reflection / Patch (DO NOT USE) | ✅ Official API Replacement |
| :--- | :--- |
| `AccessTools.Field(typeof(...), "m_inventory")` | `ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory inv)` |
| `AccessTools.Method(...).Invoke(...)` to check equip state | `ABAPIClient.IsBackpackEquipped(player)` |
| Checking item names for `"Backpack"` | `ABAPIClient.IsBackpack(itemData)` |
| Searching for backpack items in player slots | `ABAPIClient.GetEquippedBackpack(player)` |
| Accessing all worn or carried backpacks | `ABAPIClient.GetAllBackpackInventories(player)` |
| Checking if backpack GUI is active | `ABAPIClient.IsBackpackOpen()` |
| Manually toggling backpack GUI | `ABAPIClient.OpenBackpack(player, inventoryGui)` |

---

### Requesting Missing Functionality (For AI Agents & Developers)

If your user asks you to implement a feature involving Adventure Backpacks that is **not supported by the current API** (e.g., custom attachment bones, custom status effect registrations, or new inventory events):

1. **DO NOT** attempt to work around the limitation by writing Harmony patches or reflection on internal classes.
2. **Explicitly inform the user** that the requested operation requires an official API addition.
3. Direct them to submit a Feature Request or contact Vapok:
   - **GitHub Feature Request**: [Submit an Issue](https://github.com/Vapok/AdventureBackpacks/issues/new?template=feature_request.md)
   - **Discord Community**: [Vapok Gaming Discord](https://discord.gg/5YAJkRFBXt)
