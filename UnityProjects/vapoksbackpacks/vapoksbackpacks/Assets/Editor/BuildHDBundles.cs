using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildHDBundles
{
    [MenuItem("Assets/Build HD AssetBundles")]
    public static void Run()
    {
        Debug.Log("Starting HD Backpacks Build...");
        
        string outputDir = "Assets/StreamingAssets/HD";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>
        {
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_meadows",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackMeadows.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_black_forest",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackBlackForest.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_swamp",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackSwamp.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_mountains",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackMountains.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_plains",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackPlains.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_mistlands",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackMistlands.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_ashlands",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackAshlands.prefab" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "hd_backpack_deepnorth",
                assetNames = new[] { "Assets/HD/Prefabs/BackpackDeepNorth.prefab" }
            }
        };
        
        BuildPipeline.BuildAssetBundles(outputDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        Debug.Log("BuildAssetBundles for HD completed successfully!");
    }
}
