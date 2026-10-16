using System.Collections.Generic;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Factories;
using ItemManager;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

//Ashen cape base, heat resistance on top.
internal class BackpackAshlands : BackpackItem
{
    public BackpackAshlands(string assetName, string prefabName, string itemName) : base(assetName, prefabName, itemName)
    {
        RegisterConfigSettings();

        if (Item == null)
            return;

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
        AddUpgraderIngredient("Upgrader6Armor", 1);

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
        RegisterArmorPerLevel();
        RegisterSpeedMod();
        RegisterHeatResistance();
        if (BackpackBiome != null && (BackpackBiome.Value & BackpackBiomes.Ashlands) != 0)
        {
            if (EffectsFactory.EffectList.ContainsKey(BackpackEffect.FrostResistance))
                EffectsFactory.EffectList[BackpackEffect.FrostResistance].RegisterEffectBiomeQuality(BackpackBiomes.Ashlands, 1);
            if (EffectsFactory.EffectList.ContainsKey(BackpackEffect.ColdResistance))
                EffectsFactory.EffectList[BackpackEffect.ColdResistance].RegisterEffectBiomeQuality(BackpackBiomes.Ashlands, 1);
        }
    }

    internal override void UpdateStatusEffects(int quality, CustomSE statusEffects, List<HitData.DamageModPair> modifierList, ItemDrop.ItemData itemData)
    {
        var shared = itemData.m_shared;

        shared.m_movementModifier = GetSpeedModifier(quality);

        //Ashen cape stats
        shared.m_attackStaminaModifier = -0.10f;
        shared.m_blockStaminaModifier = -0.20f;

        //Stops at the Flametal set value; Forge of Potential levels do not add more
        shared.m_heatResistanceModifier = HeatResistance.Value * System.Math.Min(quality, 4);

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
