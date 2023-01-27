using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Components;
using AdventureBackpacks.Extensions;
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

        private static HitData.DamageModPair _frostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
        public static List<string> BackpackTypes => _backpackTypes;

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
            var backpack = BackpackFactory.BackpackItems.FirstOrDefault(x => x.ItemName.Equals(name));
            
            if (backpack == null)
                return null;
            
            switch (backpack.Biome)
            {
                case BackpackBiomes.Meadows:
                case BackpackBiomes.BlackForest:
                case BackpackBiomes.Swamp:
                case BackpackBiomes.Mountains:
                case BackpackBiomes.Plains:
                case BackpackBiomes.Mistlands:
                    switch (itemMQuality)
                    {
                        case 1:
                            newInventory = new Inventory(uiInventoryName, null, 2, 1);
                            break;
                        case 2:
                            newInventory = new Inventory(uiInventoryName, null, 3, 1);
                            break;
                        case 3:
                            newInventory = new Inventory(uiInventoryName, null, 4, 1);
                            break;
                        case 4:
                            newInventory = new Inventory(uiInventoryName, null, 5, 1);
                            break;
                    }
                    break;
                case BackpackBiomes.None:
                    newInventory = new Inventory(
                        uiInventoryName,
                        null,
                        (int)backpack.BackpackSize.Value.x,
                        (int)backpack.BackpackSize.Value.y
                    );
                    break;
            }

            if (newInventory != null)
                return newInventory;
            
            _log.Warning($"Calling method with unknown item name");
            return null;
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
                    AdventureBackpacks.Log.Message("You can't put a backpack inside a backpack, silly!");
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
            
            statusEffects.Effect.m_name = $"{backpackName} Level {backpackQuality} Effect";
            statusEffects.Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            statusEffects.Effect.m_startMessage = $"Your backpack feels useful.";

            var modifierList = new List<HitData.DamageModPair>();
            //Set Armor Default
            itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel * backpackQuality;
            var backpack = BackpackFactory.BackpackItems.FirstOrDefault(x => x.ItemName.Equals(backpackName));
            
            if (backpack == null)
                return null;
            
            switch (backpack.Biome)
            {
                case BackpackBiomes.Meadows:
                case BackpackBiomes.BlackForest:
                case BackpackBiomes.Swamp:
                case BackpackBiomes.Mountains:
                case BackpackBiomes.Plains:
                case BackpackBiomes.Mistlands:
                    switch (backpackQuality)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                        case 4:
                             modifierList.Add(_frostResistance);
                            break;
                    }
                    ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = 15 * backpackQuality;
                    
                    break;
                case BackpackBiomes.None:
                    itemData.m_shared.m_movementModifier = backpack.SpeedMod.Value/backpackQuality;
                    switch (backpackQuality)
                    {
                        case 1:
                            
                            break;
                        case 2:
                            modifierList.Add(_frostResistance);
                            break;
                        case 3:
                            modifierList.Add(_frostResistance);
                            break;
                        case 4:
                            modifierList.Add(_frostResistance);
                            itemData.m_shared.m_movementModifier = 0;
                            break;
                    }
                    ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = 25 * backpackQuality;
                    
                    break;
            }
            itemData.m_shared.m_maxDurability = 1000f;
            ((SE_Stats)statusEffects.Effect).m_mods = modifierList;

            itemData.AddSEToItem(statusEffects);
            return statusEffects;
        }
    }
}