using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Vapok.Common.Managers;

namespace AdventureBackpacks.Patches;

internal static class InventoryGuiPatches
{
    public static bool BackpackIsOpen;
    public static bool BackpackEquipped = false;
    private static bool _showBackpack = false;

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

    public static bool CheckForTextInput()
    {
        var textInputVisible = false;
        var textInputPanel = GameObject.Find("_GameMain/LoadingGUI/PixelFix/IngameGui(Clone)/TextInput/panel");
        
        if (textInputPanel != null)
        {
            if (textInputPanel.activeInHierarchy)
                textInputVisible = true;
        }
        
        return textInputVisible;
    }
    
    public static void ShowBackpack(Player player, InventoryGui instance)
    {
        if (ConfigRegistry.OpenWithInventory.Value && !BackpackIsOpen && player.CanOpenBackpack() && !ConfigRegistry.OpenWithHoverInteract.Value)
        {
            _showBackpack = true;
        }
    
        if (_showBackpack)
        { 
            if (!BackpackIsOpen && instance.m_currentContainer != null)
            {
                instance.m_currentContainer.SetInUse(false);
                instance.m_currentContainer = null;
            }

            _showBackpack = false; 
            player.OpenBackpack(instance);
        }
    }
    
    public static void HideBackpack(InventoryGui instance)
    {
        if (BackpackIsOpen)
        {
            instance.CloseContainer();
            BackpackIsOpen = false;
            
            if (ConfigRegistry.CloseInventory.Value)
                instance.Hide();
        }
    }

    public static bool DetectInputToHide(Player player, InventoryGui instance)
    {
        var hotKeyDown = ZInput.GetKeyDown(ConfigRegistry.HotKeyOpen.Value.MainKey);
        var hotKeyDownOnClose = ConfigRegistry.CloseInventory.Value && hotKeyDown;
        var hotKeyDrop = ConfigRegistry.OutwardMode.Value && ZInput.GetKeyDown(ConfigRegistry.HotKeyDrop.Value.MainKey);

        var openBackpack = hotKeyDown && !BackpackIsOpen && player.CanOpenBackpack() && !ConfigRegistry.OpenWithHoverInteract.Value;

        if (hotKeyDown && ConfigRegistry.OpenWithHoverInteract.Value && !CheckForTextInput())
        {
            var grids = new List<InventoryGrid>();
            grids.AddRange(instance.m_player.GetComponentsInChildren<InventoryGrid>());

            ItemDrop.ItemData hoveredItem = null;
            
            foreach (var grid in grids)
            {
                if (grid.GetHoveredElement() == null)
                    continue;
                
                var hoveredElement = grid.GetHoveredElement();
                hoveredItem = grid.GetInventory().GetItemAt(hoveredElement.m_pos.x, hoveredElement.m_pos.y);
            }

            if (ZInput.IsGamepadActive() && hoveredItem == null)
            {
                foreach (var grid in grids)
                {
                    if (grid.GetGamepadSelectedItem() == null)
                        continue;
                    hoveredItem = grid.GetGamepadSelectedItem();
                }
            }
            
            if (hoveredItem != null && hoveredItem.IsBackpack() && hoveredItem.m_equiped && !BackpackIsOpen &&
                player.CanOpenBackpack())
            {
                openBackpack = true;
            }
        }

        if (openBackpack & !CheckForTextInput())
        {
            if (instance.m_currentContainer != null)
            {
                instance.m_currentContainer.SetInUse(false);
                instance.m_currentContainer = null;
            }
            player.OpenBackpack(instance);
            return false;
        }
        
        if (hotKeyDown && BackpackIsOpen && (!hotKeyDownOnClose || ConfigRegistry.OpenWithHoverInteract.Value) && !CheckForTextInput())
        {
            instance.CloseContainer();
            BackpackIsOpen = false;
            return false;
        }
       
        if (hotKeyDrop && !CheckForTextInput())
        {
            player.QuickDropBackpack();
        }

        return ((hotKeyDownOnClose && !ConfigRegistry.OpenWithHoverInteract.Value) || hotKeyDrop) && !CheckForTextInput();
    }
    
    public static bool DetectInputToShow(Player player, InventoryGui instance)
    {
        var hotKeyDown = ZInput.GetKeyDown(ConfigRegistry.HotKeyOpen.Value.MainKey);
        var hotKeyDrop = ConfigRegistry.OutwardMode.Value && ZInput.GetKeyDown(ConfigRegistry.HotKeyDrop.Value.MainKey);

        if (hotKeyDrop && !CheckForTextInput())
        {
            player.QuickDropBackpack();
        }

        if (hotKeyDown && !BackpackIsOpen && player.CanOpenBackpack() && !ConfigRegistry.OpenWithHoverInteract.Value && !CheckForTextInput())
        {
            _showBackpack = true;
        }
        
        return ((hotKeyDown && !ConfigRegistry.OpenWithHoverInteract.Value) || _showBackpack) && !CheckForTextInput();
    }
   
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdateTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var instrs = instructions.ToList();

            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }
            
            CodeInstruction FindInstructionWithLabel(List<CodeInstruction> codeInstructions, int index, Label label)
            {
                if (index >= codeInstructions.Count)
                    return null;
                
                if (codeInstructions[index].labels.Contains(label))
                    return codeInstructions[index];
                
                return FindInstructionWithLabel(codeInstructions, index + 1, label);
            }

            var resetButtonStatus = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.ResetButtonStatus));
            var menuVisibleMethod = AccessTools.DeclaredMethod(typeof(Menu), nameof(Menu.IsVisible));
            var hideMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Hide));
            var showMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Show));
            var inputKeyDown = AccessTools.DeclaredMethod(typeof(Input), nameof(Input.GetKeyDown), new []{typeof(KeyCode)});
            var zInputButtonDown = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.GetButtonDown), new []{typeof(string)});

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
                    
                    //InventoryGui Argument.
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;

                    //Patch Call Method for Hiding.
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(ShowBackpack))));
                    counter++;
                } else if (i > 6 && instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(inputKeyDown) && instrs[i - 1].operand.Equals((sbyte)KeyCode.Escape) && instrs[i + 2].opcode == OpCodes.Ldstr && instrs[i + 2].operand.Equals("Use"))
                {

                    //1. Output current spot.
                    yield return LogMessage(instrs[i]);
                    counter++;

                    //2. Output i + 1 (this is the brtrue).
                    yield return LogMessage(instrs[i + 1]);
                    counter++;

                    //3. Grab label from brtrue.
                    Label originalLabel = (Label)instrs[i + 1].operand;
                    
                    //4. Look ahead and find instruction with label.
                    var instWithLabel = FindInstructionWithLabel(instrs, i + 2, originalLabel);
                    
                    if (instWithLabel == null)
                    {
                        AdventureBackpacks.Log.Error($"Can't Find Instruction with Label {originalLabel}");
                        continue;
                    }

                    i++;
                    
                    //5. Generate new label.
                    var detectHideLabel = ilGenerator.DefineLabel();
                    
                    //6. Save Label to instruction ahead.
                    instWithLabel.labels.Add(detectHideLabel);
                    
                    //7. Write Player Var
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_1));
                    counter++;
                    
                    //8. Write LdArg Var
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;
                    
                    //9. Write Call instruction
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(DetectInputToHide))));
                    counter++;

                    //10. Write Brture instruction with new label
                    yield return LogMessage(new CodeInstruction(OpCodes.Brtrue, detectHideLabel));
                    counter++;

                } else if (i > 6 && instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(zInputButtonDown) 
                           && instrs[i - 1].operand.Equals("Inventory") && instrs[i + 1].opcode == OpCodes.Brtrue 
                           && instrs[i + 2].opcode == OpCodes.Ldstr && instrs[i + 2].operand.Equals("JoyButtonY"))
                {
                    //1. Output current spot.
                    yield return LogMessage(instrs[i]);
                    counter++;

                    //2. Output i + 1 (this is the brtrue).
                    yield return LogMessage(instrs[i + 1]);
                    counter++;

                    //3. Grab label from brtrue.
                    Label originalLabel = (Label)instrs[i + 1].operand;
                    
                    //4. Look ahead and find instruction with label.
                    var instWithLabel = FindInstructionWithLabel(instrs, i + 2, originalLabel);

                    if (instWithLabel == null)
                    {
                        AdventureBackpacks.Log.Error($"Can't Find Instruction with Label {originalLabel}");
                        continue;
                    }
                    
                    i++;
                    
                    //5. Generate new label.
                    var detectShowLabel = ilGenerator.DefineLabel();
                    
                    //6. Save Label to instruction ahead.
                    instWithLabel.labels.Add(detectShowLabel);
                    
                    //7. Write Player Var
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_1));
                    counter++;
                    
                    //8. Write LdArg Var
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;
                    
                    //9. Write Call instruction
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(DetectInputToShow))));
                    counter++;

                    //10. Write Brture instruction with new label
                    yield return LogMessage(new CodeInstruction(OpCodes.Brtrue, detectShowLabel));
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