using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace AdventureBackpacks.Compats;

public static class AzuCraftyBoxesCompat
{
    private static Type _iContainerType;
    private static object _proxyInstance;
    private static bool _queryFramePatched;
    private static bool _hasLoggedQueryError;
    private static bool _hasLoggedUiBankError;
    private static bool _isInitialized;

    public static void Awake(Harmony harmony, string guidID)
    {
        if (PlayerExtensions.IsDedicatedOrHeadless() || _isInitialized)
            return;

        if (harmony == null || string.IsNullOrEmpty(guidID))
            return;

        if (!Chainloader.PluginInfos.TryGetValue(guidID, out var pluginInfo) || pluginInfo == null)
            return;

        _isInitialized = true;

        try
        {
            var assembly = pluginInfo.Instance?.GetType().Assembly ?? Assembly.Load("AzuCraftyBoxes");
            if (assembly == null)
            {
                AdventureBackpacks.Log.Warning("AzuCraftyBoxes assembly could not be loaded. Skipping compatibility.");
                return;
            }

            _iContainerType = assembly.GetType("AzuCraftyBoxes.IContainers.IContainer");
            var boxesType = assembly.GetType("AzuCraftyBoxes.Util.Functions.Boxes");
            var queryFrameType = boxesType?.GetNestedType("QueryFrame", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var uiItemBankType = assembly.GetType("AzuCraftyBoxes.Util.Functions.UiItemBank");

            if (_iContainerType == null || queryFrameType == null)
            {
                AdventureBackpacks.Log.Warning("AzuCraftyBoxes detected, but required container types were not found. Skipping compatibility.");
                return;
            }

            var proxy = new BackpackContainerRealProxy(_iContainerType);
            _proxyInstance = proxy.GetTransparentProxy();

            var getMethod = AccessTools.Method(queryFrameType, "Get")?.MakeGenericMethod(typeof(Player));
            if (getMethod != null)
            {
                harmony.Patch(getMethod, postfix: new HarmonyMethod(typeof(AzuCraftyBoxesCompat), nameof(QueryFrameGetPostfix)));
                _queryFramePatched = true;
            }
            else
            {
                AdventureBackpacks.Log.Warning("AzuCraftyBoxes QueryFrame.Get method could not be found. Skipping QueryFrame patch.");
            }

            if (uiItemBankType != null)
            {
                var getTotalAny = AccessTools.Method(uiItemBankType, "GetTotalAnyQuality", new[] { typeof(string) });
                if (getTotalAny != null)
                    harmony.Patch(getTotalAny, postfix: new HarmonyMethod(typeof(AzuCraftyBoxesCompat), nameof(GetTotalAnyQualityPostfix)));

                var getTotalAt = AccessTools.Method(uiItemBankType, "GetTotalAtQuality", new[] { typeof(string), typeof(int) });
                if (getTotalAt != null)
                    harmony.Patch(getTotalAt, postfix: new HarmonyMethod(typeof(AzuCraftyBoxesCompat), nameof(GetTotalAtQualityPostfix)));
            }
        }
        catch (Exception ex)
        {
            AdventureBackpacks.Log.Warning($"AzuCraftyBoxes detected, but compatibility initialization failed: {ex.Message}");
        }
    }

    private static void QueryFrameGetPostfix(ref object __result)
    {
        try
        {
            if (__result is not IList list || _proxyInstance == null)
                return;

            if (Player.m_localPlayer == null || !CraftFromBackpack.CanCraftFromBackpack(Player.m_localPlayer, out _))
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (item != null && (ReferenceEquals(item, _proxyInstance) || 
                        (RemotingServices.IsTransparentProxy(item) && RemotingServices.GetRealProxy(item) is BackpackContainerRealProxy)))
                    {
                        list.RemoveAt(i);
                    }
                }
                return;
            }

            var found = false;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item == null)
                    continue;

                if (ReferenceEquals(item, _proxyInstance) || 
                    (RemotingServices.IsTransparentProxy(item) && RemotingServices.GetRealProxy(item) is BackpackContainerRealProxy))
                {
                    if (!found)
                    {
                        found = true;
                    }
                    else
                    {
                        list.RemoveAt(i);
                    }
                }
            }

            if (!found)
            {
                list.Add(_proxyInstance);
            }
        }
        catch (Exception ex)
        {
            if (!_hasLoggedQueryError)
            {
                _hasLoggedQueryError = true;
                AdventureBackpacks.Log.Error($"Exception in AzuCraftyBoxes QueryFrameGetPostfix: {ex}");
            }
        }
    }

    private static void GetTotalAnyQualityPostfix(string sharedName, ref int __result)
    {
        try
        {
            if (_queryFramePatched)
                return;

            if (string.IsNullOrEmpty(sharedName) || Player.m_localPlayer == null)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(Player.m_localPlayer, out var backpackInventory) || backpackInventory == null)
                return;

            __result += backpackInventory.CountItems(sharedName, -1, true);
        }
        catch (Exception ex)
        {
            if (!_hasLoggedUiBankError)
            {
                _hasLoggedUiBankError = true;
                AdventureBackpacks.Log.Error($"Exception in AzuCraftyBoxes GetTotalAnyQualityPostfix: {ex}");
            }
        }
    }

    private static void GetTotalAtQualityPostfix(string sharedName, int quality, ref int __result)
    {
        try
        {
            if (_queryFramePatched)
                return;

            if (string.IsNullOrEmpty(sharedName) || Player.m_localPlayer == null)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(Player.m_localPlayer, out var backpackInventory) || backpackInventory == null)
                return;

            __result += backpackInventory.CountItems(sharedName, quality, true);
        }
        catch (Exception ex)
        {
            if (!_hasLoggedUiBankError)
            {
                _hasLoggedUiBankError = true;
                AdventureBackpacks.Log.Error($"Exception in AzuCraftyBoxes GetTotalAtQualityPostfix: {ex}");
            }
        }
    }
}
