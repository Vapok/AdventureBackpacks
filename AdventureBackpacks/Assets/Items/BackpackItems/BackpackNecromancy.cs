using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackNecromancy : BackpackItem
{
    public BackpackNecromancy(string prefabName, string itemName) : base(prefabName, itemName,"",true)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        AssignCraftingTable(CraftingTable.Workbench,1);
        
        Item.MaximumRequiredStationLevel = 4;
        
        AddRecipeIngredient("ChebGonaz_SpectralShroud",1);
        AddRecipeIngredient("TrollHide",5);
        
        AddUpgradeIngredient("Chain", 1);
        AddUpgradeIngredient("TrollHide", 5);
        
        Item.DropsFrom.Add("ChebGonaz_GuardianWraith", 0.002f, 1);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.BlackForest);
        RegisterBackpackSize(1,3,3);
        RegisterBackpackSize(2,4,3);
        RegisterBackpackSize(3,5,3);
        RegisterBackpackSize(4,6,3);
        RegisterWeightMultiplier();
        RegisterCarryBonus(20);
        RegisterSpeedMod();
        EffectsFactory.EffectList[BackpackEffect.NecromancyArmor].RegisterEffectBiomeQuality(BackpackBiome.Value, 1);
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}