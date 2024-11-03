using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMountains : BackpackItem
{
    public BackpackMountains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        RegisterConfigSettings();
        
        Item.Configurable = Configurability.Recipe | Configurability.Drop;
        AssignCraftingTable(CraftingTable.Forge,3);
        
        Item.MaximumRequiredStationLevel = 7;
        
        AddRecipeIngredient("CapeWolf",1);
        AddRecipeIngredient("WolfHairBundle",10);
        
        AddUpgradeIngredient("WolfPelt", 5);
        AddUpgradeIngredient("Silver", 5);
        
        Item.DropsFrom.Add("Fenring_Cultist", 0.002f, 1);
        Item.DropsFrom.Add("Fenring", 0.008f, 1);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Mountains);
        RegisterBackpackSize(1,3,3);
        RegisterBackpackSize(2,4,3);
        RegisterBackpackSize(3,5,3);
        RegisterBackpackSize(4,6,3);
        RegisterStatusEffectInfo();
        RegisterWeightMultiplier();
        RegisterCarryBonus(20);
        RegisterSpeedMod();
        if ((BackpackBiome.Value & BackpackBiomes.Mountains) != 0)
        {
            EffectsFactory.EffectList[BackpackEffect.FeatherFall].RegisterEffectBiomeQuality(BackpackBiomes.Mountains, 4);
            EffectsFactory.EffectList[BackpackEffect.FrostResistance].RegisterEffectBiomeQuality(BackpackBiomes.Mountains, 1);
            EffectsFactory.EffectList[BackpackEffect.ColdResistance].RegisterEffectBiomeQuality(BackpackBiomes.Mountains, 1);
        }
                        
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        itemData.m_shared.m_movementModifier = SpeedMod.Value/quality;
        
        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;
    }
}