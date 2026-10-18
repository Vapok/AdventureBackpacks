using System;
using System.IO;
using AdventureBackpacks.API.Client;
using BepInEx;
using UnityEngine;

namespace AdventureBackpacks.HD;

[BepInPlugin(ModId, ModName, ModVersion)]
[BepInDependency("vapok.mods.adventurebackpacks", BepInDependency.DependencyFlags.HardDependency)]
public class AdventureBackpacksHD : BaseUnityPlugin
{
    public const string ModId = "vapok.mods.adventurebackpackshd";
    public const string ModName = "Adventure Backpacks HD";
    public const string ModVersion = "1.0.0";
    public const string PackName = "Native HD";

    private void Awake()
    {
        string assetsDirectory = Path.Combine(Path.GetDirectoryName(Info.Location) ?? string.Empty, "Assets");

        // Modern Model Overrides (2K)
        RegisterBundle(ABAPIClient.BackpackName.Meadows, ABAPIClient.ModelStyle.Modern, "hd_backpack_meadows", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.BlackForest, ABAPIClient.ModelStyle.Modern, "hd_backpack_black_forest", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Swamp, ABAPIClient.ModelStyle.Modern, "hd_backpack_swamp", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Mountains, ABAPIClient.ModelStyle.Modern, "hd_backpack_mountains", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Plains, ABAPIClient.ModelStyle.Modern, "hd_backpack_plains", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Mistlands, ABAPIClient.ModelStyle.Modern, "hd_backpack_mistlands", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Ashlands, ABAPIClient.ModelStyle.Modern, "hd_backpack_ashlands", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.DeepNorth, ABAPIClient.ModelStyle.Modern, "hd_backpack_deepnorth", assetsDirectory);

        // Classic Model Overrides (2K)
        RegisterBundle(ABAPIClient.BackpackName.Meadows, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_meadows", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.BlackForest, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_black_forest", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Swamp, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_swamp", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Mountains, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_mountains", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Plains, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_plains", assetsDirectory);
        RegisterBundle(ABAPIClient.BackpackName.Mistlands, ABAPIClient.ModelStyle.Classic, "hd_classic_backpack_mistlands", assetsDirectory);
    }

    private void RegisterBundle(ABAPIClient.BackpackName backpack, ABAPIClient.ModelStyle modelStyle, string bundleFileName, string assetsDirectory)
    {
        string bundlePath = Path.Combine(assetsDirectory, bundleFileName);
        if (!File.Exists(bundlePath))
        {
            return;
        }

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle != null)
            {
                if (ABAPIClient.RegisterVisualBundle(PackName, backpack, modelStyle, bundle, true))
                {
                    Logger.LogInfo($"Registered visual override for '{backpack}' ({modelStyle}) from '{bundleFileName}'.");
                }
                else
                {
                    Logger.LogWarning($"Failed to register visual override for '{backpack}' ({modelStyle}). Is AdventureBackpacks active?");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to load companion bundle '{bundleFileName}': {ex.Message}");
        }
    }
}
