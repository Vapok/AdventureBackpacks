using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMistlands : BackpackItem
{
    public BackpackMistlands(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe;
        AssignCraftingTable(CraftingTable.BlackForge,1);
        
        Item.MaximumRequiredStationLevel = 2;
        
        AddRecipeIngredient("CapeFeather",1);
        AddRecipeIngredient("ScaleHide",5);
        AddRecipeIngredient("Eitr",10);
        
        AddUpgradeIngredient("ScaleHide", 4);
        AddUpgradeIngredient("Eitr", 2);
        AddUpgradeIngredient("Softtissue", 5);
        
        Item.DropsFrom.Add("Dverger", 0.02f, 1);
        Item.DropsFrom.Add("DvergerMage", 0.02f, 1);
        Item.DropsFrom.Add("DvergerMageFire", 0.02f, 1);
        Item.DropsFrom.Add("DvergerMageIce", 0.02f, 1);
        Item.DropsFrom.Add("DvergerMageSupport", 0.02f, 1);

        //RegisterShaderSwap();

    }
    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Mistlands);
        RegisterBackpackSize(1,4,4);
        RegisterBackpackSize(2,5,4);
        RegisterBackpackSize(3,6,4);
        RegisterBackpackSize(4,7,4);
        RegisterWeightMultiplier();
        RegisterCarryBonus(30);
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