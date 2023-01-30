using System.Collections.Generic;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMountains : BackpackItem
{
    public BackpackMountains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe;
        AssignCraftingTable(CraftingTable.Forge,3);
        
        Item.MaximumRequiredStationLevel = 7;
        
        AddRecipeIngredient("CapeWolf",1);
        AddRecipeIngredient("WolfHairBundle",10);
        
        AddUpgradeIngredient("WolfPelt", 5);
        AddUpgradeIngredient("Silver", 5);
        
        Item.DropsFrom.Add("Fenring_Cultist", 0.02f, 1);
        Item.DropsFrom.Add("Fenring", 0.08f, 1);
        
        RegisterShaderSwap();

    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Mountains);
        RegisterBackpackSize(1,3,3);
        RegisterBackpackSize(2,4,3);
        RegisterBackpackSize(3,5,3);
        RegisterBackpackSize(4,6,3);
        RegisterWeightMultiplier();
        RegisterCarryBonus(20);
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
            default:
                break;
        }
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;

    }
}