using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class PlayerPatches
{

    public static int AdjustCountIfEquipped(int itemCount, Player player, Piece.Requirement resource, int quality = -1)
    {
        var num = itemCount;

        if (resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null)
            return num;

        var itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return num;

        // Deduct 1 if the player is actively equipping this item
        if (num > 0 && resource.m_resItem.m_itemData.IsEquipable())
        {
            var inventory = player?.GetInventory();
            var equippedItems = inventory?.GetEquippedItems();
            if (equippedItems != null && equippedItems.Any(x => x != null && x.m_shared != null && string.Equals(x.m_shared.m_name, itemName)))
            {
                num -= 1;
            }
        }

        // Add backpack materials if CraftFromBackpack is active
        if (CraftFromBackpack.CanCraftFromBackpack(player, out _))
        {
            num += CraftFromBackpack.GetBackpackItemCount(player, itemName, quality);
        }

        return num;
    }

    public static int AdjustCountIfEquipped(int itemCount, Player player, Piece.Requirement resource)
    {
        return AdjustCountIfEquipped(itemCount, player, resource, -1);
    }

    public static int AdjustCountIfEquipped(Player player, Piece.Requirement resource, int itemCount)
    {
        return AdjustCountIfEquipped(itemCount, player, resource, -1);
    }

    public static int ConsumeUnEquippedItems(int amount, Player player, Piece.Requirement resource)
    {
        if (amount < 1 || player == null || resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null)
            return amount;
            
        var itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return amount;

        // If Craft From Backpack is enabled and active, consume from player inventory first, then backpack
        if (CraftFromBackpack.CanCraftFromBackpack(player, out _))
        {
            return CraftFromBackpack.ConsumeCraftingItem(player, itemName, amount);
        }

        if (!resource.m_resItem.m_itemData.IsEquipable())
            return amount;

        var allItems = player?.m_inventory?.GetAllItems();
        if (allItems == null)
            return amount;

        var resourceItems = allItems.Where(x => x != null && x.m_shared != null && string.Equals(x.m_shared.m_name, itemName)).ToList();

        var removedCounter = 0;
        for (int i = 0; i < amount; i++)
        {
            foreach (var item in resourceItems)
            {
                if (item.m_equipped)
                    continue;

                if (removedCounter < amount)
                {
                    player.m_inventory.RemoveItem(item, 1);
                    removedCounter++;
                }
            }
        }

        return amount - removedCounter;
    }

    public static int ConsumeUnEquippedItems(Player player, Piece.Requirement resource, int amount)
    {
        return ConsumeUnEquippedItems(amount, player, resource);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirements), new[] { typeof(Piece), typeof(Player.RequirementMode) })]
    static class PlayerHaveRequirementsPatch
    {
        static void Postfix(Player __instance, Piece piece, Player.RequirementMode mode, ref bool __result)
        {
            // If already satisfied (by vanilla inventory or container mods like ValheimPlus/ItemDrawers), do nothing!
            if (__result)
                return;

            if (piece == null || !CraftFromBackpack.CanCraftFromBackpack(__instance, out var bpInventory))
                return;

            if (mode == Player.RequirementMode.IsKnown)
                return;

            if (piece.m_craftingStation != null)
            {
                if (mode == Player.RequirementMode.CanAlmostBuild)
                {
                    if (!__instance.m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
                        return;
                }
                else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, __instance.transform.position) && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoWorkbench))
                {
                    return;
                }
            }

            if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
                return;

            var resources = piece.m_resources;
            if (resources == null)
            {
                __result = true;
                return;
            }

            foreach (var requirement in resources)
            {
                if (!requirement.m_resItem || requirement.m_amount <= 0)
                    continue;

                var itemName = requirement.m_resItem.m_itemData?.m_shared?.m_name;
                if (string.IsNullOrEmpty(itemName))
                    continue;

                switch (mode)
                {
                    case Player.RequirementMode.CanAlmostBuild:
                        if (!__instance.m_inventory.SafeHaveItem(itemName) && !bpInventory.SafeHaveItem(itemName))
                        {
                            return;
                        }
                        break;

                    case Player.RequirementMode.CanBuild:
                        var count = __instance.m_inventory.CountItems(itemName);
                        count = AdjustCountIfEquipped(count, __instance, requirement);
                        if (count < requirement.m_amount)
                        {
                            return;
                        }
                        break;
                }
            }

            __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirementItems))]
    static class PlayerHaveRequirementItemsPatch
    {
        [HarmonyPostfix]
        static void Postfix(Player __instance, Recipe piece, bool discover, int qualityLevel, int amount, ref bool __result)
        {
            // If already satisfied (by vanilla inventory or container mods like ValheimPlus/ItemDrawers), do nothing!
            if (__result)
                return;

            if (piece == null || !piece.m_requireOnlyOneIngredient)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(__instance, out _))
                return;

            var currentCraftingStation = __instance.GetCurrentCraftingStation();
            var resources = piece.m_resources;
            if (resources == null)
                return;

            foreach (var requirement in resources)
            {
                if ((!discover && currentCraftingStation != null && currentCraftingStation.m_upgrader != requirement.m_upgraderResource) ||
                    (currentCraftingStation == null && requirement.m_upgraderResource) ||
                    !requirement.m_resItem || requirement.m_resItem.m_itemData == null || requirement.m_resItem.m_itemData.m_shared == null)
                {
                    continue;
                }

                if (discover)
                {
                    if (requirement.m_amount <= 0)
                        continue;

                    if (__instance.IsMaterialKnown(requirement.m_resItem.m_itemData.m_shared.m_name))
                    {
                        __result = true;
                        return;
                    }
                    continue;
                }

                int neededAmount = requirement.GetAmount(qualityLevel) * amount;
                if (neededAmount <= 0)
                {
                    continue;
                }

                int maxInInventory = 0;
                for (int q = 1; q <= requirement.m_resItem.m_itemData.m_shared.m_maxQuality; q++)
                {
                    int count = __instance.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, q);
                    count = AdjustCountIfEquipped(count, __instance, requirement, q);
                    if (count > maxInInventory)
                    {
                        maxInInventory = count;
                    }
                }

                if (maxInInventory >= neededAmount)
                {
                    __result = true;
                    return;
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var patchedSuccess = false;
            var instrs = instructions.ToList();
            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            var countItemsMethod = AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.CountItems), new[] { typeof(string), typeof(int), typeof(bool) });

            for (int i = 0; i < instrs.Count; ++i)
            {
                yield return LogMessage(instrs[i]);
                counter++;

                if (instrs[i].opcode == OpCodes.Callvirt && 
                    (instrs[i].operand.Equals(countItemsMethod) || (instrs[i].operand is MethodInfo m && m.Name == nameof(Inventory.CountItems))))
                {
                    CodeInstruction ldLocReq = null;
                    for (int j = i - 1; j >= Math.Max(0, i - 15); j--)
                    {
                        if (instrs[j].opcode == OpCodes.Ldfld && instrs[j].operand is FieldInfo fi && fi.Name == nameof(Piece.Requirement.m_resItem))
                        {
                            ldLocReq = new CodeInstruction(instrs[j - 1].opcode, instrs[j - 1].operand);
                            break;
                        }
                    }

                    if (ldLocReq == null)
                    {
                        ldLocReq = new CodeInstruction(OpCodes.Ldloc_3);
                    }

                    // CountItems left [int itemCount] on top of the stack.
                    // We push Player (ldarg.0) and Requirement (ldLocReq) and call AdjustCountIfEquipped(itemCount, player, resource) -> returns adjusted int.
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;

                    yield return LogMessage(ldLocReq);
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlayerPatches), nameof(AdjustCountIfEquipped), new[] { typeof(int), typeof(Player), typeof(Piece.Requirement) })));
                    counter++;

                    patchedSuccess = true;
                }
            }
            
            if (!patchedSuccess)
            {
                AdventureBackpacks.Log.Error($"{nameof(Player.HaveRequirementItems)} Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources))]
    static class PlayerConsumeResourcesPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patchedSuccess = false;
            var instrs = instructions.ToList();
            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            var getAmountMethod = AccessTools.DeclaredMethod(typeof(Piece.Requirement), "GetAmount", new[] { typeof(int) }); 

            for (int i = 0; i < instrs.Count; ++i)
            {
                yield return LogMessage(instrs[i]);
                counter++;

                if (instrs[i].opcode == OpCodes.Mul && i >= 2)
                {
                    int getAmountIndex = -1;
                    for (int j = i - 1; j >= Math.Max(0, i - 5); j--)
                    {
                        if (instrs[j].opcode == OpCodes.Callvirt && 
                            (instrs[j].operand.Equals(getAmountMethod) || (instrs[j].operand is MethodInfo m && m.Name == nameof(Piece.Requirement.GetAmount))))
                        {
                            getAmountIndex = j;
                            break;
                        }
                    }

                    if (getAmountIndex >= 0)
                    {
                        CodeInstruction ldLocReq = null;
                        for (int k = getAmountIndex - 1; k >= Math.Max(0, getAmountIndex - 5); k--)
                        {
                            if (instrs[k].opcode == OpCodes.Ldloc_3 || instrs[k].opcode == OpCodes.Ldloc_S || 
                                instrs[k].opcode == OpCodes.Ldloc || instrs[k].opcode == OpCodes.Ldloc_0 || 
                                instrs[k].opcode == OpCodes.Ldloc_1 || instrs[k].opcode == OpCodes.Ldloc_2)
                            {
                                ldLocReq = new CodeInstruction(instrs[k].opcode, instrs[k].operand);
                                break;
                            }
                        }

                        if (ldLocReq == null)
                        {
                            ldLocReq = new CodeInstruction(OpCodes.Ldloc_3);
                        }

                        // Mul left [int amount] on top of the stack.
                        // We push Player (ldarg.0) and Requirement (ldLocReq) and call ConsumeUnEquippedItems(amount, player, resource) -> returns adjusted amount.
                        yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                        counter++;

                        yield return LogMessage(ldLocReq);
                        counter++;

                        yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlayerPatches), nameof(ConsumeUnEquippedItems), new[] { typeof(int), typeof(Player), typeof(Piece.Requirement) })));
                        counter++;

                        patchedSuccess = true;
                    }
                }
            }
            
            if (!patchedSuccess)
            {
                AdventureBackpacks.Log.Error($"{nameof(Player.ConsumeResources)} Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetFirstRequiredItem))]
    static class PlayerGetFirstRequiredItemPatch
    {
        [HarmonyPostfix]
        static void Postfix(Player __instance, Inventory inventory, Recipe recipe, int qualityLevel, ref int amount, ref int extraAmount, int craftMultiplier, ref ItemDrop.ItemData __result)
        {
            // If vanilla or an external mod already found a valid item, do not interfere!
            if (__result != null)
                return;

            if (recipe == null || recipe.m_resources == null)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(__instance, out var bpInventory))
                return;

            var currentCraftingStation = __instance.GetCurrentCraftingStation();
            var resources = recipe.m_resources;

            foreach (var requirement in resources)
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

                for (int q = 1; q <= requirement.m_resItem.m_itemData.m_shared.m_maxQuality; q++)
                {
                    int count = inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name, q);
                    count = AdjustCountIfEquipped(count, __instance, requirement, q);
                    if (count >= neededAmount)
                    {
                        var allBpItems = bpInventory.GetAllItems();
                        var matchingItem = allBpItems?.FirstOrDefault(x => 
                            x != null &&
                            x.m_shared != null && 
                            string.Equals(x.m_shared.m_name, requirement.m_resItem.m_itemData.m_shared.m_name) && 
                            x.m_quality == q && 
                            x.m_stack >= neededAmount);

                        if (matchingItem == null)
                        {
                            matchingItem = allBpItems?.FirstOrDefault(x => 
                                x != null &&
                                x.m_shared != null && 
                                string.Equals(x.m_shared.m_name, requirement.m_resItem.m_itemData.m_shared.m_name) && 
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
}
