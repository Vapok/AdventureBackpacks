using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using HarmonyLib;
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
                var backpackItem = item.Data().GetOrCreate<BackpackComponent>();

                var size = backpack.GetInventorySize(backpackItem.Item.m_quality);
                
                if (!backpackItem.IsEmptyingBackpack && backpackItem.InventoryNeedsValidating(size))
                {
                    AdventureBackpacks.Log.Debug($"[GetWeight() - Item Name: {item.m_shared.m_name}");
                    AdventureBackpacks.Log.Debug($"[GetWeight() - Backpack Item: {backpackItem.Item.m_shared.m_name}");
                    AdventureBackpacks.Log.Debug($"[GetWeight() - Backpack: {backpack.ItemName}");
                    Backpacks.ValidateBackpackInventorySizing(Player.m_localPlayer, backpackItem.Item);
                }
                
                var inventoryWeight = backpackItem.GetInventory()?.GetTotalWeight() ?? 0;

                returnedWeight += inventoryWeight * backpack.WeightMultiplier.Value;
            }

            return returnedWeight;
        }
        
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

            var scaleWeightByQualityField = AccessTools.DeclaredField(typeof(ItemDrop.ItemData.SharedData),"m_scaleWeightByQuality");

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 6 && instrs[i].opcode == OpCodes.Ldloc_1 && instrs[i-1].opcode == OpCodes.Stloc_1 && instrs[i-2].opcode == OpCodes.Add &&
                    instrs[i - 3].opcode == OpCodes.Mul && instrs[i - 4].opcode == OpCodes.Ldfld &&
                    instrs[i - 4].operand.Equals(scaleWeightByQualityField))
                {
                    var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldArgInstruction);

                    yield return LogMessage(ldArgInstruction);
                    counter++;
                    
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_1));
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ItemDataGetWeightTranspiler), nameof(OverrideBackpackWeight))));
                    counter++;
                    
                    //Set Weight which is stloc0
                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_1));
                    counter++;
                    
                    //Output current Operation
                    yield return LogMessage(instrs[i]);
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
                AdventureBackpacks.Log.Error($"{nameof(ItemDrop.ItemData.GetWeight)} Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }
    }

}