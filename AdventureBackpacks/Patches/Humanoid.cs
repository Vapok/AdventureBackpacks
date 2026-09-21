using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Features;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Patches;

public class HumanoidPatches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    static class HumanoidUpdateEquipmentStatusEffectsPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

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

            var ldlocInstruction = new CodeInstruction(OpCodes.Ldloc_0); 

            for (int i = 0; i < instrs.Count; ++i)
            {

                yield return LogMessage(instrs[i]);
                counter++;

                if (i > 0 && (instrs[i].opcode == OpCodes.Stloc_0 || instrs[i].opcode == OpCodes.Stloc_S || instrs[i].opcode == OpCodes.Stloc) && instrs[i - 1].opcode == OpCodes.Newobj)
                {
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldlocInstruction);
          
                    yield return LogMessage(ldlocInstruction);
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;
          
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(EquipmentEffectCache), nameof(EquipmentEffectCache.AddActiveBackpackEffects))));
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Stloc_0));
                    counter++;
                    
                    patchedSuccess = true;
                }
            }

            if (!patchedSuccess)
            {
                AdventureBackpacks.Log.Error($"{nameof(Humanoid.UpdateEquipmentStatusEffects)} Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }
    }
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    static class HumanoidUnequipItemPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Prefix(ItemDrop.ItemData __0)
        {
            try
            {
                if (__0 is null || Player.m_localPlayer == null)
                    return;

                var scene = SceneManager.GetActiveScene();
                if (scene.name == null || string.Equals(scene.name, "start"))
                    return;

                var player = Player.m_localPlayer;
                var item = __0;

                if (item.IsBackpack() && player.m_shoulderItem == item)
                {
                    var backpackInventory = player.GetEquippedBackpack();
                    if (backpackInventory is null) return;

                    //Save Backpack
                    backpackInventory.Save();

                    var inventoryGui = InventoryGui.instance;

                    // Close the backpack inventory if it's currently open
                    if (inventoryGui != null && inventoryGui.IsContainerOpen())
                    {
                        inventoryGui.CloseContainer();
                        InventoryGuiPatches.BackpackIsOpen = false;
                    }
                    
                    player.DestroyBackpackContainerProxy();
                    InventoryGuiPatches.BackpackEquipped = false;
                }
            }
            catch (System.Exception ex)
            {
                AdventureBackpacks.Log?.Warning($"Error during Humanoid.UnequipItem: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    static class HumanoidEquipItemPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        static void Postfix(Humanoid __instance, ItemDrop.ItemData __0, bool __result)
        {
            try
            {
                AdventureBackpacks.Log.Debug($"##########   EquipItem Start");
                if (__0 is null || !__result || __instance == null || Player.m_localPlayer == null || __instance != Player.m_localPlayer)
                    return;
                
                var scene = SceneManager.GetActiveScene();
                if (scene.name == null || string.Equals(scene.name, "start"))
                    return;
                
                var player = Player.m_localPlayer;
                var item = __0;

                if (item.IsBackpack() && item.TryGetBackpackItem(out var backpack))
                {
                    InventoryGuiPatches.BackpackEquipped = true;
                    
                    var backpackItem = item.Data().GetOrCreate<BackpackComponent>();
                    
                    if (!backpackItem.IsEmptyingBackpack)
                    {
                        var size = backpack.GetInventorySize(backpackItem.Item.m_quality);
                        if (backpackItem.InventoryNeedsValidating(size))
                        {
                            Backpacks.ValidateBackpackInventorySizing(player, backpackItem.Item);
                        }
                        else
                        {
                            var backpackContainer = player.GetBackpackContainerProxy(false);
                            if (backpackContainer != null)
                                backpackItem.UpdateContainerSizing(ref backpackContainer);
                        }
                    }
                }
                AdventureBackpacks.Log.Debug($"##########   EquipItem End");
            }
            catch (System.Exception ex)
            {
                AdventureBackpacks.Log?.Warning($"Error during Humanoid.EquipItem: {ex.Message}");
            }
        }
    }
}
