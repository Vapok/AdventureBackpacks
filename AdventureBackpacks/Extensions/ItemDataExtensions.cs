using AdventureBackpacks.Assets;

namespace AdventureBackpacks.Extensions;

public static class ItemDataExtensions
{
    public static bool IsBackpack(this ItemDrop.ItemData item)
    {
        return Backpacks.BackpackTypes.Contains(item.m_shared.m_name);
    }
}