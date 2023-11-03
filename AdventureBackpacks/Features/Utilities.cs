using ItemManager;
using UnityEngine;

namespace AdventureBackpacks.Features;

public static class Utilities
{
    public static AssetBundle LoadAssetBundle(string filename, string folderName)
    {
        return PrefabManager.RegisterAssetBundle(filename, folderName);
    }
}