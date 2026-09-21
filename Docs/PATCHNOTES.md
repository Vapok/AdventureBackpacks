# 2.1.9 - Umbrella Water Resistance & Weather Fixes
* **Environmental Status Effect Lifecycle Hooking (`Patches/Player.cs`)**:
  * Implemented `PlayerUpdateEnvStatusEffectsPatch` with `[HarmonyPrepare]` dedicated server isolation.
  * Added `Prefix`, `Postfix`, and `Finalizer` around `Player.UpdateEnvStatusEffects` managing `IsUpdatingEnvStatusEffects` state flag.
  * Executes virtual `EffectsBase.OnUpdateEnvStatusEffects(Player player)` on active backpack effects during the environmental cycle.
* **Umbrella Rain Prevention & Immersion Decoupling (`Patches/SEMan.cs`)**:
  * Added `AddStatusEffectPatch` targeting `SEMan.AddStatusEffect(int, bool, int, float, short)`.
  * When `IsUpdatingEnvStatusEffects` is active and the status effect hash matches `SEMan.s_statusEffectWet`, verifies if the character possesses an active `WaterResistance` backpack effect.
  * If water resistant, blocks rain-applied wetness (`__runOriginal = false`, `__result = null`).
  * Decouples rain application from water submersion (`Character.UpdateWater`), preserving natural swimming wetness while allowing prior wet status timers to tick down cleanly without rain resets.
* **Winter Cold Resistance & Weather Mod Overwrites (`Assets/Effects/ColdResistance.cs`)**:
  * Implemented `OnUpdateEnvStatusEffects(Player player)` in `ColdResistance` to invoke `player.GetSEMan().RemoveStatusEffect(SEMan.s_statusEffectCold, true)`.
  * Neutralizes winter weather overrides from external mods (such as Seasonality's `EnvMan.IsCold` Postfix) while maintaining engine-native cold handling.
* **Retirement of Destructive Environment Overrides (`Patches/EnvMan.cs`)**:
  * Emptied legacy `EnvManIsCold` and `EnvManIsWet` patches.
  * Restores authentic vanilla physics for campfires, structural cover, and ambient drying calculations.
* **Defensive Null-Safety & Headless Server Hardening (`Assets/Items/BackpackItems/BackpackSwamp.cs`, `Assets/Effects/Waterproof.cs`)**:
  * Added defensive null checks around `ObjectDB.instance` and `wet.m_icon` in `Waterproof.LoadExternalStatusEffect`.
  * Added null guards on `Item`, `Item.DropsFrom`, and `BackpackBiome` in `BackpackSwamp`, resolving startup exception [ADVENTUREBACKPACKS-1G](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-1G).

# 2.1.8 - Ecosystem Compatibility & Documentation Update
* **ContentsWithin Container Proxy Hook Update (`Compats/ContentsWithin.cs`)**:
  * Updated `ContainerAccessPrefix` to validate container targets using `ContainerPatches.IsBackpackProxy()` instead of obsolete `container.name.Equals("Player(Clone)")`.
  * Guarantees that hover contents inspection from MSchmoecker's `ContentsWithin` correctly identifies and ignores modern `AB_BackpackProxy` child containers attached to players.
* **AzuExtendedPlayerInventory Cape Rendering Fix (`AdventureBackpacks.cs`)**:
  * Replaced static plugin detection with dynamic patch inspection via `BoneReorderAlreadyApplied()`.
  * Checks whether `VisEquipment.SetShoulderEquipped` has already been patched by an external `BoneReorder` postfix before applying ours.
  * Resolves an issue where capes became invisible when worn alongside a backpack with `AzuExtendedPlayerInventory` installed (PR #189, credit @BaalEvan).
  * Enclosed `BoneReorder.ApplyOnEquipmentChanged` in defensive exception handling to prevent unexpected startup aborts.
* **Documentation & Compatibility Catalogue (`README.md`)**:
  * Updated and verified compatibility entries across modern Valheim mod ecosystem.
  * Separated and clarified entries for `AzuCraftyBoxes`, `AzuAutoStore`, `AzuExtendedPlayerInventory`, `EquipmentAndQuickSlots` (EAQS), and `ExtraSlots`.
  * Deprecated outdated reference to `AutoSplitStack`.
* **Dedicated Server Hardening & Isolation**:
  * Implemented fail-before-patching via `[HarmonyPrepare]` returning `!PlayerExtensions.IsDedicatedOrHeadless()` across all client-only patch classes:
    * `Features/QuickTransfer.cs` (`OnRightClickItemPatch`, `UseItemPatch`).
    * `Patches/InventoryGui.cs` (`InventoryGuiDoCraftingPrefix`, `InventoryGuiOnSelectedItem`, `InventoryGuiUpdateTranspiler`, `InventoryGuiSetupRequirementPatch`).
    * `Patches/GuiBar.cs` (`GuiBarAwakePatch`).
    * `Patches/InventoryGrid.cs` (`UpdateGuiPatch`).
    * `Patches/Inventory.cs` (`OnDropOutsideItemPatch`, `InventoryGridDropItemPatch`, `HumanoidDropItemPatch`, `UpdateTotalWeightPatch`, `IsTeleportablePatch`).
    * `Patches/Humanoid.cs` (`HumanoidUpdateEquipmentStatusEffectsPatch`, `HumanoidUnequipItemPatch`, `HumanoidEquipItemPatch`).
    * `Patches/EnvMan.cs` (`EnvManIsCold`, `EnvManIsWet`).
    * `Patches/Door.cs` (`HaveDoorKeyPatch`).
    * `Patches/Recipe.cs` (`RecipeGetAmountPatch`).
    * `Patches/SEMan.cs` (`RemoveStatusEffects`).
    * `Patches/FejdStartup.cs` (`FejdStartupAwakePatch`).
  * Prevents Harmony from generating dynamic detours or hooking engine methods on headless/dedicated server instances, completely eliminating detour overhead, client HUD/GUI references, and `MissingMethodException` during character interactions ([ADVENTUREBACKPACKS-14](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-14)).
  * Added dedicated server short-circuit guards to `Compats/AzuCraftyBoxesCompat.cs`, `Compats/ContentsWithin.cs`, `BoneReorder.ApplyOnEquipmentChanged` in `AdventureBackpacks.cs`, and `Features/StoreToBackpack.cs`.
* **Dependency & Reference Synchronization**:
  * Updated `Vapok.Valheim.Common` SDK reference to `v3.16.1015`.
  * Updated `JotunnLib` reference to `v2.30.2`.

# 2.1.7 - Storage & Inventory Mod Compatibility
* **Proxy Container Network View Isolation (`Patches/Container.cs`, `Extensions/PlayerExtensions.cs`)**:
  * Added a dedicated `ZNetView` component to `AB_BackpackProxy` with `ZNetViewAwakePatch` prefix suppressing `Awake()`.
  * Prevents ZDO registration in `ZDOMan` and isolates `container.m_nview` from `Player.m_localPlayer.m_nview`.
  * Resolves Sentry key collision [ADVENTUREBACKPACKS-18](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-18) where external container mods (such as MidgardPlus registering `"MP_SharedOpen2"`, hash `-1559719815`) collided when registering RPCs on proxy container awakes.
* **AzuExtendedPlayerInventory BoneReorder Compatibility (`AdventureBackpacks.cs`)**:
  * Guarded `BoneReorder.ApplyOnEquipmentChanged` against active `Azumatt.AzuExtendedPlayerInventory` installations via `Chainloader.PluginInfos`.
  * Eliminates dual-patching of `VisEquipment.SetHelmetEquipped` that triggered `InvalidProgramException` ([ADVENTUREBACKPACKS-1B](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-1B)).
* **Crafting Item Consumption Bounds & Stack Safety (`Features/CraftFromBackpack.cs`, `Patches/Player.cs`)**:
  * Hardened `ConsumeCraftingItem` and `PlayerPatches.ConsumeUnEquippedItems` with explicit typing and null/empty stack guards (`item == null || item.m_stack <= 0`).
  * Resolves `NullReferenceException` and `ArgumentOutOfRangeException` during rapid-click crafting ([ADVENTUREBACKPACKS-V](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-V) and [ADVENTUREBACKPACKS-1A](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-1A)).
* **Defensive Config & Effect Null Guards (`Assets/Items/BackpackItem.cs`, `Assets/Effects/EffectsBase.cs`)**:
  * Guarded all `SettingChanged` event subscriptions against null `ConfigEntry` objects in `BackpackItem` ([ADVENTUREBACKPACKS-19](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-19)).
  * Guarded against null `backpack.BackpackBiome` in `EffectsBase.IsEffectActive` ([ADVENTUREBACKPACKS-1C](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-1C)).

# 2.1.6 - Bug Fixes & Slot Mod Compatibility
* **Downstream Reflection Signature Restoration (`Patches/Humanoid.cs`)**:
  * Restored the 1-argument `Prefix(ItemDrop.ItemData __0)` signature on `HumanoidUnequipItemPatch.Prefix`.
  * Downstream ecosystem slot extension mods (specifically `shudnal-ExtraSlotsCustomSlots`) unpatch our Harmony hook and dynamically invoke `HumanoidUnequipItemPatch.Prefix` via reflection with a single `ItemDrop.ItemData` argument during unequip sequences.
  * Resolves an unhandled `TargetParameterCountException` during `Player.CreateTombStone` -> `Humanoid.UnequipAllItems` that aborted tombstone spawning, item dropping, and player respawn timers on character death.
* **Proxy Container Awake Guard & Network View Initialization (`Patches/Container.cs`)**:
  * In `ContainerAwakePatch.Prefix`, initialized `m_nview` (pointing to `Player.m_localPlayer.m_nview`) and `m_inventory` on `AB_BackpackProxy` if null before returning `false`.
  * Resolves Sentry regression [ADVENTUREBACKPACKS-Z](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-Z) where third-party mod postfixes on `Container.Awake` (such as `AzuCraftyBoxes.Patches.ContainerAwakePatch.Postfix`) dereferenced `container.m_nview.IsValid()` before `AddComponent<Container>()` finished returning.

# 2.1.5 - Multi-Player Isolation & Crafting Mod Compatibility
* **AzuCraftyBoxes Dynamic Compatibility Bridge (`Compats/AzuCraftyBoxesCompat.cs` & `Compats/BackpackContainerRealProxy.cs`)**:
  * Implemented decoupled dynamic compatibility for Azumatt's `AzuCraftyBoxes` without compile-time binary dependencies, eliminating `TypeLoadException` risks if the mod is absent or updated.
  * Utilized `System.Runtime.Remoting.Proxies.RealProxy` to dynamically implement `AzuCraftyBoxes.IContainers.IContainer` via a transparent proxy duck-typed at runtime.
  * Injected the transparent proxy into `AzuCraftyBoxes.Util.Functions.Boxes.QueryFrame.Get<Player>` via Harmony postfix, automatically integrating equipped backpack storage into AzuCraftyBoxes' chest radius queries, crafting station consumption (`MiscFunctions.ProcessRequirements`), building piece requirements, and UI count computations (`UiItemBank`).
  * Proxy implementation directly manages `ItemCount`, `ContainsItem`, `ProcessContainerInventory`, `GetInventory`, `GetPosition`, `Save`, `GetPrefabName`, and `RemoveItem`, cleanly decrementing stack counts, saving backpack state, and invoking `backpackInventory.Changed()`.
  * Implemented explicit `Equals`, `GetHashCode`, and `ToString` dispatches on `BackpackContainerRealProxy` alongside reverse iteration deduplication in `AzuCraftyBoxesCompat.QueryFrameGetPostfix`. Prevents duplicate proxy instances from accumulating in Azu's 0.25-second `_cachedAll` list across game frames, eliminating fluctuating/spinning material numbers in the crafting UI.
  * Added fallback postfixes on `UiItemBank.GetTotalAnyQuality` and `GetTotalAtQuality` that activate only if `QueryFrame.Get` fails to bind, guaranteeing accurate crafting requirement counts.
  * Included graceful error trapping and BepInEx log warnings if AzuCraftyBoxes is detected but reflection binding fails.
* **Public Developer API Enhancements & Bug Fixes (`API/ABAPI.cs` & `API/Privates.cs`)**:
  * Fixed boolean inversion in `Privates.GetBackPackDefinitionFromComponent` (`if (!isBackpack || backpack == null) return null;`), resolving a critical bug where `ABAPI.GetEquippedBackpack()` and `ABAPI.GetBackpack()` returned `null` for all default mod backpacks.
  * Added `ABAPI.GetEquippedBackpackInventory(Player player)` and `ABAPI.TryGetEquippedBackpackInventory(Player player, out Inventory inventory)` to provide direct, clean access to the equipped backpack's `Inventory`.
  * Added `ABAPI.GetBackpackInventory(ItemDrop.ItemData itemData)` and `ABAPI.TryGetBackpackInventory(ItemDrop.ItemData itemData, out Inventory inventory)` allowing third-party mods to retrieve backpack inventories directly from inventory items.
* **Multi-Player State Isolation & Dedicated Server Bypass (`Extensions/PlayerExtensions.cs`)**:
  * Removed static `_backpackProxyContainer` field that previously caused multi-player state clobbering across concurrent character instances.
  * Proxies are now stored per-player on a dedicated child GameObject (`AB_BackpackProxy`) parented directly to that player's transform.
  * Added `IsDedicatedOrHeadless()` runtime check (`ZNet.instance?.IsDedicated() == true` or `SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null`) and `player != Player.m_localPlayer` checks ensuring container proxies are never instantiated on dedicated servers or remote multiplayer clients.
  * Introduced `createIfMissing` parameter (default `false`) across `GetBackpackContainerProxy()`. Equipment handling (`Humanoid.EquipItem`), inventory modification listeners (`Inventory.Changed`), and container resizing checks no longer eagerly spawn proxy containers. Proxies are only created when `PlayerExtensions.OpenBackpack()` explicitly requests one for the local player's `InventoryGui`.
  * Fully resolves Sentry crash [ADVENTUREBACKPACKS-Z](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-Z).
* **Early Awake Intercept for Third-Party Container Mods (`Patches/Container.cs`)**:
  * Added `[HarmonyPriority(Priority.First)]` to `ContainerAwakePatch` on `Container.Awake()`. Ensures Adventure Backpacks suppresses vanilla `Container.Awake()` on proxy containers before any third-party mods (such as `AzuAutoStore`) execute their awake patches on the proxy.
* **Defensive Null Safety & Deserialization Hardening (`Components/BackpackComponent.cs`)**:
  * Fixed `NullReferenceException` in `BackpackComponent.Deserialize` ([ADVENTUREBACKPACKS-13](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-13)) by enclosing debug logging inside the `try/catch` block and null-guarding `Item?.m_shared?.m_name` and inventory counts.
  * Added defensive null checks to `CraftFromBackpack.ConsumeCraftingItem` ([ADVENTUREBACKPACKS-V](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-V)).
* **Codebase Cleanliness & Ground Rules Enforcement**:
  * Removed obsolete summary blocks (`/// <summary>`) and paragraph comments across `Features/CraftFromBackpack.cs`, `Features/StoreToBackpack.cs`, and `Extensions/InventoryExtensions.cs`.

# 2.1.4 - Container Proxy Compatibility & Defensive Null Safety
* **External Storage Mod Compatibility & Proxy Virtualization (`Patches/Container.cs`, `Components/BackpackComponent.cs`, `Extensions/PlayerExtensions.cs`)**:
  * Added `[HarmonyPriority(Priority.First)]` to `ContainerSetInUsePatch` on `Container.SetInUse(bool)`. Ensures Adventure Backpacks' intercept runs before third-party container management mods (specifically `AzuAutoStore.Patches.ContainerSetInUseClearWithoutOwnershipPatch`), suppressing the call chain and preventing `NullReferenceException` when closing or updating the backpack container.
  * In `PlayerExtensions.GetBackpackContainerProxy()`, assigned `_backpackProxyContainer.m_nview = player.m_nview` and changed default proxy container name from generic `$piece_container` to `"Backpack"`. Ensures any external mod inspecting `container.m_nview` evaluates cleanly against the player's network view without null dereferences.
  * In `BackpackComponent.UpdateContainerSizing()`, dynamically synchronized `backpackContainer.m_name = Item?.m_shared?.m_name ?? inventory.GetName()`, so open backpack containers accurately identify by their specific backpack item name rather than a generic piece name.
* **Defensive Null Safety & Exception Guards (`Patches/Humanoid.cs`, `Patches/InventoryGui.cs`, `Patches/Player.cs`, `Features/CraftFromBackpack.cs`)**:
  * Added comprehensive `try/catch` handlers and null guards in `HumanoidPatches.UnequipItemPrefix` and `EquipItemPostfix` around scene verification and proxy destruction.
  * Added defensive null checks and `try/catch` error trapping in `InventoryGuiPatches.HideBackpack()`, `ShowBackpack()`, and `DetectInputToHide()` / `DetectInputToShow()` to prevent UI update failures.
  * Added null validation in `PlayerPatches.ConsumeUnEquippedItems` and `CraftFromBackpack.ConsumeCraftingItem` preventing null dereferencing when searching or consuming crafting materials across inventories.

# 2.1.3 - Auto-Store Item Data & Stack Space Null Safety
* **Defensive Stack Space Scanning (`Extensions/InventoryExtensions.cs`, `Features/StoreToBackpack.cs`)**:
  * Implemented `SafeFindFreeStackSpace(this Inventory, string, float)` extension method to replace vanilla `Inventory.FindFreeStackSpace`. Vanilla `FindFreeStackSpace` iterates over `m_inventory` without null-checking items or `item.m_shared`, throwing `NullReferenceException` ([ADVENTUREBACKPACKS-W](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-W)) if an external container or inventory management mod leaves an uninitialized slot or null entry in the backpack inventory.
  * Replaced all calls to `FindFreeStackSpace` in `StoreToBackpack.ShouldStoreToBackpack`, `StoreToBackpack.CanInventoryAccept`, and `StoreToBackpack.TryStoreItem` with `SafeFindFreeStackSpace`.
* **Defensive Item Presence Scanning (`Extensions/InventoryExtensions.cs`, `Features/StoreToBackpack.cs`, `Patches/Player.cs`, `Patches/Door.cs`)**:
  * Implemented `SafeHaveItem(this Inventory, string)` extension method guarding against null items and uninitialized `m_shared` entries.
  * Updated `StoreToBackpack.ShouldStoreToBackpack` to use `backpackInventory.SafeHaveItem` when evaluating whether to auto-store.
  * Updated `PlayerPatches.PlayerHaveRequirementsPatch` to use `SafeHaveItem` when checking crafting requirements against player and backpack inventories.
  * Updated `DoorPatches.HaveDoorKeyPatch` to safely evaluate key availability with null-propagation and `SafeHaveItem`.

# 2.1.2 - Player Container Isolation & Ecosystem Compatibility
* **Player Root Container De-Pollution (`Patches/Player.cs`, `Patches/Humanoid.cs`)**:
  * Removed `PlayerAwakePatch` which added a `Container` component directly to `Player.gameObject`.
  * Eliminates component pollution on the root player entity, preventing external container mods (ValheimPlus, AzuCraftyBoxes, ItemDrawers, etc.) and vanilla systems from misidentifying the player character as a world chest or piece container.
* **Dedicated Child Proxy Architecture (`Extensions/PlayerExtensions.cs`)**:
  * Created `GetBackpackContainerProxy(this Player)` to host the `Container` component exclusively on a dedicated child GameObject (`AB_BackpackProxy`) parented under `player.transform`.
  * Added transform hierarchy verification (`_backpackProxyContainer.transform.parent == player.transform`) to ensure stale proxy instances are discarded across player deaths, respawns, or character switches.
  * Added `DestroyBackpackContainerProxy(this Player)` to cleanly destroy the proxy GameObject and reset references upon unequipping backpacks or player death (`Player.UnequipDeathDropItems`).
* **Container Proxy Virtualization Patches (`Patches/Container.cs`)**:
  * Added `ContainerSetInUsePatch` on `Container.SetInUse(bool)` to suppress vanilla execution for `IsBackpackProxy()`, preventing `NullReferenceException` crashes caused by `m_nview.IsOwner()` on proxies without a `ZNetView` during `InventoryGui.UpdateContainer()` and `InventoryGui.CloseContainer()`.
  * Added `ContainerTakeAllPatch` on `Container.TakeAll(Humanoid character, ref bool __result)` matching vanilla parameter names, delegating item transfers from backpack to player inventory, and setting `__result = true;`.
  * Added `ContainerLoadPatch` on `Container.Load(ref bool __result)` and `ContainerSavePatch` on `Container.Save()` to prevent attempts to serialize or deserialize backpack proxy inventories into the player ZDO.
  * Added `ContainerAwakePatch` on `Container.Awake()` to suppress RPC registrations and repeating `CheckForChanges` polling on the proxy.
  * Added `ContainerIsOwnerPatch` (`__result = true;`), `ContainerIsInUsePatch` (`__result = false;`), `ContainerCheckAccessPatch` (`__result = true;`), and `ContainerStackAllPatch` to cleanly virtualize container UI behaviors.

# 2.1.1 - Crafting & Container Mod Compatibility
* **Cooperative, Non-Destructive Crafting Requirement Hooks (`Patches/Player.cs`)**:
  * Refactored `PlayerHaveRequirementsPatch` on `Player.HaveRequirements(Piece, Player.RequirementMode)` from a destructive Harmony Prefix (`return false`) to a cooperative, additive `[HarmonyPostfix]`. If vanilla or external container mods (such as **ValheimPlus `CraftFromChest`** or **ItemDrawers**) already satisfy the piece requirement (`__result == true`), the postfix exits immediately without interference. Only when `__result == false` does it check if the equipped backpack inventory satisfies any remaining shortfall.
  * Refactored `PlayerGetFirstRequiredItemPatch` on `Player.GetFirstRequiredItem` from an unconditional destructive Prefix (`return false`) to a cooperative `[HarmonyPostfix]`. If vanilla or an external mod resolves a matching ingredient (`__result != null`), it yields immediately; otherwise, it searches the equipped backpack inventory as a fallback.
  * Refactored `PlayerHaveRequirementItemsPatch` for single-ingredient recipes (`m_requireOnlyOneIngredient`) from a destructive Prefix to a cooperative `[HarmonyPostfix]` that only intervenes when `__result == false`.
* **Crafting Item Consumption & Sentry Bugfix (`Features/CraftFromBackpack.cs` & `Patches/Player.cs`)**:
  * Fixed `NullReferenceException` in `CraftFromBackpack.ConsumeCraftingItem` ([ADVENTUREBACKPACKS-V](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-V)) by adding defensive `x != null && x.m_shared != null` guards and safe `string.Equals` checks across all inventory and backpack item LINQ queries.
  * Added matching null-safety checks in `PlayerPatches.ConsumeUnEquippedItems` and `PlayerPatches.AdjustCountIfEquipped`.
* **Auto Store Item Data Null Safety (`Features/StoreToBackpack.cs`, `Assets/Backpacks.cs`, `Extensions/ItemDataExtensions.cs`)**:
  * Fixed `NullReferenceException` in `StoreToBackpack.TryStoreItem` ([ADVENTUREBACKPACKS-W](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-W)) by adding comprehensive null checks in `ItemDataExtensions.IsBackpack`, `Backpacks.TryGetBackpackItem`, and `StoreToBackpack.TryStoreItem` before querying `m_shared` properties or stack sizes.
  * Added null-safe empty slot calculation guarding against null `backpackInventory.m_inventory`.
* **Equipment & Scene Transition Guards (`Patches/Humanoid.cs`)**:
  * Fixed `NullReferenceException` in `Humanoid.UnequipItem` and `Humanoid.EquipItem` ([ADVENTUREBACKPACKS-S](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-S)) by verifying `__instance == Player.m_localPlayer` before executing player-specific backpack unequip operations and guarding scene name checks.
* **Community Translation Error Handling (`Assets/Items/BackpackItem.cs`)**:
  * Wrapped localization translation fetching in `SafeGetTranslation` to catch unhandled `SemanticErrorException` crashes ([ADVENTUREBACKPACKS-A](https://vapok-gaming.sentry.io/issues/ADVENTUREBACKPACKS-A)) caused by malformed community YAML/JSON translation files.
* **Inventory Crafting Consumption Coordination (`Patches/Inventory.cs`)**:
  * Refactored `RemoveItemByNamePatch` on `Inventory.RemoveItem(string, int, int, bool)` to remove `[HarmonyPriority(Priority.First)]` and allow partial consumption delegation. If `ConsumeCraftingItem` cannot completely fulfill the required amount, `amount` is updated to `remaining` and the call proceeds to vanilla/external container managers (`return true`) instead of destructively swallowing the removal.

# 2.1.0 - Craft From Backpack & Auto Store To Backpack
* **Craft From Backpack (`Features/CraftFromBackpack.cs`)**:
  * Implemented `CraftFromBackpack` feature allowing placeable building (hammer) and crafting stations to consider items in the equipped backpack.
  * Added Harmony Prefix on `Player.HaveRequirements(Piece, Player.RequirementMode)` to include equipped backpack inventory items when checking building requirements.
  * Updated `PlayerPatches.AdjustCountIfEquipped` to add items present in the equipped backpack inventory (supporting quality-specific checks), feeding directly into existing `Player.HaveRequirementItems` and `InventoryGui.SetupRequirement` transpilers so recipe requirements and HUD counts reflect backpack inventory.
  * Updated `PlayerPatches.ConsumeUnEquippedItems` and `CraftFromBackpack.ConsumeCraftingItem` to pull required resources from Player inventory first (skipping equipped items), then pull any remaining needed amount from the equipped backpack inventory, returning 0 to vanilla `Player.ConsumeResources`.
  * Updated `PlayerPatches.PlayerGetFirstRequiredItemPatch` to check equipped backpack inventory as a fallback for single-ingredient recipes.
  * Added Harmony Prefix on `Inventory.RemoveItem(string, int, int, bool)` during active crafting (`IsDoingCrafting`) to route through `CraftFromBackpack.ConsumeCraftingItem`.
  * Added server-synced configurations: `Enable Craft From Backpack` and `Enable Craft Output To Backpack` under `Server Config`.
* **Auto Store To Backpack (`Features/StoreToBackpack.cs`)**:
  * Implemented `StoreToBackpack` feature allowing picked up, looted, or gained items to store automatically into the equipped backpack if the backpack already has $\ge 1$ of that item and has available space.
  * Added overflow protection: if the player inventory is full and the equipped backpack is empty or has space, incoming items automatically store into the backpack instead of triggering "Inventory Full" (`$msg_noroom`) feedback.
  * Added Harmony Prefix on `Inventory.CanAddItem(ItemDrop.ItemData, int)` on the player inventory to return `true` when `StoreToBackpack.ShouldStoreToBackpack` allows the item or when crafting output overflows into the backpack, enabling `Player.AutoPickup`, container loot-all, and crafting station creation when player inventory is full.
  * Updated `InventoryPatches.AddItemPatch` on `Inventory.AddItem(ItemDrop.ItemData)` to route eligible items into `StoreToBackpack.TryStoreItem`, supporting partial stack storage and falling back to player inventory for remainders or when backpack space is exhausted.
  * Safeguards implemented: strictly excludes backpacks (preventing backpack inception), skips when moving items between containers or when `BackpackIsOpen` in `InventoryGui`, and bypasses during yard sales or quick dropping.
  * Added server-synced configurations: `Enable Auto Store to Backpack` and `Enable Inventory Overflow To Backpack` under `Server Config`.

# 2.0.10 - Fix Recipe Upgrades & Backpack Status Effects
* **Backpack Status Effects & Frost Resistance Initialization (`BackpackComponent` & `FrostResistance`)**:
  * In `BackpackComponent.FirstLoad()` and `BackpackComponent.Load()`, added explicit calls to `Backpacks.UpdateStatusEffects(Item)` when instantiating a new backpack inventory. This ensures that `item.m_shared.m_equipStatusEffect`, armor calculations, carry weight bonuses, speed modifiers, and Troll set bonuses are initialized immediately upon crafting or spawning rather than waiting for a subsequent `Deserialize()` event on game reload.
  * In `FrostResistance.LoadExternalStatusEffect()`, assigned `FrostResistance.EffectMod` (`HitData.DamageModifier.Resistant` for Frost) directly to `SE_vapok_ab_frost_resistance`'s `m_mods`. This guarantees that the buff status effect itself directly grants Frost resistance to the player via `m_seman`, eliminating any desync between the HUD buff icon and actual environmental protection against the Freezing debuff.
* **Vanilla Crafting & Upgrade Fixes (`m_requireOnlyOneIngredient`)**:
  * Added Harmony Prefix on `Player.HaveRequirementItems`:
    * When `recipe.m_requireOnlyOneIngredient` is enabled, explicitly filters out and skips requirements where `neededAmount <= 0` for that quality tier.
    * Resolves vanilla issue where $0 \ge 0$ false-positive match on 0-cost base items (e.g. `CapeFeather` on higher tier upgrades) caused the Craft/Upgrade button to illuminate without actual materials in inventory.
  * Added Harmony Prefix on `Player.GetFirstRequiredItem`:
    * Skips requirements where `neededAmount <= 0`.
    * Iterates item quality starting from valid quality $q = 1$ through `maxQuality` (vanilla started at $j = 0$ returning `null`).
    * Selects unequipped items to prevent equipped gear from being consumed.
  * Added Harmony Prefix on `Recipe.GetAmount`:
    * Added null-safety guard for `singleReqItem != null` prior to calculating quality bonus multipliers, preventing unhandled `NullReferenceException` crashes.
* **Bugfixes & Null Safety Guards**:
  * In `BackpackItem.cs`, added defensive null-conditional checks on Valheim's `Localization.instance` and `Localization.m_instance` during early startup to prevent `NullReferenceException` before game localization is initialized.
  * In `BackpackItem.cs`, imported `Vapok.Common.Shared` to bind directly to internal `ConfigurationManagerAttributes`, preventing runtime `MissingFieldException` on `Category` when third-party configuration managers are present.
  * Added null validation on `Item` before setting `Item.SectionName` in `SetupBackpackDef()`.

# 2.0.9 - General Bugfixes and Splash Window Updates
* **Bugfixes & Null Safety Guards**:
  * Added defensive null checks in `AssetItem` across `GetItemDrop()`, `RegisterShaderSwap()`, `SetPersistence()`, and `ResetPrefabArmor()` to prevent `NullReferenceException` and `ArgumentNullException` during early asset instantiation.
  * Added null validation and defensive `try/catch` error handling in `PlayerExtensions.OpenBackpack()` when retrieving and attaching `Container` components on the player `GameObject`.
  * Guarded `PlayerPatches.AwakePostfix` with defensive `GetComponent<Container>()` checks and `try/catch` handlers.
  * Added defensive exception handling to `InventoryGuiPatches.CheckForTextInput()`.
* **Configuration & Error Reporting**:
  * Added `Send Error Reports` configuration setting (`ConfigRegistry.SendErrorReports`), defaulting to enabled (Opt-Out).
  * Set `Enable Anonymous Telemetry` default to false (Opt-In).
  * Registered `SendErrorReports` with `ModSplashManager`.
* **Vapok.Common Dependency Bump**:
  * Updated internalized dependency to `Vapok.Valheim.Common` 3.12.1014 bringing in-game scrollable Privacy Policy overlay, multi-language localization, and hover data disclaimers.

# 2.0.8 - Jewelcrafting Font Compatibility
* **Compatibility Fix**: Fixed issue where Jewelcrafting packages its own font which was overriding part of a vanilla font, causing the Splash screen to appear blank.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.11.1012.

# 2.0.7 - Updated README with Telemetry Information
* **Documentation Update**: Updated the README.md with Anonymous Telemetry and Privacy section per request of mod stores.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.9.1012.

# 2.0.6 - Unified Splash Screen & Telemetry Controls
* **ModSplashManager Integration**: Registered Adventure Backpacks with the unified Vapok startup splash screen, including tagline and localization support.
* **Anonymous Telemetry Configuration**: Added local configuration bindings (`Show Splash on Startup`, `Enable Anonymous Telemetry`) to manage opt-in anonymous startup and session reporting.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.5.1012.

# 2.0.4 - Container Mod Compatibility & Item Duplication Fix
* Fixed: Item duplication and inventory reset issue when building or crafting with container mods (e.g. AzuCraftyBoxes).
  * Resolved an inventory instance desynchronization where `FirstLoad()` and `Deserialize()` instantiated separate `Inventory` objects.
  * Synchronized the player's equipped container inventory with `_backpackInventory` across loading, deserializing, and resizing.
  * Added fallback reconciliation in `InventoryChangedPatch` to ensure modifications to the equipped container inventory save directly to the backpack's data.
  * Prevented vanilla `Container.Save()` and `Container.Load()` from overwriting or corrupting the player character's network ZDO data.
* Fixed: Missing null check when resizing backpack containers during quality and dimension validation.
* Removed redundant inventory loading calls during resize operations.

# 2.0.3 - Fix API DLL Size
* Fixed: `AdventureBackpacksAPI.dll` size bloat by excluding asset bundles and mod-only source files from API build configuration.

# 2.0.2 - Transpiler Resilience & Mod Compatibility Update
* Overhauled Transpilers for `Player.HaveRequirementItems`, `Player.ConsumeResources`, and `InventoryGui.SetupRequirement`:
  * Migrated requirement counting and resource consumption logic to direct evaluation-stack interception.
  * Dynamically resolves requirement locals from IL rather than relying on strict opcode positions or hardcoded indices.
  * Fixes transpiler patching failures and crashes when running alongside mods that inject crafting/inventory calculation logic (e.g. Valheim Plus CraftFromChest, AzuCraftyBoxes, AzuAutoStore, EpicLoot).
* Made `Humanoid.UpdateEquipmentStatusEffects` transpiler resilient to opcode variations (`Stloc_0`, `Stloc_S`, `Stloc`).
* Added defensive null checks across requirement parsing and unequipped item consumption helpers.

# 2.0.1 - ZenDragon ModLib Compatibility
* Added in defensive transpiler checks to ensure compatibility with ZenDragon's MobLib

# 2.0.0 - Valheim 1.0+ Adventure Backpacks
* Updated Adventure Backpacks for 2.0.0
  * Updated All Transpilers and Harmony References for Valheim 1.0
  * Fixed: Bug on Drops were not taking into account World Scaling and Level/Star rating of creatures.
    * Enhancements:
      * Two New Config Settings
        * Adjust Drops by Level Factor
        * Adjust Drops by World Scaling Factor
  * Tested Extensively without other mods.
    * I DO NOT KNOW IF THERE ARE COMPATIBILITY ISSUES WITH OTHER MODS BUT PLEASE LET ME KNOW <3 XOXO.
  * No New Content in this version, this is to get AB working quickly with 1.0
    * I see you Ashlands. There will be more bags, but wanted to get the initial 1.0 out to get you all rolling.
  * Please inform me of any errors or issues to my GitHub or Discord and I'll respond quickly.

# 1.9.13 - Refreshed Drop Lists & Bugfixes
* Refreshed Drop Lists for All Backpacks
  * All Biome Bosses through Plains now have a drop change by default.
  * Bears, Viles, and other newly introduced mobs have been added to drop lists.
* Fixed: InventoryGrid Index Error when opening a backpack up for the first time upon game load.
  * I am not sure if this error was caused by Adventure Backpacks or another mod compatibility issue, as it seems random when the error occurs.
  * Additionally, swaping backpacks back and forth fixes the inventory grid issue.
  * I have implemented a fix that corrects the InventoryGrid regardless.
* Fixed: Container Prefix Patch incorrectly referencing ArmorStand
* Fixed: A number of areas where possible NRE's could occur.
* Fixed: Effect Registration was happening twice.
* Optimization: Optimized Effect Toggle Handling

# 1.9.12 - External References Updated and Item Duplication Guardrails
* Still getting a few random reports about item duplication, around the yard sale function.
  * I've implemented additional checks and guardrails to see if we can prevent this.
    * This could mean that Thor might strike you down after trying to put a backpack in a backpack, and then nothing happens (and the backpack doesn't move)
    * If you are getting duplication issues, please join my Discord and let me know. Log files are helpful.
* Updating to Vapok.Valheim.Common 2.11.22112
* Updating to Jotunn 2.27.1

# 1.9.11 - Valheim Version Maintenance - Compatibility Note
* Updated to Valheim Version 0.221.12
* ZenUI/ZenModLib Compatibility
  * ZenUI removes the Vanilla InventoryGui.Show() method
  * As a result, a change has been made, that if Show() method is missing, look further up.
    * If another mod removes the vanilla Player.ShowTutorial(), this fix is cooked.

# 1.9.10 - Additional Fixes
* Fixed: Issue when backpack size x or y get set to 0, causes game crash.
* Fixed: Item loss issue when upgrading non-equipped backpacks while having a backpack equipped.
* Fixed: Character Load Failures - Given Key '500' Not Present Key
* Bug Reports That Can't Be Reproduced:
  * Reports on Inventory Clearing on Teleport
    * Tested on XPortal, Advanced Portals, Vanilla Portals (normal and stone), all work for me.
    * Tested with ExtraSlots, and a number of other mods provided, all working..
  * If you come across something that is found, please join my Discord and let me know.

# 1.9.9 - Regression Issue on Backpack Equiping and Upgrades
* Last Update caused a regression issue on backup equiping and upgrading
  * This should fix it.
* Apologies on the many updates. Trying to push out quickly to prevent these stupid issues.
  * Also hopefully this slows down the updates.

# 1.9.8 - Fixing an Item Duping Issue
* Fixed: Item Duping during Backpack Resizing
* Changes
  * Updated Config Hint Text for Multipler. Lot of confusion on this.
    * Setting to 100% basically disables the weight reduction.
      * I see what you are doing out there.
      * Setting to 0% will give you the desired effect.
* API Changes 1.1.0 -> 1.2.0
  * Updated API Dll to .NET 4.8

# 1.9.7 - Item Loss Issue - Update to Dedicated Server Strictness and Wisplight Config Changes
* It has been identified that major item data loss can occur if two clients are on a dedicated server, but only one client has Adventure Backpacks installed. The client without the mod can create dataloss, including the contents of a backpack.
  * To safeguard against this, I have enabled JVL Network Compatibility to enforce that all clients must use Adventure Backpacks.
  * This will require Adventure Backpacks, along with JVL and Yaml mods to be installed on dedicated servers.
  * Additionally all clients and servers must be using the same version as of this update.
* Reorganized Configuration Settings for Wisplight, now found under "Wisplight Client Settings" under the Effect
  * This is prep work for providing similar functionality later on for other effects.
  * This might result in resetting of your client config for these settings.
* Updates to ReadMe

# 1.9.6 - TIL Wisplights are Desired
* Added a new config setting called Wisplight Biome Logic
  * This can be disabled to allow the wisplight to be used in any biome
  * You're welcome schrodingerspsycho <3
* Adjusted Readme to highlight new settings.

# 1.9.5 - More Updates and Enhancements
* Explorers Wisppack Wisplight is Now Togglable
  * Set keybind in config, default is "L" for Light
  * Additionally, Wisplight will automatically be put away when not in Mistland's Biome's.
* Updated AssetBundles to Unity 6.0.0.0
* Revamped Managers to no longer look for ServerSync
  * This has moved all configuration now to Jotunn for managers now.
  * Note: This has reduced some of the "user friendly" fields in Configuration Manager (for now)
* The Trinket slot could be a lot of fun.

# 1.9.4 - Turkish, Upgraded Bag Upgrades, Localization, and Dependency Updates
* Added Turkish Translation thanks to Fahrim!
* Tired of Thor saving your bags?
  * Now upgrade bags with inventory without having your inventory explode.
* Completely revamped Localization System
  * Localizations are now working again.
  * Yes, they were broken. I'm sorry about that.
* Various Dependencies Updated to Current Versions

# 1.9.3 - Fixing Dedicated Server Config Syncing
* A regression issue was introduced when switching to Jotunn preventing servers from dictating configs to clients.
  * This has been resolved.
* Appropriately added the BepInDependency Flags for graceful mod exit if missing dependencies.

# 1.9.2 - Removed ServerSync - Updated to Jotunn
* 1.9.2 - Forgot to actually change Project References (good news it still worked!)
* 1.9.1 - Removed ServerSync and Updated to Jotunn for Config Management
* 1.9.0 - Updated for Valheim 0.221.4

# 1.7.10 - Updates for 0.219.16
* Resolved a Shader Compatibility issue with Blacks7ar's FeatherCollector
* Added a configuration setting to enable/disable the Shader Replacer (requires game restart)

# 1.7.9 - Additional Updates and Bug Fixes
* Updated Changes based on feedback from Pull Requests
* Fixed Bug that removed default effects from Backpacks
* Changed and updated default settings for Frost and Cold Resistances on Backpacks
  * You may need to reset your Cold Resist configs for Cold Resist to show back up on Lox Hide Backpack

# 1.7.8 - Forgot to increment the in-game version to 1.7.7
* In-game version now shows 1.7.8

# 1.7.7 - Updating for Ashland and Bog Witch Updates
* Updates to Valheim 0.219.14
* Updated ItemManager and Piece Manager

# 1.7.6 - Updating for 0.217.38 Valheim
* Fixed error on load.

# 1.7.5 - More Compatibility Support
* Added in Protection Override so that backpacks can be placed on an Item Stand, similar to Armor Stands.
* Added initial ContentsWithin support to prevent backpacks from being previewed. 

# 1.7.4 - One more Drop Bug
* Fixed: Drops were being enabled on world start.

# 1.7.3 - Fixed Configuration Issue
* Fixed the ItemManager issue and now running on new version.
* Drops tested and working as expected.

# 1.7.2 - Fixing Drop Issues
* Reverting to a previous version of ItemManager until I can understand why drops are not working fully.
  * If you having continued issues with drops after updating to this update, you might have reset your Adventure Backpacks configuration file.

# 1.7.1 - Release of the Adventure Backpacks API - Effect and Backpack Creation
* API (v1.1) now supports bringing in custom effects and backpacks
  * [Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
  * [Download ABAPI.DLL from GitHub](https://github.com/Vapok/AdventureBackpacks/releases)
* Cheb's Necromany Backpack Integration converted to API
* Updated to 0.218.28 Valheim
* Upgraded to .net 4.7.2

# 1.7.0 - Release of the Adventure Backpacks API
* Initial Release of the ABAPI.
  * [Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
  * [Download ABAPI.DLL from GitHub](https://github.com/Vapok/AdventureBackpacks/releases)
* Valheim Update 0.217.27
* Upgraded to .net 4.7.2

# 1.6.29 - Multiplayer Issue
* Fixing ServerSync

# 1.6.28 - 0.217.24 Update
* Updates for Valheim 0.217.24

# 1.6.27 - Bug Fix
* Fixed: In a rare case, when a bag is upgraded, and then you die before unequipping your bag, it can potentially lose the contents of the backpack.
  * This is fixed in this version.

# 1.6.26 - Hotfix to address Critical Crashing Bug
* Turns out adding a container on Players, make them interactable which crashes the player object.
  * Disabled the interaction function on containers when the container is Player(Clone)

# 1.6.25 - Container Checking for Mod Compatibility
* Made changes will allow the Container to be recognized as "player built"
  * This is needed to prevent other mods from having to make specific updates for this mod.
  * AzuCraftyBoxes is now fully supported with Adventure Backpacks when using the Equipped Backpack
  * Craft From Containers needs 1 update in order to work (They need to not check for Piece component)
* Fixes a Compatibility issue with Valheim+ Where Transpilers were fighting for attention.

# 1.6.24 - Updates and Compatibilities
* Fixing a Player Load error on Startup when wearing a backpack.

# 1.6.23 - Updates and Compatibilities
* Fixed the Inventory Input Control that was broken.
* Added in logging and warning messages when Transpilers don't patch.
* Added in Container compatibility for mods (like AzuCraftyBoxes) that would access containers. (GitHub Issue #110)
  * This places a Container component on the Player(Clone) that will always contain the inventory of the EQUIPPED backpack
  * This would allow Craft from Containers (assuming no code changes needed on other mods) to access that inventory.
* Added Configuration to Show/Hide the Backpack Status Effect (GitHub Issue #104)
* Added Configuration to give Backpack Status Effect a Custom Name (GitHub Issue #104)

# 1.6.22 - Hildir's Bug Fixing
* Fixes Item Requirement Count Method - Missed 1 method.

# 1.6.21 - Hildir's Bug Fixing
* Fixes Item Requirement Count Method
* Updated Russian Translations

# 1.6.20 - Valheim 0.217.14 Update
* Implements updates needed for Valheim Update Hildir's Request

# 1.6.19 - Valheim 0.216.9 Update
* Implements updates needed for Valheim Update 0.216.9
* Adds in Polish Translations (big thanks to Gryfu and rysson for the collaboration and pull requests)

# 1.6.18 - Cheb's Necromancy Hotfix
* Fixes Cheb's Necromancy Asset Issue
* Put in strong error handling for when this will happen again in the future.

# 1.6.17 - MaxAxe Compatibility Hotfix
* Changes to crafting were assuming unstackable items.
  * Fixed to allow stackable items (that can be equipped) to be removed correctly.

# 1.6.16 - Various Updates
* Crafting bags will no longer allow you to consume equipped cape's.
  * To craft a bag with a cape, and the only one in inventory is equipped, it must be unequipped in order for it to be used.
* Scaled down size and repositioned the Explorer's Wisppack.
* Updated Adventure Backpacks Unity version to 2020.3.45
* Updates codebase to 0.214.300 Valheim References
* Adding CHANGELOG.md to Thunderstore package

# 1.6.15.0 - Controller Support!!!, also some bug fixes.
* Fully Implemented Controller/Gamepad Support.
  * Set bindings in configuration for opening up the backpack and other settings.
* Rebuilt the mechanism for calculating backpack weight. Now uses transpiler.
* Addition Sign and Tame Rename Interaction Fixes
* Open Backpack With Hover now fully works on extended inventory and quick slot grids.
* Open with Inventory now work with Open/Close Backpack with Hover.
