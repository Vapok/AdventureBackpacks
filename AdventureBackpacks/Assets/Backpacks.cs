using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Assets.Effects;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
using BepInEx;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

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
                
                    if (item.IsBackpack())
                    {
                        //UpdateStatusEffects(item);
                        var backpackItem = item.Data().GetOrCreate<BackpackComponent>();
                        backpackItem.Load();
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
            var equipped = Player.m_localPlayer.GetInventory().GetEquipedtems();

            if (equipped != null)
            {
                SearchInventory(equipped);    
            }
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

        public static bool CheckForInception(Inventory instance, ItemDrop.ItemData item)
        {
            if (instance.IsBackPackInventory() && Player.m_localPlayer != null)
            {
                // If the item is a backpack...
                if (item.IsBackpack())
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception");    

                    // Nope!
                    AdventureBackpacks.Log.Message("Odin says, 'You can't put a backpack inside a backpack!'");
                    return false;
                }
            }
            return true;
        }

        public static CustomSE UpdateStatusEffects(ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return null;
            
            var backpackName = itemData.m_shared.m_name;
            var backpackQuality = itemData.m_quality;
            var statusEffects = new CustomSE(Enums.StatusEffects.Stats, $"SE_{backpackName}_{backpackQuality}");
            
            statusEffects.Effect.m_name = $"{backpackName} $vapok_mod_level {backpackQuality} $vapok_mod_effect";
            statusEffects.Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            statusEffects.Effect.m_startMessage = $"$vapok_mod_useful_backpack";

            var modifierList = new List<HitData.DamageModPair>();
            //Set Armor Default
            itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel * backpackQuality;
            
            if (!itemData.TryGetBackpackItem(out var backpack))
                return null;
            
            //Apply Frost Resistance If Configured.
            if (FrostResistance.ShouldHaveFrostResistance(itemData))
                modifierList.Add(BackpackEffects.FrostResistance);
            
            backpack.UpdateStatusEffects(backpackQuality, statusEffects, modifierList, itemData);
            
            itemData.m_shared.m_maxDurability = 1000f;
            ((SE_Stats)statusEffects.Effect).m_mods = modifierList;

            itemData.AddSEToItem(statusEffects);
            return statusEffects;
        }
    }
}