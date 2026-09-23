using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Vapok.Common.Managers;
using AdventureBackpacks.Features;

namespace AdventureBackpacks.Patches;

internal static class InventoryGuiPatches
{
    public static bool BackpackIsOpen;
    public static bool BackpackEquipped = false;
    private static bool _showBackpack ;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    static class InventoryGuiDoCraftingPrefix
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        static void Prefix(InventoryGui __instance)
        {
            AdventureBackpacks.Log.Debug($"########################################");
            AdventureBackpacks.Log.Debug($"####       DoCrafting.Prefix       #####");
            AdventureBackpacks.Log.Debug($"########################################");
            InventoryPatches.IsDoingCrafting = true;
            
        }
        static void Postfix(InventoryGui __instance)
        {
            AdventureBackpacks.Log.Debug($"########################################");
            AdventureBackpacks.Log.Debug($"####       DoCrafting.Postfix      #####");
            AdventureBackpacks.Log.Debug($"########################################");

            InventoryPatches.IsDoingCrafting = false;
            if ( Player.m_localPlayer == null)
                return;
            var player = Player.m_localPlayer;
            
            if (__instance.m_craftUpgradeItem != null && __instance.m_craftUpgradeItem.IsBackpack())
            {
                AdventureBackpacks.Log.Debug($"Item: {__instance.m_craftUpgradeItem.m_shared.m_name} ");

                var backpack = __instance.m_craftUpgradeItem.Data().Get<BackpackComponent>();
                AdventureBackpacks.Log.Debug($"Backpack: {__instance.m_craftUpgradeItem.m_shared.m_name} ");
                if (backpack == null)
                    return;

                backpack?.Load();

                if (player.IsThisBackpackEquipped(backpack.Item))
                {
                    var backpackContainer = player.GetBackpackContainerProxy();
                    backpack?.UpdateContainerSizing(ref backpackContainer);
                }
                    
                
                player.UpdateEquipmentStatusEffects();
            }
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
    static class InventoryGuiOnSelectedItem
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        static Exception Finalizer(Exception __exception, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod, InventoryGui __instance)
        {

            if (__exception != null)
            {
                if (__exception is NullReferenceException)
                {
                    if (__instance != null && __instance.m_currentContainer == null && grid != null && grid.GetInventory() != null && Player.m_localPlayer != null
                        && item != null && item.m_shared != null
                        && Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                    {
                            Player.m_localPlayer.DropItem(Player.m_localPlayer.GetInventory(), item, 1);
                            BackpackIsOpen = false;
                            __instance.Hide();
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$vapok_mod_you_droped_bag");
                            
                            return null;
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

    private static bool _hasLoggedTextInputError;

    public static bool CheckForTextInput()
    {
        try
        {
            return TextInput.IsVisible();
        }
        catch (Exception ex)
        {
            if (!_hasLoggedTextInputError)
            {
                _hasLoggedTextInputError = true;
                AdventureBackpacks.Log.Error($"Exception in CheckForTextInput: {ex}");
            }
            return false;
        }
    }
    
    public static void ShowBackpack(Player player, InventoryGui instance)
    {
        if (player == null || instance == null)
            return;

        if (ConfigRegistry.OpenWithInventory.Value && !BackpackIsOpen && player.CanOpenBackpack())
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
        try
        {
            if (BackpackIsOpen)
            {
                if (instance != null)
                {
                    instance.CloseContainer();
                    BackpackIsOpen = false;
                    
                    if (ConfigRegistry.CloseInventory.Value && !ConfigRegistry.OpenWithHoverInteract.Value)
                        instance.Hide();
                }
                else
                {
                    BackpackIsOpen = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            BackpackIsOpen = false;
            AdventureBackpacks.Log?.Warning($"Error hiding backpack: {ex.Message}");
        }
    }

    public static bool DetectInputToHide(Player player, InventoryGui instance)
    {
        if (player == null || instance == null || PlayerExtensions.IsDedicatedOrHeadless() || ZInput.instance == null)
            return false;

        var hotKeyDown = ZInput.GetKeyDown(ConfigRegistry.HotKeyOpen.Value.MainKey);
        var hotKeyDownOnClose = ConfigRegistry.CloseInventory.Value && hotKeyDown && !ConfigRegistry.OpenWithHoverInteract.Value;
        var hotKeyDrop = ConfigRegistry.OutwardMode.Value && ZInput.GetKeyDown(ConfigRegistry.HotKeyDrop.Value.MainKey);

        var openBackpack = hotKeyDown && !BackpackIsOpen && player.CanOpenBackpack() && !ConfigRegistry.OpenWithHoverInteract.Value;
        
        var grids = new List<InventoryGrid>();
        if (instance.m_player != null)
            grids.AddRange(instance.m_player.GetComponentsInChildren<InventoryGrid>());

        if (hotKeyDown && !BackpackIsOpen && ConfigRegistry.OpenWithHoverInteract.Value && !CheckForTextInput())
        {
            ItemDrop.ItemData hoveredItem = null;
            
            foreach (var grid in grids)
            {
                if (grid == null || grid.GetHoveredElement() == null)
                    continue;
                
                var hoveredElement = grid.GetHoveredElement();
                var gridInv = grid.GetInventory();
                if (gridInv != null)
                    hoveredItem = gridInv.GetItemAt(hoveredElement.Position.x, hoveredElement.Position.y);
            }

            if (ZInput.IsGamepadActive() && hoveredItem == null)
            {
                foreach (var grid in grids)
                {
                    if (grid == null || grid.GetGamepadSelectedItem() == null)
                        continue;
                    hoveredItem = grid.GetGamepadSelectedItem();
                }
            }
            
            if (hoveredItem != null && hoveredItem.IsBackpack() && hoveredItem.m_equipped && !BackpackIsOpen &&
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
            bool closeBackpack = false;
            
            if (ConfigRegistry.OpenWithHoverInteract.Value)
            {
                ItemDrop.ItemData hoveredItem = null;
            
                foreach (var grid in grids)
                {
                    if (grid == null || grid.GetHoveredElement() == null)
                        continue;
                
                    var hoveredElement = grid.GetHoveredElement();
                    var gridInv = grid.GetInventory();
                    if (gridInv != null)
                        hoveredItem = gridInv.GetItemAt(hoveredElement.Position.x, hoveredElement.Position.y);
                }

                if (ZInput.IsGamepadActive() && hoveredItem == null)
                {
                    foreach (var grid in grids)
                    {
                        if (grid == null || grid.GetGamepadSelectedItem() == null)
                            continue;
                        hoveredItem = grid.GetGamepadSelectedItem();
                    }
                }
                
                if (hoveredItem != null && hoveredItem.IsBackpack() && hoveredItem.m_equipped && BackpackIsOpen)
                {
                    closeBackpack = true;
                }
            }
            else
            {
                closeBackpack = true;
            }

            if (closeBackpack)
            {
                instance.CloseContainer();
                BackpackIsOpen = false;
                return false;
            }
        }
       
        if (hotKeyDrop && !CheckForTextInput())
        {
            player.QuickDropBackpack();
        }

        return ((hotKeyDownOnClose) || hotKeyDrop) && !CheckForTextInput();
    }
    
    public static bool DetectInputToShow(Player player, InventoryGui instance)
    {
        if (player == null || instance == null || PlayerExtensions.IsDedicatedOrHeadless() || ZInput.instance == null)
            return false;

        var hotKeyDown = ZInput.GetKeyDown(ConfigRegistry.HotKeyOpen.Value.MainKey);
        var hotKeyDrop = ConfigRegistry.OutwardMode.Value && ZInput.GetKeyDown(ConfigRegistry.HotKeyDrop.Value.MainKey);

        if (hotKeyDrop && !CheckForTextInput())
        {
            player.QuickDropBackpack();
        }

        if (hotKeyDown && !ConfigRegistry.OpenWithHoverInteract.Value && !BackpackIsOpen && player.CanOpenBackpack() && !CheckForTextInput())
        {
            _showBackpack = true;
        }
        
        return _showBackpack && !CheckForTextInput();
    }
   
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
    static class InventoryGuiUpdateTranspiler
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
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

            CodeInstruction CreateLdlocFromStloc(CodeInstruction stloc)
            {
                if (stloc.opcode == OpCodes.Stloc_0) return new CodeInstruction(OpCodes.Ldloc_0);
                if (stloc.opcode == OpCodes.Stloc_1) return new CodeInstruction(OpCodes.Ldloc_1);
                if (stloc.opcode == OpCodes.Stloc_2) return new CodeInstruction(OpCodes.Ldloc_2);
                if (stloc.opcode == OpCodes.Stloc_3) return new CodeInstruction(OpCodes.Ldloc_3);
                if (stloc.opcode == OpCodes.Stloc_S) return new CodeInstruction(OpCodes.Ldloc_S, stloc.operand);
                return new CodeInstruction(OpCodes.Ldloc, stloc.operand);
            }

            CodeInstruction CreateStlocFromStloc(CodeInstruction stloc)
            {
                return new CodeInstruction(stloc.opcode, stloc.operand);
            }

            var resetButtonStatus = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.ResetButtonStatus));
            var menuVisibleMethod = AccessTools.DeclaredMethod(typeof(Menu), nameof(Menu.IsVisible));
            var hideMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Hide));
            var showMethod = AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.Show));
            var tutorialMethod = AccessTools.DeclaredMethod(typeof(Player), nameof(Player.ShowTutorial));
            var zInputKeyDown = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.GetKeyDown), new []{typeof(KeyCode), typeof(bool)});
            var zInputButtonDown = AccessTools.DeclaredMethod(typeof(ZInput), nameof(ZInput.GetButtonDown), new []{typeof(string)});
            var hiddenFramesField = AccessTools.DeclaredField(typeof(InventoryGui), nameof(InventoryGui.m_hiddenFrames));

            var patchedHideBackpackMethod = false;
            var patchedShowBackpackMethod = false;
            var patchedDetectInputHideMethod = false;
            var patchedDetectInputShowMethod = false;

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 6 && instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(resetButtonStatus) &&
                    instrs[i + 1].opcode == OpCodes.Ldarg_0 && instrs[i + 2].opcode == OpCodes.Call &&
                    instrs[i + 2].operand.Equals(hideMethod))
                {
                    //Call to Hide Backpack
                    var ldArgInstruction = new CodeInstruction(OpCodes.Ldarg_0);
                    //Move Any Labels from the instruction position being patched to new instruction.\n                    if (instrs[i].labels.Count > 0)
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

                    patchedHideBackpackMethod = true;
                    
                } else if (i > 6 && (instrs[i].opcode == OpCodes.Call && instrs[i].operand.Equals(showMethod) &&
                           instrs[i - 1].opcode == OpCodes.Ldc_I4_1 && instrs[i - 2].opcode == OpCodes.Ldnull &&
                           instrs[i - 3].opcode == OpCodes.Ldarg_0 || instrs[i - 4].opcode == OpCodes.Callvirt && instrs[i - 4].operand.Equals(tutorialMethod)))
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

                    patchedShowBackpackMethod = true;
                } else if (i > 6 && instrs[i].opcode == OpCodes.Call 
                                 && instrs[i].operand.Equals(zInputKeyDown)
                                 && instrs[i - 1].opcode == OpCodes.Ldc_I4_1
                                 && instrs[i - 2].opcode == OpCodes.Ldc_I4_S
                                 && instrs[i - 2].operand.Equals((sbyte)KeyCode.Escape) 
                                 && instrs[i + 2].opcode == OpCodes.Ldstr 
                                 && instrs[i + 2].operand.Equals("Use"))
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

                    patchedDetectInputHideMethod = true;

                } else if (i > 6 && (instrs[i].opcode == OpCodes.Stloc_3 || instrs[i].opcode == OpCodes.Stloc_S || instrs[i].opcode == OpCodes.Stloc || instrs[i].opcode == OpCodes.Stloc_0 || instrs[i].opcode == OpCodes.Stloc_1 || instrs[i].opcode == OpCodes.Stloc_2)
                           && instrs[i + 1].opcode == OpCodes.Ldarg_0
                           && instrs[i + 2].opcode == OpCodes.Ldfld && instrs[i + 2].operand.Equals(hiddenFramesField)
                           && instrs.GetRange(Math.Max(0, i - 10), Math.Min(10, i)).Any(inst => inst.opcode == OpCodes.Ldstr && "JoyButtonY".Equals(inst.operand)))
                {
                    // 1. Output current stloc instruction (stores the vanilla / ModLib flag result)
                    yield return LogMessage(instrs[i]);
                    counter++;

                    // 2. Define skip label
                    var skipLabel = ilGenerator.DefineLabel();

                    // 3. Load flag (same local variable as instrs[i])
                    yield return LogMessage(CreateLdlocFromStloc(instrs[i]));
                    counter++;

                    // 4. Branch to skip if flag is already true
                    yield return LogMessage(new CodeInstruction(OpCodes.Brtrue, skipLabel));
                    counter++;

                    // 5. Load Player (ldloc.1)
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_1));
                    counter++;

                    // 6. Load InventoryGui (ldarg.0)
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_0));
                    counter++;

                    // 7. Call DetectInputToShow
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGuiPatches), nameof(DetectInputToShow))));
                    counter++;

                    // 8. Store result back into the flag local variable
                    yield return LogMessage(CreateStlocFromStloc(instrs[i]));
                    counter++;

                    // 9. Attach the skip label to the next instruction (ldarg.0)
                    instrs[i + 1].labels.Add(skipLabel);

                    patchedDetectInputShowMethod = true;
                }
                else
                {
                    yield return LogMessage(instrs[i]);
                    counter++;
                }
            }

            if (!patchedHideBackpackMethod || !patchedShowBackpackMethod || !patchedDetectInputHideMethod ||
                !patchedDetectInputShowMethod)
            {
                AdventureBackpacks.Log.Error($"InventoryGui.Update Transpiler Failed To Patch");
                AdventureBackpacks.Log.Warning($" patchedHideBackpackMethod {patchedHideBackpackMethod}");
                AdventureBackpacks.Log.Warning($" patchedShowBackpackMethod {patchedShowBackpackMethod}");
                AdventureBackpacks.Log.Warning($" patchedDetectInputHideMethod {patchedDetectInputHideMethod}");
                AdventureBackpacks.Log.Warning($" patchedDetectInputShowMethod {patchedDetectInputShowMethod}");
                AdventureBackpacks.Log.Error($"Please inform Mod Author.");
                Thread.Sleep(5000);
            }
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement))]
    static class InventoryGuiSetupRequirementPatch
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

            var countItemsMethod = AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.CountItems), new[] { typeof(string), typeof(int), typeof(bool) }); 

            for (int i = 0; i < instrs.Count; ++i)
            {
                yield return LogMessage(instrs[i]);
                counter++;

                if (instrs[i].opcode == OpCodes.Callvirt && 
                    (instrs[i].operand.Equals(countItemsMethod) || (instrs[i].operand is MethodInfo m && m.Name == nameof(Inventory.CountItems))))
                {
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_2));
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Ldarg_1));
                    counter++;

                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlayerPatches), nameof(PlayerPatches.AdjustCountIfEquipped), new[] { typeof(int), typeof(Player), typeof(Piece.Requirement) })));
                    counter++;

                    patchedSuccess = true;
                }
            }
            if (!patchedSuccess)
            {
                AdventureBackpacks.Log.Error($"InventoryGui.SetupRequirement Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }

        [UsedImplicitly]
        static void Postfix(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality, int craftMultiplier, ref bool __result)
        {
            if (!__result || elementRoot == null || req == null || req.m_resItem == null || req.m_resItem.m_itemData == null || player == null)
                return;

            if (!CraftFromBackpack.CanCraftFromBackpack(player, out _))
                return;

            var itemName = req.m_resItem.m_itemData.m_shared?.m_name;
            if (string.IsNullOrEmpty(itemName))
                return;

            var bpCount = CraftFromBackpack.GetBackpackItemCount(player, itemName);
            if (bpCount <= 0)
                return;

            var resAmountObj = elementRoot.Find("res_amount");
            if (resAmountObj == null)
                return;

            var textComponent = resAmountObj.GetComponent<TMP_Text>();
            if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
                return;

            if (textComponent.text.Contains("/"))
            {
                var parts = textComponent.text.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[0], out var currentCount) && int.TryParse(parts[1], out var reqCount))
                {
                    var newCount = currentCount + bpCount;
                    textComponent.text = $"{newCount}/{reqCount}";
                    if (newCount >= reqCount)
                    {
                        textComponent.color = Color.white;
                    }
                }
            }
        }
    }
}
