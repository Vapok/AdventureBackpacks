using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class OptimizeAndBuildClassicBundles
{
    public static void Run()
    {
        Debug.Log("Starting Classic Backpacks Optimization and Build...");
        
        string[] classicPrefabs = new string[]
        {
            "Assets/vapok/Prefabs/BackpackMeadows.prefab",
            "Assets/vapok/Prefabs/BackpackBlackForest.prefab",
            "Assets/vapok/Prefabs/BackpackSwamp.prefab",
            "Assets/vapok/Prefabs/BackpackMountains.prefab",
            "Assets/vapok/Prefabs/BackpackPlains.prefab",
            "Assets/vapok/Prefabs/BackpackMistlands.prefab",
            "Assets/vapok/Prefabs/CapeIronBackpack.prefab",
            "Assets/vapok/Prefabs/CapeSilverBackpack.prefab"
        };
        
        string[] dependencies = AssetDatabase.GetDependencies(classicPrefabs, true);
        int textureCount = 0;
        
        foreach (string dep in dependencies)
        {
            if (dep.StartsWith("Assets/BaalEvan"))
                continue;

            TextureImporter importer = AssetImporter.GetAtPath(dep) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                
                TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings("Standalone");
                
                if (!settings.overridden || settings.maxTextureSize > 1024 || !settings.crunchedCompression)
                {
                    settings.overridden = true;
                    settings.maxTextureSize = Mathf.Min(settings.maxTextureSize, 1024);
                    settings.textureCompression = TextureImporterCompression.Compressed;
                    settings.crunchedCompression = true;
                    settings.compressionQuality = 80;
                    importer.SetPlatformTextureSettings(settings);
                    modified = true;
                }
                
                if (importer.maxTextureSize > 1024)
                {
                    importer.maxTextureSize = 1024;
                    modified = true;
                }
                
                if (modified)
                {
                    importer.SaveAndReimport();
                    textureCount++;
                    Debug.Log($"Optimized texture: {dep}");
                }
            }
        }
        
        Debug.Log($"Optimized {textureCount} textures.");
        
        string outputDir = "Assets/StreamingAssets/Classic";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>
        {
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_meadows",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackMeadows.prefab", "Assets/vapok/Sprites/BackpackMeadows_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_black_forest",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackBlackForest.prefab", "Assets/vapok/Sprites/IronBackpack_Icon.asset" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_swamp",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackSwamp.prefab", "Assets/vapok/Sprites/BackpackSwamp_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_mountains",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackMountains.prefab", "Assets/vapok/Sprites/SilverBackpack_Icon.asset" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_plains",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackPlains.prefab", "Assets/vapok/Sprites/BackpackPlains_02_64_64.png" }
            },
            new AssetBundleBuild
            {
                assetBundleName = "classic_backpack_mistlands",
                assetNames = new[] { "Assets/vapok/Prefabs/BackpackMistlands.prefab", "Assets/vapok/Sprites/BackpackMistlands_02_64_64.png" }
            }
        };
        
        BuildPipeline.BuildAssetBundles(outputDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        Debug.Log("BuildAssetBundles for Classic completed successfully!");
    }
}
