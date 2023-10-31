using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackPlains : BackpackItem
{
    public BackpackPlains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        AssignCraftingTable(CraftingTable.Forge,3);
        
        Item.MaximumRequiredStationLevel = 7;
        
        AddRecipeIngredient("CapeLox",1);
        AddRecipeIngredient("Tar",5);
        AddRecipeIngredient("BlackMetal",5);
        
        AddUpgradeIngredient("LoxPelt", 2);
        AddUpgradeIngredient("BlackMetal", 5);

        Item.DropsFrom.Add("Goblin", 0.002f, 1);
        Item.DropsFrom.Add("GoblinArcher", 0.002f, 1);
        Item.DropsFrom.Add("GoblinBrute", 0.002f, 1);
        Item.DropsFrom.Add("GoblinShaman", 0.002f, 1);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Plains);
        RegisterBackpackSize(1,3,4);
        RegisterBackpackSize(2,4,4);
        RegisterBackpackSize(3,5,4);
        RegisterBackpackSize(4,6,4);
        RegisterStatusEffectInfo();
        RegisterWeightMultiplier();
        RegisterCarryBonus(25);
        RegisterSpeedMod();
        if ((BackpackBiome.Value & BackpackBiomes.Plains) != 0)
            EffectsFactory.EffectList[BackpackEffect.FrostResistance].RegisterEffectBiomeQuality(BackpackBiomes.Plains, 1);
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}