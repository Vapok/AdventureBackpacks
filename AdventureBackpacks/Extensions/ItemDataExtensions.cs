using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Components;

namespace AdventureBackpacks.Extensions;

public static class ItemDataExtensions
{
    public static bool IsBackpack(this ItemDrop.ItemData item)
    {
        if (item == null)
            return false;

        if (item.m_shared != null && item.m_shared.m_name != null && Backpacks.IsBackpackType(item.m_shared.m_name))
            return true;

        if (item.m_dropPrefab != null && BackpackFactory.IsBackpackPrefab(item.m_dropPrefab))
            return true;

        if (item.m_customData != null && (item.m_customData.ContainsKey("AdventureBackpacks#AdventureBackpacks.Components.BackpackComponent") || item.m_customData.ContainsKey(BackpackComponent.OldPluginCustomData)))
            return true;

        return false;
    }
}