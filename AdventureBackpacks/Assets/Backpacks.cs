using System;
using System.Collections.Generic;
using System.Reflection;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
using AdventureBackpacks.Extensions;
using ItemManager;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.PieceManager;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;
using CraftingTable = ItemManager.CraftingTable;

namespace AdventureBackpacks.Assets
{
    public static class Backpacks
    {
        // Asset and prefab loading
        private const string BackpackAssetName = "vapokbackpacks";
        private const string RuggedBackpackPrefab = "CapeIronBackpack";
        private const string ArcticBackpackPrefab = "CapeSilverBackpack";
        private const string MeadowsBackpackPrefab = "BackpackMeadows";
        private const string RuggedBackpackName = "$vapok_mod_item_rugged_backpack";
        private const string ArcticBackpackName = "$vapok_mod_item_arctic_backpack";
        private const string MeadowsBackpackName = "$vapok_mod_item_backpack_meadows";
        
        private static Item _ruggedBackpack;
        private static Item _arcticBackpack;
        private static Item _meadowsBackpack;

        private static ILogIt _log;
        private static List<string> _backpackTypes = new();

        private static HitData.DamageModPair _frostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
        public static List<string> BackpackTypes => _backpackTypes;

        public static void LoadAssets()
        {
            _log = AdventureBackpacks.Log;
            _log.Info($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            
            
            //Adding Backpack Names
            BackpackTypes.Add(RuggedBackpackName);
            BackpackTypes.Add(ArcticBackpackName);
            BackpackTypes.Add(MeadowsBackpackName);
            
            //Register Meadows Backpack
            _meadowsBackpack = new Item(BackpackAssetName, MeadowsBackpackPrefab, "Assets.Bundles");
            _meadowsBackpack.Crafting.Add(CraftingTable.Workbench,2);
            _meadowsBackpack.RequiredItems.Add("CapeDeerHide",1);
            _meadowsBackpack.RequiredItems.Add("DeerHide",8);
            _meadowsBackpack.RequiredItems.Add("BoneFragments",2);
            _meadowsBackpack.RequiredUpgradeItems.Add("LeatherScraps", 5);
            _meadowsBackpack.RequiredUpgradeItems.Add("DeerHide",3);
            _meadowsBackpack.Configurable = Configurability.Disabled;

            //Adjust Rugged ItemData
            var meadowItemItemDrop = _meadowsBackpack.Prefab.GetComponent<ItemDrop>();
            var meadowItemData = meadowItemItemDrop.m_itemData;
            if (meadowItemData != null)
            {
                meadowItemData.m_shared.m_armor = meadowItemData.m_shared.m_armorPerLevel;
                meadowItemItemDrop.Save();
            }
            
            
            MaterialReplacer.RegisterGameObjectForShaderSwap(_meadowsBackpack.Prefab,MaterialReplacer.ShaderType.PieceShader);
            
            //Register Rugged Backpack
            _ruggedBackpack = new Item(BackpackAssetName, RuggedBackpackPrefab, "Assets.Bundles");
            _ruggedBackpack.Crafting.Add(CraftingTable.Workbench,1);
            _ruggedBackpack.RequiredItems.Add("LeatherScraps",8);
            _ruggedBackpack.RequiredItems.Add("DeerHide",2);
            _ruggedBackpack.RequiredItems.Add("Bronze",2);
            _ruggedBackpack.RequiredUpgradeItems.Add("Bronze", 4);
            _ruggedBackpack.RequiredUpgradeItems.Add("DeerHide", 4);

            _ruggedBackpack.Configurable = Configurability.Disabled;
            
            MaterialReplacer.RegisterGameObjectForShaderSwap(_ruggedBackpack.Prefab,MaterialReplacer.ShaderType.PieceShader);

            //Adjust Rugged ItemData
            var ruggedItemItemDrop = _ruggedBackpack.Prefab.GetComponent<ItemDrop>();
            var ruggedItemData = ruggedItemItemDrop.m_itemData;
            if (ruggedItemData != null)
            {
                ruggedItemData.m_shared.m_armor = ruggedItemData.m_shared.m_armorPerLevel;
                ruggedItemItemDrop.Save();
            }
            
            //Set Persistence
            _ruggedBackpack.Prefab.GetComponent<ZNetView>().m_persistent = true;
            
            //Register Arctic Backpack
            _arcticBackpack = new Item(BackpackAssetName, ArcticBackpackPrefab, "Assets.Bundles");
            _arcticBackpack.Crafting.Add(CraftingTable.Workbench,3);
            _arcticBackpack.RequiredItems.Add("LeatherScraps",8);
            _arcticBackpack.RequiredItems.Add("WolfPelt",2);
            _arcticBackpack.RequiredItems.Add("Silver",2);
            _arcticBackpack.RequiredUpgradeItems.Add("Silver", 4);
            _arcticBackpack.RequiredUpgradeItems.Add("WolfPelt", 4);

            _arcticBackpack.Configurable = Configurability.Disabled;
            
            MaterialReplacer.RegisterGameObjectForShaderSwap(_arcticBackpack.Prefab,MaterialReplacer.ShaderType.PieceShader);
            
            //Adjust Arctic ItemData
            var arcticItemItemDrop = _arcticBackpack.Prefab.GetComponent<ItemDrop>();
            var arcticItemData = arcticItemItemDrop.m_itemData;
            if (arcticItemData != null)
            {
                arcticItemData.m_shared.m_armor = arcticItemData.m_shared.m_armorPerLevel;
                arcticItemItemDrop.Save();
            }
            
            //Set Persistence
            _arcticBackpack.Prefab.GetComponent<ZNetView>().m_persistent = true;
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
            switch (name)
            {
                case MeadowsBackpackName:
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
                case RuggedBackpackName:
                    newInventory = new Inventory(
                        uiInventoryName,
                        null,
                        (int)ConfigRegistry.RuggedBackpackSize.Value.x,
                        (int)ConfigRegistry.RuggedBackpackSize.Value.y
                    );
                    break;
                case ArcticBackpackName:
                    newInventory = new Inventory(
                        uiInventoryName,
                        null,
                        (int)ConfigRegistry.ArcticBackpackSize.Value.x,
                        (int)ConfigRegistry.ArcticBackpackSize.Value.y
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
            switch (backpackName)
            {
                case MeadowsBackpackName:
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
                case RuggedBackpackName:
                    itemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModRugged.Value/backpackQuality;
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
                case ArcticBackpackName:
                    itemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModArctic.Value/backpackQuality;
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
                    ((SE_Stats)statusEffects.Effect).m_addMaxCarryWeight = 35 * backpackQuality;
                    break;
            }
            itemData.m_shared.m_maxDurability = 1000f;
            ((SE_Stats)statusEffects.Effect).m_mods = modifierList;

            itemData.AddSEToItem(statusEffects);
            return statusEffects;
        }
    }
}