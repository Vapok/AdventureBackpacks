using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class HumanoidPatches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    static class HumanoidUpdateEquipmentStatusEffectsPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instrs = instructions.ToList();

            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            var ldlocInstruction = new CodeInstruction(OpCodes.Ldloc_0); 

            for (int i = 0; i < instrs.Count; ++i)
            {

                yield return LogMessage(instrs[i]);
                counter++;

                //In Humanoid.UpdateEquipmentStatusEffects, Local Variable 0, or loc_0 is the target HashSet variable 
                //listed as "other".  So, patching after Stloc_0 is called immediately after Newobj, which creates the object.
                if (instrs[i].opcode == OpCodes.Stloc_0 && instrs[i-1].opcode == OpCodes.Newobj)
                {
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldlocInstruction);
          
                    //Patch the ldloc_0 which is the argument of my method using local variable 0.
                    yield return LogMessage(ldlocInstruction);
                    counter++;
          
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(EquipmentEffectCache), nameof(EquipmentEffectCache.AddActiveBackpackEffects))));
                    counter++;

                    //Save output of calling method to local variable 0
                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_0));
                    counter++;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    static class HumanoidUnequipItemPatch
    {
        // The "__instance" here is a Humanoid type, but we want the ItemData argument, so we use "__0" instead.
        // "__0" fetches the argument passed into the first parameter of the original method, which in this case is an ItemData object.
        static void Prefix(ItemDrop.ItemData __0)
        {
            if (__0 is null) return;
            
            if ( Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;

            var item = __0;

            // Check if the item being unequipped is a backpack, and see if it is the same backpack the player is wearing
            if (item.IsBackpack() && player.m_shoulderItem == item)
            {
                var backpackInventory = player.GetEquippedBackpack();
                if (backpackInventory is null) return;

                //Save Backpack
                backpackInventory.Save();

                var inventoryGui = InventoryGui.instance;

                // Close the backpack inventory if it's currently open
                if (inventoryGui.IsContainerOpen())
                {
                    inventoryGui.CloseContainer();
                    InventoryGuiPatches.BackpackIsOpen = false;
                }

                InventoryGuiPatches.BackpackEquipped = false;
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    static class HumanoidEquipItemPatch
    {
        static void Postfix(ItemDrop.ItemData __0, bool __result)
        {
            if (__0 is null) return;
            
            if ( Player.m_localPlayer == null && !__result)
                return;

            var item = __0;

            if (item.IsBackpack())
            {
                InventoryGuiPatches.BackpackEquipped = true;
            }
        }
    }
}