using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackPlains : BackpackItem
{
    public BackpackPlains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.Plains;
        RegisterConfigSettings();
        Item.Configurable = Configurability.Recipe;
        AssignCraftingTable(CraftingTable.Forge,3);
        
        Item.MaximumRequiredStationLevel = 7;
        
        AddRecipeIngredient("CapeLox",1);
        AddRecipeIngredient("Tar",5);
        AddRecipeIngredient("BlackMetal",5);
        
        AddUpgradeIngredient("LoxPelt", 2);
        AddUpgradeIngredient("BlackMetal", 5);
        
        RegisterShaderSwap();
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackSize(1,3,4);
        RegisterBackpackSize(2,4,4);
        RegisterBackpackSize(3,5,4);
        RegisterBackpackSize(4,6,4);
        RegisterWeightMultiplier();
        RegisterCarryBonus(25);
        RegisterSpeedMod();
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        modifierList.Add(BackpackEffects.FrostResistance);
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