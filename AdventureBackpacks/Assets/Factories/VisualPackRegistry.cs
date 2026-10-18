using System;
using System.Collections.Generic;
using AdventureBackpacks.API;
using UnityEngine;

namespace AdventureBackpacks.Assets.Factories;

internal static class VisualPackRegistry
{
    internal const string Native = "Native";

    private static readonly Dictionary<string, Dictionary<string, AssetBundle>> _bundleOverrides =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, string> _defaultPacks =
        new(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeId(string backpackId)
    {
        if (string.IsNullOrEmpty(backpackId))
        {
            return string.Empty;
        }

        if (backpackId.StartsWith("classic_", StringComparison.OrdinalIgnoreCase))
        {
            return backpackId.Substring(8);
        }

        return backpackId;
    }

    private static string MakeKey(string backpackId, ModelStyle modelStyle)
    {
        return $"{NormalizeId(backpackId)}:{(int)modelStyle}";
    }

    internal static void Register(string packName, string backpackId, ModelStyle modelStyle, AssetBundle bundle, bool isDefault = false)
    {
        if (string.IsNullOrEmpty(packName) || string.IsNullOrEmpty(backpackId) || bundle == null)
        {
            return;
        }

        string key = MakeKey(backpackId, modelStyle);
        if (!_bundleOverrides.TryGetValue(key, out Dictionary<string, AssetBundle> packs))
        {
            packs = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
            _bundleOverrides[key] = packs;
        }

        packs[packName] = bundle;

        if (isDefault)
        {
            _defaultPacks[key] = packName;
        }
    }

    internal static void Register(string packName, string targetBundleName, AssetBundle bundle, bool isDefault = false)
    {
        if (string.IsNullOrEmpty(targetBundleName))
        {
            return;
        }

        ModelStyle style = targetBundleName.StartsWith("classic_", StringComparison.OrdinalIgnoreCase)
            ? ModelStyle.Classic
            : ModelStyle.Modern;

        Register(packName, NormalizeId(targetBundleName), style, bundle, isDefault);
    }

    internal static string GetDefaultPackFor(string backpackId, ModelStyle modelStyle)
    {
        string key = MakeKey(backpackId, modelStyle);
        if (_defaultPacks.TryGetValue(key, out string defaultPack) &&
            _bundleOverrides.TryGetValue(key, out Dictionary<string, AssetBundle> packs) &&
            packs.ContainsKey(defaultPack))
        {
            return defaultPack;
        }

        return Native;
    }

    internal static string GetDefaultPackFor(string targetBundleName)
    {
        ModelStyle style = targetBundleName.StartsWith("classic_", StringComparison.OrdinalIgnoreCase)
            ? ModelStyle.Classic
            : ModelStyle.Modern;
        return GetDefaultPackFor(NormalizeId(targetBundleName), style);
    }

    internal static string[] GetAvailablePacksFor(string backpackId, ModelStyle modelStyle)
    {
        List<string> options = new() { Native };
        string key = MakeKey(backpackId, modelStyle);

        if (_bundleOverrides.TryGetValue(key, out Dictionary<string, AssetBundle> packs))
        {
            foreach (string packName in packs.Keys)
            {
                bool exists = false;
                for (int i = 0; i < options.Count; i++)
                {
                    if (string.Equals(options[i], packName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    options.Add(packName);
                }
            }
        }

        return options.ToArray();
    }

    internal static string[] GetAvailablePacksFor(string targetBundleName)
    {
        ModelStyle style = targetBundleName.StartsWith("classic_", StringComparison.OrdinalIgnoreCase)
            ? ModelStyle.Classic
            : ModelStyle.Modern;
        return GetAvailablePacksFor(NormalizeId(targetBundleName), style);
    }

    internal static bool TryGetOverride(string packName, string backpackId, ModelStyle modelStyle, out AssetBundle bundle)
    {
        bundle = null;
        if (string.IsNullOrEmpty(packName) ||
            string.Equals(packName, Native, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(backpackId))
        {
            return false;
        }

        string key = MakeKey(backpackId, modelStyle);
        return _bundleOverrides.TryGetValue(key, out Dictionary<string, AssetBundle> packs) &&
               packs.TryGetValue(packName, out bundle) &&
               bundle != null;
    }

    internal static bool TryGetOverride(string packName, string targetBundleName, out AssetBundle bundle)
    {
        ModelStyle style = targetBundleName.StartsWith("classic_", StringComparison.OrdinalIgnoreCase)
            ? ModelStyle.Classic
            : ModelStyle.Modern;
        return TryGetOverride(packName, NormalizeId(targetBundleName), style, out bundle);
    }

    internal static bool HasOverride(string packName, string backpackId, ModelStyle modelStyle)
    {
        return TryGetOverride(packName, backpackId, modelStyle, out _);
    }

    internal static bool HasOverride(string packName, string targetBundleName)
    {
        return TryGetOverride(packName, targetBundleName, out _);
    }

    internal static string GetNativeBundleName(string backpackId, ModelStyle modelStyle)
    {
        string cleanId = NormalizeId(backpackId);
        return modelStyle == ModelStyle.Classic ? $"classic_{cleanId}" : cleanId;
    }

    internal static string ResolveBundle(string packName, string backpackId, ModelStyle modelStyle)
    {
        if (TryGetOverride(packName, backpackId, modelStyle, out AssetBundle overrideBundle))
        {
            return overrideBundle.name;
        }

        return GetNativeBundleName(backpackId, modelStyle);
    }
}
