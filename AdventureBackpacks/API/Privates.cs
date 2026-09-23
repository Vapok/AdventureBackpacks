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
        if (definition == null)
            return null;
        
        var backpackItem = new Backpack
        {
            Name = definition.ItemName,
            ItemData = component.Item,
            Definition = definition,
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

    private static BackpackDefinition GetBackPackDefinitionFromComponent(BackpackComponent component)
    {
        if (component == null || component.Item == null)
            return null;

        bool isBackpack = component.Item.TryGetBackpackItem(out BackpackItem backpack);
        if (!isBackpack || backpack == null)
            return null;
        
        return GetBackPackDefinition(backpack);
    }

    private static BackpackDefinition GetBackPackDefinition(BackpackItem backpack)
    {
        if (backpack == null)
            return null;

        BackpackDefinition definition = new BackpackDefinition
        {
            ItemName = backpack.ItemName,
            PrefabName = backpack.PrefabName,
            BackpackSizeByQuality = GetBackpackSizing(backpack),
            WeightMultiplier = backpack.WeightMultiplier != null ? backpack.WeightMultiplier.Value : 0.5f,
            CarryBonus = backpack.CarryBonus != null ? backpack.CarryBonus.Value : 0,
            SpeedMod = backpack.SpeedMod != null ? backpack.SpeedMod.Value : 0f,
            EnableFreezing = backpack.EnableFreezing != null ? backpack.EnableFreezing.Value : false,
            BackpackBiome = backpack.BackpackBiome != null ? backpack.BackpackBiome.Value : BackpackBiomes.None
        };
        return definition;
    }
}
#endif
