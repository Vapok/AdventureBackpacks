using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackSwamp : BackpackItem
{
    public BackpackSwamp(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        AssignCraftingTable(CraftingTable.Workbench,2);
        
        Item.MaximumRequiredStationLevel = 5;
        
        AddRecipeIngredient("Bloodbag",10);
        AddRecipeIngredient("Root",4);
        AddRecipeIngredient("Guck",4);
        
        AddUpgradeIngredient("Bloodbag", 2);
        AddUpgradeIngredient("Iron", 5);
        
        Item.DropsFrom.Add("Draugr", 0.002f, 1);
        Item.DropsFrom.Add("Draugr_Ranged", 0.004f, 1);
        Item.DropsFrom.Add("Draugr_Elite", 0.004f, 1);
        Item.DropsFrom.Add("Troll", 0.008f, 1);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Swamp);
        RegisterBackpackSize(1,2,3);
        RegisterBackpackSize(2,3,3);
        RegisterBackpackSize(3,4,3);
        RegisterBackpackSize(4,5,3);
        RegisterWeightMultiplier();
        RegisterCarryBonus(15);
        RegisterSpeedMod();
        Waterproof.Configuration.RegisterEffectBiomeQuality(BackpackBiome.Value, 2);
        ColdResistance.Configuration.RegisterEffectBiomeQuality(BackpackBiome.Value, 1);
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
            default:
                break;
        }
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;

    }
}