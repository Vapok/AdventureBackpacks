using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildHDClassicBundles
{
    public static void Run()
    {
        Debug.Log("Starting HD Classic Backpacks AssetBundle Build...");
        
        string outputDir = "Assets/StreamingAssets/HD_Classic";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>
        {
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_meadows",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackMeadows.prefab", "Assets/vapok/Sprites/BackpackMeadows_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_black_forest",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackBlackForest.prefab", "Assets/vapok/Sprites/IronBackpack_Icon.asset" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_swamp",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackSwamp.prefab", "Assets/vapok/Sprites/BackpackSwamp_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_mountains",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackMountains.prefab", "Assets/vapok/Sprites/SilverBackpack_Icon.asset" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_plains",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackPlains.prefab", "Assets/vapok/Sprites/BackpackPlains_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_classic_backpack_mistlands",
                assetNames = new[] { "Assets/HD_Classic/Prefabs/BackpackMistlands.prefab", "Assets/vapok/Sprites/BackpackMistlands_02_64_64.png" }
            }
        };
        
        BuildPipeline.BuildAssetBundles(outputDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        Debug.Log("BuildAssetBundles for HD Classic completed successfully!");
    }
}
