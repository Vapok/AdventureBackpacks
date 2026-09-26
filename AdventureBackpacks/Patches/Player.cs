using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class PlayerPatches
{

    [Obsolete("Legacy reflection shim maintained for backward compatibility. Use CraftingContext and CraftFromBackpack directly.")]
    public static int AdjustCountIfEquipped(int itemCount, Player player, Piece.Requirement resource, int quality = -1)
    {
        int num = itemCount;

        if (player == null || resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null)
            return num;

        string itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return num;

        if (num > 0 && resource.m_resItem.m_itemData.IsEquipable())
        {
            Inventory inventory = player.GetInventory();
            List<ItemDrop.ItemData> equippedItems = inventory?.GetEquippedItems();
            if (equippedItems != null && equippedItems.Any(x => x != null && x.m_shared != null && string.Equals(x.m_shared.m_name, itemName)))
            {
                num -= 1;
            }
        }

        if (CraftFromBackpack.CanCraftFromBackpack(player, out Inventory _))
        {
            num += CraftFromBackpack.GetBackpackItemCount(player, itemName, quality);
        }

        return num;
    }

    [Obsolete("Legacy reflection shim maintained for backward compatibility. Use CraftingContext and CraftFromBackpack directly.")]
    public static int AdjustCountIfEquipped(int itemCount, Player player, Piece.Requirement resource)
    {
        return AdjustCountIfEquipped(itemCount, player, resource, -1);
    }

    [Obsolete("Legacy reflection shim maintained for backward compatibility. Use CraftingContext and CraftFromBackpack directly.")]
    public static int AdjustCountIfEquipped(Player player, Piece.Requirement resource, int itemCount)
    {
        return AdjustCountIfEquipped(itemCount, player, resource, -1);
    }

    [Obsolete("Legacy reflection shim maintained for backward compatibility. Use CraftingContext and CraftFromBackpack directly.")]
    public static int ConsumeUnEquippedItems(int amount, Player player, Piece.Requirement resource)
    {
        if (amount < 1 || player == null || resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null || PlayerExtensions.IsDedicatedOrHeadless())
            return amount;
            
        string itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return amount;

        if (CraftFromBackpack.CanCraftFromBackpack(player, out Inventory _))
        {
            return CraftFromBackpack.ConsumeCraftingItem(player, itemName, amount);
        }

        return amount;
    }

    [Obsolete("Legacy reflection shim maintained for backward compatibility. Use CraftingContext and CraftFromBackpack directly.")]
    public static int ConsumeUnEquippedItems(Player player, Piece.Requirement resource, int amount)
    {
        return ConsumeUnEquippedItems(amount, player, resource);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), new[] { typeof(Piece), typeof(Player.RequirementMode) })]
    static class PlayerHaveRequirementsPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPrefix]
        [HarmonyPriority(900)]
        static void Prefix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Enter();
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(100)]
        static void Finalizer(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Exit();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), new[] { typeof(Recipe), typeof(bool), typeof(int), typeof(int) })]
    static class PlayerHaveRequirementsRecipePatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPrefix]
        [HarmonyPriority(900)]
        static void Prefix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Enter();
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(100)]
        static void Finalizer(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Exit();
            }
        }
    }

    [HarmonyPatch(typeof(Player), "HaveRequirementItems", new[] { typeof(Recipe), typeof(bool), typeof(int), typeof(int) })]
    static class PlayerHaveRequirementItemsPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPrefix]
        [HarmonyPriority(900)]
        static void Prefix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Enter();
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(100)]
        static void Finalizer(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Exit();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacement))]
    static class PlayerUpdatePlacementPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPrefix]
        [HarmonyPriority(900)]
        static void Prefix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Enter();
            }
        }

        [HarmonyFinalizer]
        [HarmonyPriority(100)]
        static void Finalizer(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Exit();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetFirstRequiredItem))]
    static class PlayerGetFirstRequiredItemPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPostfix]
        static void Postfix(Player __instance, Inventory inventory, Recipe recipe, int qualityLevel, ref int amount, ref int extraAmount, int craftMultiplier, ref ItemDrop.ItemData __result)
        {
            if (__result != null)
                return;

            if (recipe == null || recipe.m_resources == null)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(__instance, out Inventory bpInventory))
                return;

            CraftingStation currentCraftingStation = __instance.GetCurrentCraftingStation();
            Piece.Requirement[] resources = recipe.m_resources;

            foreach (Piece.Requirement requirement in resources)
            {
                if ((currentCraftingStation != null && currentCraftingStation.m_upgrader != requirement.m_upgraderResource) ||
                    (currentCraftingStation == null && requirement.m_upgraderResource) ||
                    !requirement.m_resItem || requirement.m_resItem.m_itemData == null || requirement.m_resItem.m_itemData.m_shared == null)
                {
                    continue;
                }

                int neededAmount = requirement.GetAmount(qualityLevel) * craftMultiplier;
                if (neededAmount <= 0)
                {
                    continue;
                }

                string reqName = requirement.m_resItem.m_itemData.m_shared.m_name;
                for (int q = 1; q <= requirement.m_resItem.m_itemData.m_shared.m_maxQuality; q++)
                {
                    int bpCount = bpInventory.CountItems(reqName, q);
                    if (CraftFromBackpack.LeaveOneItemInBackpack != null && CraftFromBackpack.LeaveOneItemInBackpack.Value && bpCount > 0)
                    {
                        bpCount = Math.Max(0, bpCount - 1);
                    }

                    int count = inventory.CountItems(reqName, q) + bpCount;
                    if (count >= neededAmount)
                    {
                        List<ItemDrop.ItemData> allBpItems = bpInventory.GetAllItems();
                        ItemDrop.ItemData matchingItem = allBpItems?.FirstOrDefault(x => 
                            x != null &&
                            x.m_shared != null && 
                            string.Equals(x.m_shared.m_name, reqName) && 
                            x.m_quality == q && 
                            x.m_stack >= neededAmount);

                        if (matchingItem == null)
                        {
                            matchingItem = allBpItems?.FirstOrDefault(x => 
                                x != null &&
                                x.m_shared != null && 
                                string.Equals(x.m_shared.m_name, reqName) && 
                                x.m_quality == q);
                        }

                        if (matchingItem != null)
                        {
                            amount = neededAmount;
                            extraAmount = requirement.m_extraAmountOnlyOneIngredient;
                            __result = matchingItem;
                            return;
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDestroy))]
    static class PlayerOnDestroyPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPostfix]
        static void Postfix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Reset();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    static class PlayerOnDeathPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [HarmonyPostfix]
        static void Postfix(Player __instance)
        {
            if (__instance != null && __instance == Player.m_localPlayer)
            {
                CraftingContext.Reset();
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
    internal static class PlayerUpdateEnvStatusEffectsPatch
    {
        public static bool IsUpdatingEnvStatusEffects { get; private set; }

        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        private static void Prefix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
                IsUpdatingEnvStatusEffects = true;
        }

        private static void Postfix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                IsUpdatingEnvStatusEffects = false;

                foreach (KeyValuePair<BackpackEffect, Assets.Effects.EffectsBase> kvp in EffectsFactory.EffectList)
                {
                    kvp.Value.OnUpdateEnvStatusEffects(__instance);
                }
            }
        }

        private static void Finalizer()
        {
            IsUpdatingEnvStatusEffects = false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
    internal static class PlayerApplyArmorDamageModsPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        private static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
        {
            if (__instance == null || Player.m_localPlayer == null || __instance != Player.m_localPlayer)
                return;

            if (__instance.m_shoulderItem == null || __instance.m_shoulderItem.IsBackpack())
            {
                Inventory inventory = __instance.GetInventory();
                List<ItemDrop.ItemData> equippedItems = inventory?.GetEquippedItems();
                if (equippedItems != null)
                {
                    for (int i = 0; i < equippedItems.Count; i++)
                    {
                        ItemDrop.ItemData item = equippedItems[i];
                        if (item != null && item != __instance.m_shoulderItem && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder && !item.IsBackpack())
                        {
                            mods.Apply(item.m_shared.m_damageModifiers);
                        }
                    }
                }
            }
        }
    }
}

