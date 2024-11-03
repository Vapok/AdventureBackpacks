using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;


namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMistlands : BackpackItem
{
    public BackpackMistlands(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        
        AssignCraftingTable(CraftingTable.BlackForge,1);
        
        Item.MaximumRequiredStationLevel = 2;
        
        AddRecipeIngredient("CapeFeather",1);
        AddRecipeIngredient("ScaleHide",5);
        AddRecipeIngredient("Eitr",10);
        
        AddUpgradeIngredient("ScaleHide", 4);
        AddUpgradeIngredient("Eitr", 2);
        AddUpgradeIngredient("Softtissue", 5);
        
        Item.DropsFrom.Add("Dverger", 0.002f, 1);
        Item.DropsFrom.Add("DvergerMage", 0.002f, 1);
        Item.DropsFrom.Add("DvergerMageFire", 0.002f, 1);
        Item.DropsFrom.Add("DvergerMageIce", 0.002f, 1);
        Item.DropsFrom.Add("DvergerMageSupport", 0.002f, 1);
    }
    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Mistlands);
        RegisterBackpackSize(1,8,2);
        RegisterBackpackSize(2,5,4);
        RegisterBackpackSize(3,6,4);
        RegisterBackpackSize(4,7,4);
        RegisterStatusEffectInfo();
        RegisterWeightMultiplier();
        RegisterCarryBonus(30);
        RegisterSpeedMod();
        if ((BackpackBiome.Value & BackpackBiomes.Mistlands) != 0)
        {
            EffectsFactory.EffectList[BackpackEffect.FeatherFall].RegisterEffectBiomeQuality(BackpackBiomes.Mistlands, 3);
            EffectsFactory.EffectList[BackpackEffect.Demister].RegisterEffectBiomeQuality(BackpackBiomes.Mistlands, 4);
            EffectsFactory.EffectList[BackpackEffect.FrostResistance].RegisterEffectBiomeQuality(BackpackBiomes.Mistlands, 2);
            EffectsFactory.EffectList[BackpackEffect.ColdResistance].RegisterEffectBiomeQuality(BackpackBiomes.Mistlands, 1);
        }
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}