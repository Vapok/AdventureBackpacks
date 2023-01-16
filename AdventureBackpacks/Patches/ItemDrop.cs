using System;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using HarmonyLib;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Patches;

public class ItemDropPatches
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
    [HarmonyBefore(new string[]{"randyknapp.mods.epicloot"})]
    static class GetWeightPatch
    {
        static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            if (__instance == null || __instance.m_shared == null)
                return;
            
            try
            {
                if (string.IsNullOrEmpty(__instance.m_shared.m_name))
                    return;

                if (Backpacks.BackpackTypes.Contains(__instance.m_shared.m_name))
                {
                    // If the item in GetWeight() is a backpack, and it has been Extended(), call GetTotalWeight() on its Inventory.
                    // Note that GetTotalWeight() just returns a the value of m_totalWeight, and doesn't do any calculation on its own.
                    // If the Inventory has been changed at any point, it calls UpdateTotalWeight(), which should ensure that its m_totalWeight is accurate.
                    var inventoryWeight = __instance.Data().GetOrCreate<BackpackComponent>().GetInventory()?.GetTotalWeight() ?? 0;

                    // To the backpack's item weight, add the backpack's inventory weight multiplied by the weightMultiplier in the configs.
                    __result += inventoryWeight * ConfigRegistry.WeightMultiplier.Value;
                }
            }
            catch (Exception e)
            {
                AdventureBackpacks.Log.Debug($"[ItemDrop.ItemData.GetWeight] An Error occurred - {e.Message}");
            }
        }
    }
}