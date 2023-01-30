using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class LegacyIronBackpack: BackpackItem
{
    public LegacyIronBackpack(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome();
        RegisterBackpackSize();
        RegisterWeightMultiplier();
        RegisterCarryBonus(25);
        RegisterSpeedMod();
        RegisterEnableFreezing(false);
    }
    internal override Vector2i GetInventorySize(int quality)
    {
        return base.GetInventorySize(1);
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        if (EnableFreezing.Value)
            modifierList.Add(BackpackEffects.FrostResistance);
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}