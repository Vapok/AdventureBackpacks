using System.Collections.Generic;
using JetBrains.Annotations;
#if ! API
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
#endif

namespace AdventureBackpacks.API;

/// <summary>
/// Adventure Backpacks API. Be sure to include the AdventureBackpacksAPI.dll as a dependency to your project.
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
}