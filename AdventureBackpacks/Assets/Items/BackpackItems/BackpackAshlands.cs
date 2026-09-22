using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

//Ashen cape base, heat resistance on top.
internal class BackpackAshlands : BackpackItem
{
    internal const string Prefab = "BackpackAshlands";

    public BackpackAshlands(string assetName, string prefabName, string itemName, string registerAs = null) : base(assetName, prefabName, itemName, registerAs: registerAs)
    {
        RegisterConfigSettings();

        Item.Configurable = Configurability.Recipe | Configurability.Drop;

        AssignCraftingTable(CraftingTable.BlackForge, 3);

        Item.MaximumRequiredStationLevel = 4;

        AddRecipeIngredient("CapeAsh", 1);
        AddRecipeIngredient("AskHide", 6);
        AddRecipeIngredient("FlametalNew", 5);
        AddRecipeIngredient("MorgenSinew", 2);

        AddUpgradeIngredient("AskHide", 2);
        AddUpgradeIngredient("FlametalNew", 3);
        AddUpgradeIngredient("CharredBone", 5);

        Item.DropsFrom.Add("Charred_Melee", 0.002f, 1, dontScale: true);
        Item.DropsFrom.Add("Charred_Archer", 0.002f, 1, dontScale: true);
        Item.DropsFrom.Add("Charred_Mage", 0.002f, 1, dontScale: true);
        Item.DropsFrom.Add("Charred_Twitcher", 0.001f, 1, dontScale: true);
        Item.DropsFrom.Add("Morgen", 0.005f, 1, dontScale: true);
        Item.DropsFrom.Add("Fader", 0.08f, 1, dontScale: true);
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackBiome(BackpackBiomes.Ashlands);
        RegisterBackpackSize(1, 6, 4);
        RegisterBackpackSize(2, 7, 4);
        RegisterBackpackSize(3, 8, 4);
        RegisterBackpackSize(4, 9, 4);
        RegisterStatusEffectInfo();
        RegisterWeightMultiplier();
        RegisterCarryBonus(35);
        RegisterSpeedMod();
        RegisterHeatResistance();
        if ((BackpackBiome.Value & BackpackBiomes.Ashlands) != 0)
        {
            EffectsFactory.EffectList[BackpackEffect.FrostResistance].RegisterEffectBiomeQuality(BackpackBiomes.Ashlands, 1);
            EffectsFactory.EffectList[BackpackEffect.ColdResistance].RegisterEffectBiomeQuality(BackpackBiomes.Ashlands, 1);
        }
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        var shared = itemData.m_shared;

        shared.m_movementModifier = SpeedMod.Value / quality;

        //Ashen cape stats
        shared.m_attackStaminaModifier = -0.10f;
        shared.m_blockStaminaModifier = -0.20f;

        shared.m_heatResistanceModifier = HeatResistance.Value * quality;

        ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = CarryBonus.Value * quality;

        //Keep it below Barley Wine (Resistant)
        if (quality >= 3)
        {
            modifierList.Add(new HitData.DamageModPair
            {
                m_type = HitData.DamageType.Fire,
                m_modifier = HitData.DamageModifier.SlightlyResistant
            });
        }
    }
}
