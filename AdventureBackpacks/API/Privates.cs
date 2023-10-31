#if ! API
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vapok.Common.Managers;using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;


namespace AdventureBackpacks.API;


// ReSharper disable once InconsistentNaming
public partial class ABAPI
{
    private static Backpack? ConvertBackpackItem(BackpackComponent component)
    {
        var definition = GetBackPackDefinitionFromComponent(component);
        if (!definition.HasValue)
            return null;
        
        var backpackItem = new Backpack
        {
            Name = definition.Value.ItemName,
            ItemData = component.Item,
            Definition = definition.Value,
            Inventory = component.GetInventory()
        };
        return backpackItem;
    }
    
    private static Backpack? ConvertBackpackItem(ItemDrop.ItemData itemData)
    {
        if (!itemData.IsBackpack())
            return null;
        
        var component = itemData.Data().GetOrCreate<BackpackComponent>();
        return ConvertBackpackItem(component);
    }

    private static Dictionary<int, Vector2> GetBackpackSizing(BackpackItem backpack)
    {
        return backpack.BackpackSize.ToDictionary(entry => entry.Key, entry => entry.Value.Value);
    }

    private static BackpackDefinition? GetBackPackDefinitionFromComponent(BackpackComponent component)
    {
        var isBackpack = component.Item.TryGetBackpackItem(out var backpack);
        if (isBackpack)
            return null;
        
        return GetBackPackDefinition(backpack);
    }

    private static BackpackDefinition GetBackPackDefinition(BackpackItem backpack)
    {
        var definition = new BackpackDefinition
        {
            ItemName = backpack.ItemName,
            PrefabName = backpack.PrefabName,
            BackpackSizeByQuality = GetBackpackSizing(backpack),
            WeightMultiplier = backpack.WeightMultiplier.Value,
            CarryBonus = backpack.CarryBonus.Value,
            SpeedMod = backpack.SpeedMod.Value,
            EnableFreezing = backpack.EnableFreezing.Value,
            BackpackBiome = backpack.BackpackBiome.Value
        };
        return definition;
    }
}
#endif
