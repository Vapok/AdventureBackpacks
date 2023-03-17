using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using UnityEngine;
using Vapok.Common.Managers;
using Vapok.Common.Tools;

namespace AdventureBackpacks.Patches;

internal static class InventoryGuiPatches
{
    public static bool BackpackIsOpen;
    public static bool BackpackIsOpening;
    public static bool BackpackEquipped = false;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    static class InventoryGuiDoCraftingPrefix
    {
        static void Postfix(InventoryGui __instance)
        {
            if (__instance.m_craftUpgradeItem != null && __instance.m_craftUpgradeItem.IsBackpack())
            {
                var backpack = __instance.m_craftUpgradeItem.Data().Get<BackpackComponent>();
                backpack?.Load();
                Player.m_localPlayer.UpdateEquipmentStatusEffects();
            }
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
    static class InventoryGuiOnSelectedItem
    {
        static Exception Finalizer(Exception __exception, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod, InventoryGui __instance)
        {

            if (__exception != null)
            {
                if (__exception is NullReferenceException)
                {
                    if (__instance != null && __instance.m_currentContainer == null && grid.GetInventory() != null && Player.m_localPlayer != null)
                    {
                        //Is item a backpack
                        if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                        {
                            Player.m_localPlayer.DropItem(Player.m_localPlayer.GetInventory(), item, 1);
                            BackpackIsOpen = false;
                            __instance.Hide();
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$vapok_mod_you_droped_bag");
                            
                            return null;
                        }
                    }
                }
                AdventureBackpacks.Log.Warning($"The following error was captured by Adventure Backpacks, but was caused by another mod. Advanced Backpacks is going to allow the operation to continue, but is going to replay the error below:");
                AdventureBackpacks.Log.Error($"External Mod Error Message: {__exception.Message}");
                AdventureBackpacks.Log.Error($"External Mod Error Source: {__exception.Source}");
                AdventureBackpacks.Log.Error($"External Mod Error Stack Trace: {__exception.StackTrace}");
                AdventureBackpacks.Log.Warning($"Please check with other mod authors listed above.");
            }
            return null;
        }
    }
    
    public static void ShowBackpack(Player player)
    {
        if (ConfigRegistry.OpenWithInventory.Value && player.CanOpenBackpack())
        {
           player.OpenBackpack(false);
        }
    }
    
    public static void HideBackpack(InventoryGui instance)
    {
        if (ConfigRegistry.OpenWithInventory.Value)
        {
            if (BackpackIsOpen)
            {
                instance.CloseContainer();
                BackpackIsOpen = false;
                
                if (ConfigRegistry.CloseInventory.Value)
                    instance.Hide();
            }
        }
    }

    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdateTranspiler
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

            var resetButtonStatus = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.ResetButtonStatus));
            var hideMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Hide));
            var showMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Show));

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 6 && instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(resetButtonStatus) &&
                    instrs[i + 1].opcode == OpCodes.Ldarg_0 && instrs[i + 2].opcode == OpCodes.Call &&
                    instrs[i + 2].operand.Equals(hideMethod))
                {
                    //Call to Hide Backpack
                    var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldArgInstruction);
                    
                    //Output current Operation
                    yield return LogMessage(instrs[i]);
                    counter++;

                    //Patch ldarg_0 this is instance of InventoryGui.
                    yield return LogMessage(ldArgInstruction);
                    counter++;

                    //Patch Call Method for Hiding.
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(HideBackpack))));
                    counter++;
                } else if (i > 6 && instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(showMethod) &&
                           instrs[i - 1].opcode == OpCodes.Ldc_I4_1 && instrs[i - 2].opcode == OpCodes.Ldnull &&
                           instrs[i - 3].opcode == OpCodes.Ldarg_0)
                {
                    //Call to Show Backpack
                    //Get localPlayer at ldloc.1
                    var localPlayerInstruction = new CodeInstruction(OpCodes.Ldloc_1);
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(localPlayerInstruction);
                    
                    //Output current Operation
                    yield return LogMessage(instrs[i]);
                    counter++;

                    //Patch ldloc_1 this is localPlayer.
                    yield return LogMessage(localPlayerInstruction);
                    counter++;

                    //Patch Call Method for Hiding.
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(ShowBackpack))));
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

    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdatePatch
    {
        static void Postfix(Animator ___m_animator, ref Container ___m_currentContainer)
        {
            if ( !ConfigRegistry.HotKeyOpen.Value.IsDown() || !ZInput.GetButtonDown(ConfigRegistry.HotKeyOpen.Value.Serialize()) || !Player.m_localPlayer || !___m_animator.GetBool("visible"))
                return;

            ZInput.ResetButtonStatus(ConfigRegistry.HotKeyOpen.Value.Serialize());
                
            if (BackpackIsOpening)
            {
                BackpackIsOpening = false;
                return;
            }
            
            if (BackpackIsOpen)
            {
                InventoryGui.instance.CloseContainer();
                BackpackIsOpen = false;
                
                if (ConfigRegistry.CloseInventory.Value)
                    InventoryGui.instance.Hide();
                
                return;
            }
            
            if (Player.m_localPlayer.CanOpenBackpack())
            {
                Player.m_localPlayer.OpenBackpack(false);
            }
        }
    }
}