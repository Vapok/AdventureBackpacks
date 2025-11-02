using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using BepInEx;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;
using Random = System.Random;

namespace AdventureBackpacks.Assets
{
    public static class Backpacks
    {

        private static ILogIt _log = AdventureBackpacks.Log;
        private static List<string> _backpackTypes = new();
        
        
        public static List<string> BackpackTypes => _backpackTypes;

        public static void LoadBackpackTypes(List<string> backpackTypes)
        {
            _backpackTypes = backpackTypes;
        }

        internal static bool TryGetBackpackItem(this ItemDrop.ItemData itemData, out BackpackItem backpack)
        {
            backpack = null;
            
            if (itemData == null)
                return false;

            return TryGetBackpackItemByName(itemData.m_shared.m_name, out backpack);
        }

        internal static bool TryGetBackpackItemByName(string name, out BackpackItem backpack)
        {
            backpack = null;
            
            if (name.IsNullOrWhiteSpace())
                return false;
            
            backpack = BackpackFactory.BackpackItems.FirstOrDefault(x => x.ItemName.Equals(name));
            return backpack != null;
        }

        public static void UpdateItemDataConfigValues(object sender, EventArgs e)
        {
            if (Player.m_localPlayer == null || Player.m_localPlayer.GetInventory() == null)
                return;

            var player = Player.m_localPlayer;
            //Close Inventory before making changes.  Leaving open can have undesirable effects.
            InventoryGui.instance.Hide();
            
            void SearchInventory(List<ItemDrop.ItemData> inventory)
            {
                if (inventory == null)
                    return;
                
                // Go through all the equipped items, match them for any of the names in backpackTypes.
                // If a match is found, return the backpack ItemData object.
                foreach (var item in inventory)
                {
                    if (item == null)
                        continue;
                
                    if (item.IsBackpack() && item.TryGetBackpackItem(out var backpack))
                    {
                        var currentBackpack = item.Data().GetOrCreate<BackpackComponent>();
                        var size = backpack.GetInventorySize(currentBackpack.Item.m_quality);
                        if (currentBackpack.InventoryNeedsValidating(size))
                        {
                            ValidateBackpackInventorySizing(player, item);
                        }
                        
                    }
                }
            }
            
            // Get a list of all  items.
            var inventory = Player.m_localPlayer.GetInventory();

            if (inventory != null)
            {
                SearchInventory(inventory.m_inventory);    
            }

            // Get a list of all equipped items.
            var equipped = Player.m_localPlayer.GetInventory().GetEquippedItems();

            if (equipped != null)
            {
                SearchInventory(equipped);    
            }
            
            Player.m_localPlayer.UpdateEquipmentStatusEffects();
        }
        
        public static Inventory NewInventoryInstance(string name, int itemMQuality = 1)
        {
            Inventory newInventory = null;
            var uiInventoryName = $"{name} $vapok_mod_level {itemMQuality}";
            
            if (!TryGetBackpackItemByName(name, out var backpack))
                return null;
            var backpackSize = backpack.GetInventorySize(itemMQuality);
            
            newInventory = new Inventory(uiInventoryName, null, backpackSize.x, backpackSize.y);
            
            return newInventory;
        }

        public static Vector2 ValidateMinMaxChestSize(float x, float y)
        {
            if (x < 1)
                x = 1;
            if (y < 1)
                y = 1;

            return new Vector2(x, y);
        }

        public static void ValidateBackpackInventorySizing(Player player, ItemDrop.ItemData currentBackpack)
        {
            AdventureBackpacks.Log.Debug($"############################################");
            AdventureBackpacks.Log.Debug($"####  ValidateBackpackInventorySizing  #####");
            AdventureBackpacks.Log.Debug($"{currentBackpack.m_shared.m_name}");
            AdventureBackpacks.Log.Debug($"############################################");
            var stackTrace = new StackTrace();

            for (int i = 1; i <= 20; i++)
            {
                if (stackTrace.FrameCount < i)
                    break;
                
                var callingMethod = stackTrace.GetFrame(i)?.GetMethod();
                AdventureBackpacks.Log.Debug($"[{i}]Called by: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name}");
            }
            
            if (!currentBackpack.TryGetBackpackItem(out var backpackDefinition)) return;

            var vectorConfig = backpackDefinition.BackpackSize[currentBackpack.m_quality];
            var vectorSize = ValidateMinMaxChestSize(vectorConfig.Value.x, vectorConfig.Value.y);
            
            var backpackSize = (int)Math.Floor(vectorSize.x) * (int)Math.Floor(vectorSize.y);
            AdventureBackpacks.Log.Debug($"[{currentBackpack.m_shared.m_name}]### Backpack Slot Size: {backpackSize}");
                        
            var backpackItem = currentBackpack.Data().GetOrCreate<BackpackComponent>();
            var currentInventory = backpackItem.GetInventory();
            AdventureBackpacks.Log.Debug($"[{currentBackpack.m_shared.m_name}]### Current Inventory Slot Size: {currentInventory.m_inventory.Count}");

            if (backpackSize < currentInventory.m_inventory.Count)
            {
                var diff = currentInventory.m_inventory.Count - backpackSize;
                AdventureBackpacks.Log.Debug($"[{currentBackpack.m_shared.m_name}]### I need to YEET {diff} items");
                PerformYardSale(player, backpackItem.Item, true, diff);

                var newInventorySize = currentInventory.m_inventory.Count;
                AdventureBackpacks.Log.Debug($"[{currentBackpack.m_shared.m_name}]### New Inventory Size {newInventorySize}");
                backpackItem.IsLoadingInventory = true;
                var newInventory = NewInventoryInstance(backpackDefinition.ItemName, currentBackpack.m_quality);
                
                newInventory.MoveAll(currentInventory);
                
                backpackItem.IsLoadingInventory = false;
                
                backpackItem.Save(newInventory);
            }
            
            backpackItem.Load();
            if (player.IsThisBackpackEquipped(currentBackpack))
            {
                var backpackContainer = player.gameObject.GetComponent<Container>();
                backpackItem.UpdateContainerSizing(backpackContainer);
            }
            AdventureBackpacks.Log.Debug($"############################################");
            AdventureBackpacks.Log.Debug($"DONE  ValidateBackpackInventorySizing  DONE");
            AdventureBackpacks.Log.Debug($"{currentBackpack.m_shared.m_name}");
            AdventureBackpacks.Log.Debug($"############################################");

        }
        
        public static bool CheckForInception(Inventory instance, ItemDrop.ItemData item)
        {
            if (instance.IsBackPackInventory() && Player.m_localPlayer != null)
            {
                // If the item is a backpack...
                if (item.TryGetBackpackItem(out var backpack))
                {
                    backpack.InceptionCounter++;

                    switch (backpack.InceptionCounter)
                    {
                        case 1:
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception1");
                            break;
                        case 2:
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception2");
                            break;
                        case 5:
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception3");
                            break;
                        case 10:
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception4");

                            var interval = new Random(42);
                            var timeInterval = interval.Next(5000,20000);
                            backpack.YardSaleTimer = new System.Timers.Timer(timeInterval);
                            backpack.YardSaleTimer.AutoReset = false;
                            backpack.YardSaleTimer.Enabled = false;
                            backpack.YardSaleTimer.Elapsed += (sender,e) =>  YardSaleEvent(backpack);
                            backpack.YardSaleTimer.Start();
                            break;
                    }
                    
                    // Nope!
                    return false;
                }
            }
            return true;
        }

        public static void PerformYardSale(Player mLocalPlayer, ItemDrop.ItemData itemData, bool backpackOnly = false, int numberItems = 0)
        {
            if (itemData.IsBackpack())
            {
                var backpack = itemData.Data().Get<BackpackComponent>();
                if (backpack != null)
                {

                    void EmtpyInventory(Inventory inventory)
                    {
                        if (backpack.IsEmptyingBackpack)
                            return;
                        
                        AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}]Number of Items: {numberItems}");
                        AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}]Inventory Count: {inventory.m_inventory.Count}");
                        
                        backpack.IsEmptyingBackpack = true;
                        try
                        {
                            if (numberItems == 0)
                            {
                                while (inventory.m_inventory.Count > 0)
                                {
                                    if (inventory.m_inventory.Count == 0 || inventory.m_inventory[0] == null)
                                        break;
                                    
                                    var item = inventory.m_inventory[0];
                        
                                    var amount = inventory.CountItems(item.m_shared.m_name, -1);

                                    var dropAmount = amount > item.m_stack ? item.m_stack : amount;
                        
                                    mLocalPlayer.DropItem(inventory,item,dropAmount);
                                }
                            }
                            else
                            {
                                var dropped = 0;
                                while (dropped < numberItems && inventory.m_inventory.Count > 0)
                                {
                                    if (inventory.m_inventory.Count == 0 || inventory.m_inventory[0] == null)
                                        break;
                                    
                                    var item = inventory.m_inventory[0]; // safe because of Count check

                                    var amount = inventory.CountItems(item.m_shared.m_name, -1);
                                    AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}] Number of {item.m_shared.m_name}: {amount}");
                                    AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}] Stack Size of {item.m_shared.m_name}: {item.m_stack}");

                                    var dropAmount = amount > item.m_stack ? item.m_stack : amount;
                                    AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}] Drop Amount: {dropAmount}");

                                    mLocalPlayer.DropItem(inventory, item, dropAmount);

                                    dropped++;
                                }
                            }
                            AdventureBackpacks.Log.Debug($"[{itemData.m_shared.m_name}] Inventory Count (end): {inventory.m_inventory.Count}");
                        }
                        finally
                        {
                            backpack.IsEmptyingBackpack = false;
                        }
                    }
                    
                    var inventory = backpack.GetInventory();
                    EmtpyInventory(inventory);

                    if (!backpackOnly)
                    {
                        var playerInventory = mLocalPlayer.GetInventory();
                        EmtpyInventory(playerInventory);
                    
                        mLocalPlayer.UnequipAllItems();
                    
                        EmtpyInventory(playerInventory);
                        
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception5");
                        AdventureBackpacks.PerformYardSale = false;
                    }
                    else
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_thor_saves_contents");
                    }
                }
            }
        }
        
        private static void YardSaleEvent(BackpackItem backpack)
        {
            backpack.YardSaleTimer.Stop();
            AdventureBackpacks.PerformYardSale = true;
        }

        public static CustomSE UpdateStatusEffects(ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return null;

            if (!itemData.TryGetBackpackItem(out var backpack))
                return null;
            
            var backpackName = itemData.m_shared.m_name;
            
            var backpackQuality = itemData.m_quality;
            
            var statusEffects = new CustomSE(Enums.StatusEffects.Stats, $"SE_{backpackName}_{backpackQuality}");
            
            var defaultStatusName = $"{backpackName} $vapok_mod_level {backpackQuality} $vapok_mod_effect";
            

            if (backpack.ShowBackpackStatusEffect.Value)
            {
                statusEffects.Effect.m_name = string.IsNullOrEmpty(backpack.CustomStatusEffectName.Value) ? defaultStatusName : backpack.CustomStatusEffectName.Value;
                statusEffects.Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
                statusEffects.Effect.m_startMessage = $"$vapok_mod_useful_backpack";
            }
            
            var modifierList = new List<HitData.DamageModPair>();
            //Set Armor Default
            itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel * backpackQuality;
            
            //Apply Frost Resistance if configured.
            var frostResistEffect = EffectsFactory.EffectList[BackpackEffect.FrostResistance];
            if (frostResistEffect.IsEffectActive(itemData))
                modifierList.Add(FrostResistance.EffectMod);
            
            var trollEffect = EffectsFactory.EffectList[BackpackEffect.TrollArmor];
            //Apply Troll Armor Set if configured.
            if (trollEffect.HasActiveStatusEffect(itemData, out var trollSneakEffect))
            {
                itemData.m_shared.m_setName = "troll";
                itemData.m_shared.m_setSize = 4;
                itemData.m_shared.m_setStatusEffect = trollSneakEffect;
            }
            else
            {
                itemData.m_shared.m_setName = string.Empty;
                itemData.m_shared.m_setSize = 0;
                itemData.m_shared.m_setStatusEffect = null;
            }
            
            backpack.UpdateStatusEffects(backpackQuality, statusEffects, modifierList, itemData);
            
            itemData.m_shared.m_maxDurability = 1000f;
            ((SE_Stats)statusEffects.Effect).m_mods = modifierList;
            if (backpack.ShowBackpackStatusEffect.Value)
            {
                ((SE_Stats)statusEffects.Effect).m_icon = itemData.GetIcon();   
            }

            itemData.AddSEToItem(statusEffects);
            
            return statusEffects;
        }
    }
}