using HarmonyLib;
using UnityEngine;

namespace AdventureBackpacks.Patches;

public static class RecipePatches
{
    [HarmonyPatch(typeof(Recipe), nameof(Recipe.GetAmount))]
    static class RecipeGetAmountPatch
    {
        [HarmonyPrefix]
        static bool Prefix(Recipe __instance, int quality, out int need, out ItemDrop.ItemData singleReqItem, int craftMultiplier, ref int __result)
        {
            need = 0;
            singleReqItem = null;

            if (__instance == null)
            {
                __result = 0;
                return false;
            }

            int num = __instance.m_amount;

            if (__instance.m_requireOnlyOneIngredient)
            {
                var player = Player.m_localPlayer;
                if (player != null)
                {
                    singleReqItem = player.GetFirstRequiredItem(player.GetInventory(), __instance, quality, out need, out var extraAmount, craftMultiplier);
                    if (singleReqItem != null)
                    {
                        num += (int)Mathf.Ceil((float)((singleReqItem.m_quality - 1) * __instance.m_amount) * __instance.m_qualityResultAmountMultiplier) + extraAmount;
                    }
                }
            }

            __result = num * craftMultiplier;
            return false;
        }
    }
}
