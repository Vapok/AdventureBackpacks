using Jotunn.Utils;
using UnityEngine;

namespace AdventureBackpacks.Features;

public static class Utilities
{
    public static AssetBundle LoadAssetBundle(string assetBundleFileName, string folderName)
    {
        return AssetUtils.LoadAssetBundleFromResources(assetBundleFileName, typeof(AdventureBackpacks).Assembly);
    }
}