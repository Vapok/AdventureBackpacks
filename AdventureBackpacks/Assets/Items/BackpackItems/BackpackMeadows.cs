using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMeadows : BackpackItem
{
    public BackpackMeadows(string prefabName, string itemName) : base(prefabName , itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe;
        AssignCraftingTable(CraftingTable.Workbench,2);
        
        Item.MaximumRequiredStationLevel = 3;
        
        AddRecipeIngredient("CapeDeerHide",1);
        AddRecipeIngredient("DeerHide",8);
        AddRecipeIngredient("BoneFragments",2);
        
        AddUpgradeIngredient("LeatherScraps", 5);
        AddUpgradeIngredient("DeerHide", 3);
        
        RegisterShaderSwap();
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Meadows);
        RegisterBackpackSize(1,3,1);
        RegisterBackpackSize(2,4,1);
        RegisterBackpackSize(3,5,1);
        RegisterBackpackSize(4,6,1);
        RegisterWeightMultiplier();
        RegisterCarryBonus(5);
        RegisterSpeedMod();
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        switch (quality)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
        }
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}