using System.Collections.Generic;
using System.Linq;
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

    public static int AdjustCountIfEquipped(Player player, Piece.Requirement resource, int itemCount)
    {
        var num = itemCount;

        if (num < 1 || !resource.m_resItem.m_itemData.IsEquipable())
            return num;
            
        var itemName = resource.m_resItem.m_itemData.m_shared.m_name;
        var equippedItems = player.GetInventory().GetEquippedItems();

        if (equippedItems.Any(x => x.m_shared.m_name.Equals(itemName)))
        {
            num -= 1;
        }

        return num;
    }

    public static int ConsumeUnEquippedItems(Player player, Piece.Requirement resource, int amount)
    {
        var num = amount;

        if (num < 1 || !resource.m_resItem.m_itemData.IsEquipable())
            return num;
            
        var itemName = resource.m_resItem.m_itemData.m_shared.m_name;
        var resourceItems = player.m_inventory.GetAllItems().Where(x => x.m_shared.m_name.Equals(itemName)).ToList();

        var removedCounter = 0;
        for (int i = 0; i < num; i++)
        {
            foreach (var item in resourceItems)
            {
                if (item.m_equipped)
                    continue;

                if (removedCounter < amount)
                {
                    player.m_inventory.RemoveItem(item,1);
                    removedCounter++;
                }
            }
        }

        return num - removedCounter;
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

            var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
            var countItemsMethod = AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.CountItems), new[] { typeof(string), typeof(int), typeof(bool) });

            
            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 5 && instrs[i].opcode == OpCodes.Stloc_S && instrs[i+1].opcode == OpCodes.Ldarg_1 && instrs[i+2].opcode == OpCodes.Ldfld)
                {
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldArgInstruction);

                    yield return LogMessage(instrs[i]);
                    counter++;
          
                    //Player this
                    yield return LogMessage(ldArgInstruction);
                    counter++;
                    
                    //Piece.Requirement resource
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_2));
                    counter++;
                    
                    //int num
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_S, instrs[i].operand));
                    counter++;
          
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlayerPatches), nameof(AdjustCountIfEquipped))));
                    counter++;

                    //Save output of calling method to local variable 0
                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_S, instrs[i].operand));
                    counter++;
                    
                    patchedSuccess = true;
                }
                else
                {
                    yield return LogMessage(instrs[i]);
                    counter++;
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

            var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
            var getAmountMethod = AccessTools.DeclaredMethod(typeof(Piece.Requirement), "GetAmount", new[] { typeof(int) }); 

            for (int i = 0; i < instrs.Count; ++i)
            {

                yield return LogMessage(instrs[i]);
                counter++;

                if (i > 5 && instrs[i-1].opcode == OpCodes.Callvirt && instrs[i-1].operand.Equals(getAmountMethod) && instrs[i].opcode == OpCodes.Stloc_3)
                {
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldArgInstruction);
          
                    //Player this
                    yield return LogMessage(ldArgInstruction);
                    counter++;
                    
                    //Piece.Requirement resource
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_2));
                    counter++;
                    
                    //int amount
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_3));
                    counter++;
          
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlayerPatches), nameof(ConsumeUnEquippedItems))));
                    counter++;

                    //Save output of calling method to local variable 0
                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_3));
                    counter++;
                    
                    patchedSuccess = true;
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