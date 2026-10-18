using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
#if ! API
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using AdventureBackpacks.Patches;
#endif

namespace AdventureBackpacks.API;

/// <summary>
/// Adventure Backpacks developer API.
/// 
/// IMPORTANT FOR MOD DEVELOPERS &amp; AI AGENTS:
/// - INVARIANT: Exactly ONE backpack may be equipped at any time per player. Multi-equipping is forbidden.
/// - DO NOT use reflection (AccessTools, Traverse) or Harmony patches on internal classes (BackpackManager, etc.).
/// - For soft integrations, copy Docs/ABAPI_Client.cs into your project.
/// - If a needed capability is missing, submit a feature request at https://github.com/Vapok/AdventureBackpacks/issues
///   or join the Discord at https://discord.gg/5YAJkRFBXt.
/// 
/// Integration methods:
/// 1. Zero-Dependency Client Wrapper (Recommended for soft integrations):
///    Drop [Docs/ABAPI_Client.cs](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs) into your mod project.
/// 2. Repackable API Assembly:
///    Reference [AdventureBackpacksAPI.dll](https://github.com/Vapok/AdventureBackpacks/releases) and merge into your mod via ILRepack.
/// </summary>
[PublicAPI]
// ReSharper disable once InconsistentNaming
public partial class ABAPI
{
    /// <summary>
    /// Notifies if the ABAPI is active or not.
    /// </summary>
    /// <returns>true of false</returns>
    public static bool IsLoaded()
    {
#if ! API
        return true;
#else
return false;
#endif
    }

    /// <summary>
    /// When provided with an ItemData object, will detect whether the Item is an Adventure Backpack or not.
    /// </summary>
    /// <param name="itemData">This is the ItemDrop.ItemData object of the item.</param>
    /// <returns>true or false</returns>
    public static bool IsBackpack(ItemDrop.ItemData itemData)
    {
#if ! API
        return itemData != null && itemData.IsBackpack();
#else
return false;
#endif
    }


    /// <summary>
    /// Determines if the Player provided is currently wearing a backpack.
    /// Exactly ONE backpack may ever be equipped at any time per player.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <returns>true or false</returns>
    public static bool IsBackpackEquipped(Player player)
    {
#if ! API
        return player != null && player.IsBackpackEquipped();
#else
return false;
#endif
    }

    /// <summary>
    /// Determines if the player is capable of currently opening the equipped backpack.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <returns>true or false</returns>
    public static bool CanOpenBackpack(Player player)
    {
#if ! API
        return player != null && player.CanOpenBackpack();
#else
return false;
#endif
    }

    /// <summary>
    /// Indicates whether the backpack container is currently open in the local player's InventoryGui.
    /// </summary>
    /// <returns>True if open, otherwise false.</returns>
    public static bool IsBackpackOpen()
    {
#if ! API
        return InventoryGuiPatches.BackpackIsOpen;
#else
        return false;
#endif
    }

    /// <summary>
    /// Determines if the player provided is wearing the item provided and that it's a backpack.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="itemData">Any ItemData</param>
    /// <returns>true or false. If item provided is not a backpack, will return false.</returns>
    public static bool IsThisBackpackEquipped(Player player, ItemDrop.ItemData itemData)
    {
#if ! API
        return player != null && player.IsThisBackpackEquipped(itemData);
#else
return false;
#endif
    }

    /// <summary>
    /// Returns a Backpack object if the provided Player is currently wearing a backpack.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <returns>Nullable Backpack Object</returns>
    public static Backpack? GetEquippedBackpack(Player player)
    {
#if ! API
        var backpackComponent = player.GetEquippedBackpack();
        return ConvertBackpackItem(backpackComponent);
#else
return null;
#endif
    }

    /// <summary>
    /// Returns Backpack object of the provided itemData. Operates similarly to a TryGet but with a nullable type.
    /// </summary>
    /// <param name="itemData">ItemDrop.ItemData object</param>
    /// <returns>Nullable Backpack Object. Check HasValue.</returns>
    public static Backpack? GetBackpack(ItemDrop.ItemData itemData)
    {
#if ! API
        return ConvertBackpackItem(itemData);
#else
return null;
#endif
    }

    /// <summary>
    /// Retrieves the Inventory of the equipped backpack on the specified player.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <returns>Inventory object, or null if no backpack equipped</returns>
    public static Inventory GetEquippedBackpackInventory(Player player)
    {
#if ! API
        if (player == null)
            return null;
        var component = player.GetEquippedBackpack();
        return component?.GetInventory();
#else
        return null;
#endif
    }

    /// <summary>
    /// Tries to retrieve the Inventory of the equipped backpack on the specified player.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="inventory">Out parameter for the backpack inventory</param>
    /// <returns>true if backpack equipped and inventory exists, otherwise false</returns>
    public static bool TryGetEquippedBackpackInventory(Player player, out Inventory inventory)
    {
        inventory = GetEquippedBackpackInventory(player);
        return inventory != null;
    }

    /// <summary>
    /// Retrieves the Inventory of a backpack from an ItemDrop.ItemData object.
    /// </summary>
    /// <param name="itemData">ItemDrop.ItemData object</param>
    /// <returns>Inventory object, or null if not a backpack or uninitialized</returns>
    public static Inventory GetBackpackInventory(ItemDrop.ItemData itemData)
    {
#if ! API
        if (itemData == null)
            return null;
        var backpack = GetBackpack(itemData);
        return backpack?.Inventory;
#else
        return null;
#endif
    }

    /// <summary>
    /// Tries to retrieve the Inventory of a backpack from an ItemDrop.ItemData object.
    /// </summary>
    /// <param name="itemData">ItemDrop.ItemData object</param>
    /// <param name="inventory">Out parameter for the backpack inventory</param>
    /// <returns>true if item is a backpack and inventory exists, otherwise false</returns>
    public static bool TryGetBackpackInventory(ItemDrop.ItemData itemData, out Inventory inventory)
    {
        inventory = GetBackpackInventory(itemData);
        return inventory != null;
    }

    /// <summary>
    /// Retrieves the current Active Backpack StatusEffects running in the local players game.
    /// </summary>
    /// <returns>HashSet of Status Effects.</returns>
    public static HashSet<StatusEffect> GetActiveBackpackStatusEffects()
    {
#if ! API
        return EquipmentEffectCache.ActiveEffects;
#else
return null;
#endif
    }

    /// <summary>
    /// Retrieves all Status Effects Registered with Adventure Backpacks
    /// </summary>
    /// <returns>HashSet of Status Effects.</returns>
    public static HashSet<StatusEffect> GetRegisterdStatusEffects()
    {
#if ! API
        return EffectsFactory.GetRegisteredEffects();
#else
return null;
#endif
    }

    /// <summary>
    /// Method to activate the backpack on the local player's GUI and open it. Use in conjunction with CanOpenBackpack()
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="gui">The instance of InventoryGui</param>
    public static void OpenBackpack(Player player, InventoryGui gui)
    {
#if ! API
        if (player != null)
            player.OpenBackpack(gui);
#endif
    }

    /// <summary>
    /// Method to activate the backpack on the local player's GUI with a specific active UI group.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="gui">The instance of InventoryGui</param>
    /// <param name="activeGroup">The UI group index to retain active focus (e.g. 3 for crafting station, 1 for inventory).</param>
    public static void OpenBackpack(Player player, InventoryGui gui, int activeGroup)
    {
#if ! API
        if (player != null)
            player.OpenBackpack(gui, activeGroup);
#endif
    }

    /// <summary>
    /// Use this method in the Awake() of your mod to register a Status Effect that can be utilized on Adventure Backpacks
    /// </summary>
    /// <param name="effectDefinition">Create a new EffectDefinition that contains the overall parameters that are needed to register the new effect.</param>
    public static void RegisterEffect(EffectDefinition effectDefinition)
    {
#if ! API
        EffectsFactory.RegisterExternalEffect(effectDefinition);
#endif
    }

    /// <summary>
    /// Use this method in the Awake() of your mod to register a new Backpack that can be utilized on Adventure Backpacks.
    /// </summary>
    /// <param name="definition">Create a new BackpackDefinition that contains the overall parameters that are needed to register the new backpack.</param>
    public static void RegisterBackpack(BackpackDefinition definition)
    {
#if ! API
        BackpackFactory.RegisterExternalBackpack(definition);
#endif
    }

    public static bool IsCraftFromBackpackEnabled()
    {
#if ! API
        return CraftFromBackpack.EnableCraftFromBackpack?.Value ?? false;
#else
        return false;
#endif
    }

    /// <summary>
    /// Indicates whether opening the backpack alongside crafting stations is enabled in configuration.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsOpenWithCraftingStationEnabled()
    {
#if ! API
        return ConfigRegistry.OpenWithCraftingStation?.Value ?? false;
#else
        return false;
#endif
    }

    /// <summary>
    /// Indicates whether opening the backpack with the inventory is enabled in configuration.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsOpenWithInventoryEnabled()
    {
#if ! API
        return ConfigRegistry.OpenWithInventory?.Value ?? false;
#else
        return false;
#endif
    }

    /// <summary>
    /// Indicates whether toggling between an opened container and backpack via tabs is enabled in configuration.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsEnableContainerTabsEnabled()
    {
#if ! API
        return ConfigRegistry.EnableContainerTabs?.Value ?? false;
#else
        return false;
#endif
    }

    /// <summary>
    /// Indicates whether an external container is currently open and managed alongside the backpack.
    /// </summary>
    /// <returns>True if an external container is active with container tabs, false otherwise.</returns>
    public static bool HasActiveExternalContainer()
    {
#if ! API
        return ContainerTabs.HasActiveExternalContainer;
#else
        return false;
#endif
    }

    public static bool IsCraftingContextActive()
    {
#if ! API
        return CraftingContext.IsActive;
#else
        return false;
#endif
    }

    public static string GetConsumptionPriority()
    {
#if ! API
        return CraftFromBackpack.MaterialConsumptionPriority?.Value.ToString() ?? "PlayerInventoryFirst";
#else
        return "PlayerInventoryFirst";
#endif
    }

    public static bool IsLeaveOneItemInBackpackEnabled()
    {
#if ! API
        return CraftFromBackpack.LeaveOneItemInBackpack?.Value ?? false;
#else
        return false;
#endif
    }

    /// <summary>
    /// Indicates whether requirement counts in the crafting panel display as 'Available/Required'.
    /// </summary>
    /// <returns>True if enabled, false otherwise.</returns>
    public static bool IsDisplayTotalIngredientCountEnabled()
    {
#if ! API
        return CraftFromBackpack.DisplayTotalIngredientCount?.Value ?? false;
#else
        return false;
#endif
    }

    public static void SuppressNativeCrafting(string modIdentifier)
    {
#if ! API
        CraftingContext.Suppress(modIdentifier);
#endif
    }

    public static void UnsuppressNativeCrafting(string modIdentifier)
    {
#if ! API
        CraftingContext.Unsuppress(modIdentifier);
#endif
    }

    /// <summary>
    /// Retrieves all distinct backpack inventories associated with the player,
    /// including equipped backpacks (handling custom equipment slots) and backpacks carried in the player's inventory.
    /// </summary>
    /// <param name="player">The player to inspect, usually Player.m_localPlayer.</param>
    /// <returns>A list of unique backpack Inventory instances.</returns>
    public static List<Inventory> GetAllBackpackInventories(Player player)
    {
#if ! API
        List<Inventory> inventories = new List<Inventory>();
        if (player == null)
            return inventories;

        HashSet<Inventory> seen = new HashSet<Inventory>();

        Inventory equippedInv = GetEquippedBackpackInventory(player);
        if (equippedInv != null && seen.Add(equippedInv))
        {
            inventories.Add(equippedInv);
        }

        Inventory playerInv = player.GetInventory();
        if (playerInv != null)
        {
            foreach (ItemDrop.ItemData item in playerInv.GetAllItems())
            {
                if (TryGetBackpackInventory(item, out Inventory bagInv) && bagInv != null && seen.Add(bagInv))
                {
                    inventories.Add(bagInv);
                }
            }
        }

        return inventories;
#else
        return new List<Inventory>();
#endif
    }

    /// <summary>
    /// Tries to retrieve all distinct backpack inventories associated with the player.
    /// </summary>
    /// <param name="player">The player to inspect.</param>
    /// <param name="inventories">Output list containing all found backpack inventories.</param>
    /// <returns>True if at least one backpack inventory was found; otherwise false.</returns>
    public static bool TryGetAllBackpackInventories(Player player, out List<Inventory> inventories)
    {
        inventories = GetAllBackpackInventories(player);
        return inventories != null && inventories.Count > 0;
    }

    /// <summary>
    /// Determines whether Adventure Backpacks would store a picked up item into the equipped backpack of the specified player.
    /// Evaluates auto-store and inventory-overflow configurations, open backpack state, yard sale and quick-drop protection,
    /// anti-inception constraints, and available space in the equipped backpack without actually inserting the item.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="item">ItemDrop.ItemData candidate for pickup</param>
    /// <returns>True if the item would be accepted and stored into the equipped backpack, otherwise false.</returns>
    public static bool WouldStorePickup(Player player, ItemDrop.ItemData item)
    {
#if ! API
        return StoreToBackpack.ShouldStoreToBackpack(player, item, out _);
#else
        return false;
#endif
    }

    /// <summary>
    /// Determines whether Adventure Backpacks would store a picked up item into the equipped backpack of the specified player,
    /// and retrieves the target backpack inventory if accepted.
    /// Evaluates auto-store and inventory-overflow configurations, open backpack state, yard sale and quick-drop protection,
    /// anti-inception constraints, and available space in the equipped backpack without actually inserting the item.
    /// </summary>
    /// <param name="player">Player, usually Player.m_localPlayer</param>
    /// <param name="item">ItemDrop.ItemData candidate for pickup</param>
    /// <param name="backpackInventory">When this method returns true, contains the target backpack inventory; otherwise, null.</param>
    /// <returns>True if the item would be accepted and stored into the equipped backpack, otherwise false.</returns>
    public static bool WouldStorePickup(Player player, ItemDrop.ItemData item, out Inventory backpackInventory)
    {
#if ! API
        return StoreToBackpack.ShouldStoreToBackpack(player, item, out backpackInventory);
#else
        backpackInventory = null;
        return false;
#endif
    }

    /// <summary>
    /// Registers an AssetBundle override for a specific backpack and model style under a named Texture or Visual Pack.
    /// Enables companion mods to explicitly provide Modern and/or Classic visual assets.
    /// </summary>
    /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
    /// <param name="backpackId">The backpack identifier (e.g. "backpack_meadows", "backpack_black_forest").</param>
    /// <param name="modelStyle">The model style this bundle targets (Modern or Classic).</param>
    /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
    /// <param name="isDefault">Optional: If true and no player configuration is set, this pack will be the default selection for this backpack and style.</param>
    public static void RegisterVisualBundle(string packName, string backpackId, ModelStyle modelStyle, AssetBundle overrideBundle, bool isDefault = false)
    {
#if ! API
        VisualPackRegistry.Register(packName, backpackId, modelStyle, overrideBundle, isDefault);
#endif
    }

    /// <summary>
    /// Registers an AssetBundle override for a specific progression backpack and model style under a named Texture or Visual Pack.
    /// Enables companion mods to explicitly provide Modern and/or Classic visual assets using strongly-typed BackpackName.
    /// </summary>
    /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
    /// <param name="backpack">The strongly typed progression backpack identifier.</param>
    /// <param name="modelStyle">The model style this bundle targets (Modern or Classic).</param>
    /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
    /// <param name="isDefault">Optional: If true and no player configuration is set, this pack will be the default selection for this backpack and style.</param>
    public static void RegisterVisualBundle(string packName, BackpackName backpack, ModelStyle modelStyle, AssetBundle overrideBundle, bool isDefault = false)
    {
        RegisterVisualBundle(packName, backpack.ToBackpackId(), modelStyle, overrideBundle, isDefault);
    }

    /// <summary>
    /// Registers an AssetBundle override for a specific backpack model under a named Texture or Visual Pack.
    /// Enables companion mods and community texture packs to offer selectable visual overhauls.
    /// </summary>
    /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
    /// <param name="targetBundleName">The base bundle name to override (e.g. "backpack_meadows", "classic_backpack_black_forest").</param>
    /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
    public static void RegisterVisualBundle(string packName, string targetBundleName, AssetBundle overrideBundle)
    {
        RegisterVisualBundle(packName, targetBundleName, overrideBundle, false);
    }

    /// <summary>
    /// Registers an AssetBundle override for a specific backpack model under a named Texture or Visual Pack.
    /// Enables companion mods and community texture packs to offer selectable visual overhauls.
    /// </summary>
    /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
    /// <param name="targetBundleName">The base bundle name to override (e.g. "backpack_meadows", "classic_backpack_black_forest").</param>
    /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
    /// <param name="isDefault">Optional: If true and no player configuration is set, this pack will be the default selection for this bundle.</param>
    public static void RegisterVisualBundle(string packName, string targetBundleName, AssetBundle overrideBundle, bool isDefault)
    {
#if ! API
        VisualPackRegistry.Register(packName, targetBundleName, overrideBundle, isDefault);
#endif
    }

    /// <summary>
    /// Checks whether an asset bundle override is registered for the specified visual pack, backpack, and model style.
    /// </summary>
    /// <param name="packName">Display name of the texture pack.</param>
    /// <param name="backpackId">The backpack identifier to check.</param>
    /// <param name="modelStyle">The model style to check (Modern or Classic).</param>
    /// <returns>True if an override is registered, otherwise false.</returns>
    public static bool HasVisualBundle(string packName, string backpackId, ModelStyle modelStyle)
    {
#if ! API
        return VisualPackRegistry.HasOverride(packName, backpackId, modelStyle);
#else
        return false;
#endif
    }

    /// <summary>
    /// Checks whether an asset bundle override is registered for the specified visual pack, progression backpack, and model style.
    /// </summary>
    /// <param name="packName">Display name of the texture pack.</param>
    /// <param name="backpack">The strongly typed progression backpack identifier to check.</param>
    /// <param name="modelStyle">The model style to check (Modern or Classic).</param>
    /// <returns>True if an override is registered, otherwise false.</returns>
    public static bool HasVisualBundle(string packName, BackpackName backpack, ModelStyle modelStyle)
    {
        return HasVisualBundle(packName, backpack.ToBackpackId(), modelStyle);
    }

    /// <summary>
    /// Checks whether an asset bundle override is registered for the specified visual pack and bundle name.
    /// </summary>
    /// <param name="packName">Display name of the texture pack.</param>
    /// <param name="targetBundleName">The base bundle name to check.</param>
    /// <returns>True if an override is registered, otherwise false.</returns>
    public static bool HasVisualBundle(string packName, string targetBundleName)
    {
#if ! API
        return VisualPackRegistry.HasOverride(packName, targetBundleName);
#else
        return false;
#endif
    }

    /// <summary>
    /// Retrieves all registered texture pack names available for a specific backpack and model style, with 'Native' always included.
    /// </summary>
    /// <param name="backpackId">The backpack identifier to query.</param>
    /// <param name="modelStyle">The model style to query (Modern or Classic).</param>
    /// <returns>An array of available texture pack names.</returns>
    public static string[] GetAvailableVisualPacks(string backpackId, ModelStyle modelStyle)
    {
#if ! API
        return VisualPackRegistry.GetAvailablePacksFor(backpackId, modelStyle);
#else
        return new string[] { "Native" };
#endif
    }

    /// <summary>
    /// Retrieves all registered texture pack names available for a specific progression backpack and model style, with 'Native' always included.
    /// </summary>
    /// <param name="backpack">The strongly typed progression backpack identifier to query.</param>
    /// <param name="modelStyle">The model style to query (Modern or Classic).</param>
    /// <returns>An array of available texture pack names.</returns>
    public static string[] GetAvailableVisualPacks(BackpackName backpack, ModelStyle modelStyle)
    {
        return GetAvailableVisualPacks(backpack.ToBackpackId(), modelStyle);
    }

    /// <summary>
    /// Retrieves all registered texture pack names available for a specific bundle name, with 'Native' always included.
    /// </summary>
    /// <param name="targetBundleName">The base bundle name to query.</param>
    /// <returns>An array of available texture pack names.</returns>
    public static string[] GetAvailableVisualPacks(string targetBundleName)
    {
#if ! API
        return VisualPackRegistry.GetAvailablePacksFor(targetBundleName);
#else
        return new string[] { "Native" };
#endif
    }
}