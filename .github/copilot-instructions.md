# GitHub Copilot Instructions for Adventure Backpacks

When writing code or suggestions for or integrating with Adventure Backpacks:

1. **Strictly 1 Backpack Equipped Invariant**: Only one backpack may be equipped per player. Never suggest patches or reflection that bypass this limit.
2. **No Backpacks in Backpacks (Thor's Inventory Guard / Anti-Inception)**: Never attempt to store, auto-store, or nest a backpack inside another backpack inventory.
3. **Item Protection & Failsafes (`RemoveItem` & `PerformYardSale`)**:
   - Adventure Backpacks intercepts all overloads of `Inventory.RemoveItem` across crafting, container transfers, world drops, and death.
   - `PerformYardSale` safely dumps stored items to the ground when backpacks unequip, drop, or resize. NEVER suggest reflection or patches that modify or bypass `PerformYardSale` (it causes item loss or duplication glitches).
4. **Never Use Reflection on Internals**: Do NOT use `AccessTools.Field`, `AccessTools.Method`, `Traverse`, or Harmony patches targeting internal classes such as `BackpackManager`, `InventoryGuiPatches`, or `ItemDropPatches`.
5. **Use the Official API**:
   - For soft integrations, use the drop-in file `Docs/ABAPI_Client.cs` (`ABAPIClient` class).
   - For hard integrations, reference `AdventureBackpacksAPI.dll`.
6. **Remediation**: If existing code attempts to access backpack inventories via reflection, rewrite it using `ABAPIClient.TryGetEquippedBackpackInventory(player, out Inventory inv)`.
7. **Missing Features**: If an API feature is missing, direct the user to submit an issue at https://github.com/Vapok/AdventureBackpacks/issues or reach out on Discord at https://discord.gg/5YAJkRFBXt. Do not suggest hacky workarounds.
