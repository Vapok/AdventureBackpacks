using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class PlayerPatches
{
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    static class PlayerAwakePatch
    {
        static void Postfix(Player __instance)
        {
            __instance.gameObject.AddComponent<Container>();
        }
    }

    public static int AdjustCountIfEquipped(int itemCount, Player player, Piece.Requirement resource)
    {
        var num = itemCount;

        if (num < 1 || resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null || !resource.m_resItem.m_itemData.IsEquipable())
            return num;

        var inventory = player?.GetInventory();
        if (inventory == null)
            return num;
            
        var itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return num;

        var equippedItems = inventory.GetEquippedItems();

        if (equippedItems != null && equippedItems.Any(x => x.m_shared != null && x.m_shared.m_name.Equals(itemName)))
        {
            num -= 1;
        }

        return num;
    }

    public static int AdjustCountIfEquipped(Player player, Piece.Requirement resource, int itemCount)
    {
        return AdjustCountIfEquipped(itemCount, player, resource);
    }

    public static int ConsumeUnEquippedItems(int amount, Player player, Piece.Requirement resource)
    {
        var num = amount;

        if (num < 1 || resource == null || resource.m_resItem == null || resource.m_resItem.m_itemData == null || !resource.m_resItem.m_itemData.IsEquipable())
            return num;
            
        var itemName = resource.m_resItem.m_itemData.m_shared?.m_name;
        if (string.IsNullOrEmpty(itemName))
            return num;

        var allItems = player?.m_inventory?.GetAllItems();
        if (allItems == null)
            return num;

        var resourceItems = allItems.Where(x => x.m_shared != null && x.m_shared.m_name.Equals(itemName)).ToList();

        var removedCounter = 0;
        for (int i = 0; i < num; i++)
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

        return num - removedCounter;
    }

    public static int ConsumeUnEquippedItems(Player player, Piece.Requirement resource, int amount)
    {
        return ConsumeUnEquippedItems(amount, player, resource);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveRequirementItems))]
    static class PlayerHaveRequirementItemsPatch
    {
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
}
