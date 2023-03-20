using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Patches;

public class ItemDropPatches
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetWeight))]
    static class ItemDataGetWeightTranspiler
    {
        public static float OverrideBackpackWeight(ItemDrop.ItemData item, float originalWeight)
        {
            var returnedWeight = originalWeight;

            if (!string.IsNullOrEmpty(item.m_shared.m_name) && item.TryGetBackpackItem(out var backpack))
            {
                    
                // If the item in GetWeight() is a backpack, call GetTotalWeight() on its Inventory.
                // Note that GetTotalWeight() just returns a the value of m_totalWeight, and doesn't do any calculation on its own.
                // If the Inventory has been changed at any point, it calls UpdateTotalWeight(), which should ensure that its m_totalWeight is accurate.
                var inventoryWeight = item.Data().GetOrCreate<BackpackComponent>().GetInventory()?.GetTotalWeight() ?? 0;

                // To the backpack's item weight, add the backpack's inventory weight multiplied by the weightMultiplier in the configs.
                returnedWeight += inventoryWeight * backpack.WeightMultiplier.Value;
            }

            return returnedWeight;
        }
        
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instrs = instructions.ToList();

            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            var scaleWeightByQualityField = AccessTools.DeclaredField(typeof(ItemDrop.ItemData.SharedData),"m_scaleWeightByQuality");

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 6 && instrs[i].opcode == OpCodes.Ldloc_0 && instrs[i-1].opcode == OpCodes.Stloc_0 && instrs[i-2].opcode == OpCodes.Add &&
                    instrs[i - 3].opcode == OpCodes.Mul && instrs[i - 4].opcode == OpCodes.Ldfld &&
                    instrs[i - 4].operand.Equals(scaleWeightByQualityField))
                {
                    //Call to Hide Backpack
                    var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldArgInstruction);

                    //Insert new instructions first.

                    //Patch ldarg_0 this is instance of ItemData.
                    yield return LogMessage(ldArgInstruction);
                    counter++;
                    
                    //Get Weight which is ldloc0
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_0));
                    counter++;

                    //Patch Call Method for Overriding the Weight.
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ItemDataGetWeightTranspiler), nameof(OverrideBackpackWeight))));
                    counter++;
                    
                    //Set Weight which is stloc0
                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_0));
                    counter++;
                    
                    //Output current Operation
                    yield return LogMessage(instrs[i]);
                    counter++;
                } 
                else
                {
                    yield return LogMessage(instrs[i]);
                    counter++;
                }
            }
        }
    }

}