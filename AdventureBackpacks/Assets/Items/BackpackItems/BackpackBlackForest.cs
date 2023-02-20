using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackBlackForest : BackpackItem
{
    public BackpackBlackForest(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        
        AssignCraftingTable(CraftingTable.Forge,1);
        
        Item.MaximumRequiredStationLevel = 3;
        
        AddRecipeIngredient("CapeTrollHide",1);
        AddRecipeIngredient("Copper",5);
        
        AddUpgradeIngredient("TrollHide", 3);
        AddUpgradeIngredient("Bronze", 3);
        
        Item.DropsFrom.Add("Greydwarf", 0.002f, 1);
        Item.DropsFrom.Add("Greydwarf_Elite", 0.004f, 1);
        Item.DropsFrom.Add("Greydwarf_Shaman", 0.004f, 1);
        Item.DropsFrom.Add("Troll", 0.01f, 1);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.BlackForest);
        RegisterBackpackSize(1,3,2);
        RegisterBackpackSize(2,4,2);
        RegisterBackpackSize(3,5,2);
        RegisterBackpackSize(4,6,2);
        RegisterWeightMultiplier();
        RegisterCarryBonus(10);
        RegisterSpeedMod();
        if ((BackpackBiome.Value & BackpackBiomes.BlackForest) != 0)
        {
            EffectsFactory.EffectList[BackpackEffect.ColdResistance].RegisterEffectBiomeQuality(BackpackBiomes.BlackForest, 1);
            EffectsFactory.EffectList[BackpackEffect.TrollArmor].RegisterEffectBiomeQuality(BackpackBiomes.BlackForest, 2);
        }
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}