using System;
using System.Collections.Generic;
using System.IO;
using AdventureBackpacks.API;
using AdventureBackpacks.Features;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;

namespace AdventureBackpacks.Compats;

public static class ChebsNecromancy
{
        public static void SetupNecromancyBackpackUsingApi()
        {
            var effectName = "SetEffect_NecromancyArmor";
            var assetName = "chebgonaz";
            var assetFolderName = "Assets";
            var assetBundlePath = "";
            AssetBundle assetBundle;
            var necromancySkillIdentifier = "friendlyskeletonwand_necromancy_skill";
            Skills.SkillType skillUid;
            StatusEffect externalStatusEffect = null;

            if (!ABAPI.IsLoaded()) return;

            var pluginInfo = Chainloader.PluginInfos["com.chebgonaz.ChebsNecromancy"];
            assetBundlePath = Path.Combine(Paths.PluginPath,Path.GetDirectoryName(pluginInfo.Location) ?? "", assetFolderName, assetName);
            
            if (!File.Exists(assetBundlePath))
                assetBundlePath = Path.Combine(Paths.PluginPath,Path.GetDirectoryName(pluginInfo.Location) ?? "", assetName);
            
            assetBundle = !File.Exists(assetBundlePath) ? null : AssetBundle.LoadFromFile(assetBundlePath);

            if (assetBundle == null)
            {
                AdventureBackpacks.Log.Error($"Can't find Asset Bundle for Status Effect: {effectName} - Effect Not Registered");
                return;
            }
            
            var num = Math.Abs(necromancySkillIdentifier.GetStableHashCode());
            
            skillUid = (Skills.SkillType)num;
            
            var seStat = assetBundle.LoadAsset<SE_Stats>(effectName);
            if (seStat != null)
            {
                seStat.m_skillLevel = skillUid;
                seStat.m_skillLevelModifier = 10; //This is obviously a value that can change, but let's keep it to the value in the mod using the API.
            }
            externalStatusEffect = seStat;
            
            //Register Effect First
            var effectDefinition = new ABAPI.EffectDefinition(
                "Necromancy Armor Effect",
                "Necromancy Armor Effect",
                effectName,
                "When activated the backpack provides the Necromancy Armor effect from Cheb's Necromancy",
                externalStatusEffect
            );
            
            ABAPI.RegisterEffect(effectDefinition);
            
            //Register Backpack
            var backpackAssetBundle = Utilities.LoadAssetBundle("chebsbackpack","Assets.Bundles");

            if (backpackAssetBundle == null)
            {
                AdventureBackpacks.Log.Warning($"Asset Bundle Not Found");
                return;
            }
            var backpackPrefab = "BackpackNecromancy";

            var backpackDefinition = new ABAPI.BackpackDefinition(backpackAssetBundle, backpackPrefab);

            backpackDefinition.ItemName = "$item_friendlyskeletonwand_spectralshroud_backpack";
            backpackDefinition.CraftingTable = "piece_workbench";
            backpackDefinition.StationLevel = 1;
            backpackDefinition.MaxRequiredStationLevel = 4;
            backpackDefinition.RecipeIngredients.Add(new ABAPI.RecipeIngredient("ChebGonaz_SpectralShroud",1));
            backpackDefinition.RecipeIngredients.Add(new ABAPI.RecipeIngredient("TrollHide",5));
            backpackDefinition.UpgradeIngredients.Add(new ABAPI.RecipeIngredient("Chain", 1));
            backpackDefinition.UpgradeIngredients.Add(new ABAPI.RecipeIngredient("TrollHide", 5));
            backpackDefinition.DropsFrom.Add(new ABAPI.DropTarget("ChebGonaz_GuardianWraith",0.002f, 1));

            backpackDefinition.BackpackBiome = BackpackBiomes.EffectBiome1 | BackpackBiomes.BlackForest;
            backpackDefinition.BackpackSizeByQuality.Add(1,new Vector2(3,3));
            backpackDefinition.BackpackSizeByQuality.Add(2,new Vector2(4,3));
            backpackDefinition.BackpackSizeByQuality.Add(3,new Vector2(5,3));
            backpackDefinition.BackpackSizeByQuality.Add(4,new Vector2(6,3));
            backpackDefinition.WeightMultiplier = 20;
            backpackDefinition.EffectsToApply.Add(BackpackBiomes.EffectBiome1,new KeyValuePair<StatusEffect, int>(externalStatusEffect,1));
            backpackDefinition.ItemSetStatusEffect = externalStatusEffect;

            ABAPI.RegisterBackpack(backpackDefinition);
        }

}