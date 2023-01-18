using System;
using System.Collections.Generic;
using System.Reflection;
using AdventureBackpacks.Components;
using AdventureBackpacks.Configuration;
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
        private const string RuggedBackpackName = "$vapok_mod_item_rugged_backpack";
        private const string ArcticBackpackName = "$vapok_mod_item_arctic_backpack";
        private const string UiInventoryName = "$vapok_mod_ui_backpack_inventoryname";
        
        private static Item _ruggedBackpack;
        private static Item _arcticBackpack;
        private static CustomSE _ruggedBackpackEffect;
        private static CustomSE _arcticBackpackEffect;

        private static ILogIt _log;
        private static List<string> _backpackTypes = new List<string>();
        private static bool _opening;
        private static Container _backpackContainer;
        private static BackpackComponent _backpackEquipped; //Backpack currently equipped.
        private static HitData.DamageModPair _frostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
        public static List<string> BackpackTypes => _backpackTypes;

        public static bool Opening
        {
            get => _opening;
            set => _opening = value;
        }

        public static Container BackpackContainer => _backpackContainer;

        public static string BackpacksInventoryName => UiInventoryName;

        public static void LoadAssets()
        {
            _log = AdventureBackpacks.Log;
            _log.Info($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            
            BackpackTypes.Add(RuggedBackpackName);
            BackpackTypes.Add(ArcticBackpackName);
            
            //Register Rugged Backpack
            _ruggedBackpack = new Item(BackpackAssetName, RuggedBackpackPrefab, "Assets.Bundles");
            _ruggedBackpack.Crafting.Add(CraftingTable.Workbench,1);
            _ruggedBackpack.RequiredItems.Add("LeatherScraps",8);
            _ruggedBackpack.RequiredItems.Add("DeerHide",2);
            _ruggedBackpack.RequiredItems.Add("Bronze",2);

            _ruggedBackpack.Configurable = Configurability.Disabled;
            
            MaterialReplacer.RegisterGameObjectForShaderSwap(_ruggedBackpack.Prefab,MaterialReplacer.ShaderType.PieceShader);
            
            //Adding Rugged Status Effect
            _ruggedBackpackEffect = new CustomSE(Enums.StatusEffects.Stats, "SE_RuggedBackpack");
            _ruggedBackpackEffect.Effect.m_name = "$vapok_mod_se_ruggedbackpack";
            _ruggedBackpackEffect.Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            _ruggedBackpackEffect.Effect.m_startMessage = "$vapok_mod_se_ruggedbackpackeffects_start";
            ((SE_Stats)_ruggedBackpackEffect.Effect).m_addMaxCarryWeight = ConfigRegistry.CarryBonusRugged.Value;
            if (ConfigRegistry.FreezingRugged.Value)
                ((SE_Stats)_ruggedBackpackEffect.Effect).m_mods = new List<HitData.DamageModPair> { _frostResistance };

            _ruggedBackpackEffect.AddSEToPrefab(_ruggedBackpackEffect,RuggedBackpackPrefab);

            //Adjust Rugged ItemData
            var ruggedItemItemDrop = _ruggedBackpack.Prefab.GetComponent<ItemDrop>();
            var ruggedItemData = ruggedItemItemDrop.m_itemData;
            if (ruggedItemData != null)
            {
                ruggedItemData.m_shared.m_maxDurability = 1000f;
                ruggedItemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModRugged.Value;
            }
            
            ruggedItemItemDrop.Save();
            
            //Set Persistence
            _ruggedBackpack.Prefab.GetComponent<ZNetView>().m_persistent = true;
            
            //Register Arctic Backpack
            _arcticBackpack = new Item(BackpackAssetName, ArcticBackpackPrefab, "Assets.Bundles");
            _arcticBackpack.Crafting.Add(CraftingTable.Workbench,3);
            _arcticBackpack.RequiredItems.Add("LeatherScraps",8);
            _arcticBackpack.RequiredItems.Add("WolfPelt",2);
            _arcticBackpack.RequiredItems.Add("Silver",2);

            _arcticBackpack.Configurable = Configurability.Disabled;
            
            MaterialReplacer.RegisterGameObjectForShaderSwap(_arcticBackpack.Prefab,MaterialReplacer.ShaderType.PieceShader);
            
            //Adding Arctic Status Effect
            _arcticBackpackEffect = new CustomSE(Enums.StatusEffects.Stats, "SE_ArcticBackpack");
            _arcticBackpackEffect.Effect.m_name = "$vapok_mod_se_arcticbackpack";
            _arcticBackpackEffect.Effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            _arcticBackpackEffect.Effect.m_startMessage = "$vapok_mod_se_arcticbackpackeffects_start";
            ((SE_Stats)_arcticBackpackEffect.Effect).m_addMaxCarryWeight = ConfigRegistry.CarryBonusArctic.Value;
            
            if (ConfigRegistry.FreezingArctic.Value)
                ((SE_Stats)_arcticBackpackEffect.Effect).m_mods = new List<HitData.DamageModPair> { _frostResistance };

            _arcticBackpackEffect.AddSEToPrefab(_arcticBackpackEffect,ArcticBackpackPrefab);

            //Adjust Arctic ItemData
            var arcticItemItemDrop = _arcticBackpack.Prefab.GetComponent<ItemDrop>();
            var arcticItemData = arcticItemItemDrop.m_itemData;
            if (arcticItemData != null)
            {
                arcticItemData.m_shared.m_maxDurability = 1000f;
                arcticItemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModArctic.Value;
            }
            
            arcticItemItemDrop.Save();
            
            //Set Persistence
            _arcticBackpack.Prefab.GetComponent<ZNetView>().m_persistent = true;
        }

        public static void UpdateStatusEffectConfigValues(object sender, EventArgs e)
        {
            ((SE_Stats)_ruggedBackpackEffect.Effect).m_addMaxCarryWeight = ConfigRegistry.CarryBonusRugged.Value;

            if (ConfigRegistry.FreezingRugged.Value)
            {
                if (!((SE_Stats)_ruggedBackpackEffect.Effect).m_mods.Contains(_frostResistance))
                    ((SE_Stats)_ruggedBackpackEffect.Effect).m_mods = new List<HitData.DamageModPair> { _frostResistance };
            }
            else
            {
                if (((SE_Stats)_ruggedBackpackEffect.Effect).m_mods.Contains(_frostResistance))
                    ((SE_Stats)_ruggedBackpackEffect.Effect).m_mods.Remove(_frostResistance);
            }
            
            ((SE_Stats)_arcticBackpackEffect.Effect).m_addMaxCarryWeight = ConfigRegistry.CarryBonusArctic.Value;
            
            if (ConfigRegistry.FreezingArctic.Value)
            {
                if (!((SE_Stats)_arcticBackpackEffect.Effect).m_mods.Contains(_frostResistance))
                    ((SE_Stats)_arcticBackpackEffect.Effect).m_mods = new List<HitData.DamageModPair> { _frostResistance };
            }
            else
            {
                if (((SE_Stats)_arcticBackpackEffect.Effect).m_mods.Contains(_frostResistance))
                    ((SE_Stats)_arcticBackpackEffect.Effect).m_mods.Remove(_frostResistance);
            }
        }

        public static void UpdateItemDataConfigValues(object sender, EventArgs e)
        {
            //Adjust Rugged ItemData
            var ruggedItemItemDrop = _ruggedBackpack.Prefab.GetComponent<ItemDrop>();
            var ruggedItemData = ruggedItemItemDrop.m_itemData;
            if (ruggedItemData != null)
            {
                ruggedItemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModRugged.Value;
            }
            ruggedItemItemDrop.Save();
            
            //Adjust Arctic ItemData
            var arcticItemItemDrop = _arcticBackpack.Prefab.GetComponent<ItemDrop>();
            var arcticItemData = arcticItemItemDrop.m_itemData;
            if (arcticItemData != null)
            {
                arcticItemData.m_shared.m_movementModifier = ConfigRegistry.SpeedModArctic.Value;
            }
            arcticItemItemDrop.Save();

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
                
                    if (BackpackTypes.Contains(item.m_shared.m_name))
                    {
                        switch (item.m_shared.m_name)
                        {
                            case RuggedBackpackName:
                                item.m_shared.m_movementModifier = ConfigRegistry.SpeedModRugged.Value;
                                break;
                            case ArcticBackpackName:
                                item.m_shared.m_movementModifier = ConfigRegistry.SpeedModArctic.Value;
                                break;
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
            var equipped = Player.m_localPlayer.GetInventory().GetEquipedtems();

            if (equipped != null)
            {
                SearchInventory(equipped);    
            }
        }
        
        public static Inventory NewInventoryInstance(string name)
        {
            Inventory newInventory = null;
            switch (name)
            {
                case RuggedBackpackName:
                    newInventory = new Inventory(
                        UiInventoryName,
                        null,
                        (int)ConfigRegistry.RuggedBackpackSize.Value.x,
                        (int)ConfigRegistry.RuggedBackpackSize.Value.y
                    );
                    break;
                case ArcticBackpackName:
                    newInventory = new Inventory(
                        UiInventoryName,
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
        
        public static BackpackComponent GetEquippedBackpack()
        {
            if (Player.m_localPlayer == null || Player.m_localPlayer.GetInventory() == null)
                return null;
            
            // Get a list of all equipped items.
            List<ItemDrop.ItemData> equippedItems = Player.m_localPlayer.GetInventory().GetEquipedtems();

            if (equippedItems is null) return null;

            // Go through all the equipped items, match them for any of the names in backpackTypes.
            // If a match is found, return the backpack ItemData object.
            foreach (ItemDrop.ItemData item in equippedItems)
            {
                if (BackpackTypes.Contains(item.m_shared.m_name))
                {
                    return item.Data().GetOrCreate<BackpackComponent>();
                }
            }

            // Return null if no backpacks are found.
            return null;
        }

        public static void ResetBackpackContainer()
        {
            _backpackContainer = null;
        }
        
        public static bool CanOpenBackpack()
        {
            _backpackEquipped = GetEquippedBackpack();

            // Return true if GetEquippedBackpack() does not return null.
            if (_backpackEquipped != null)
            {
                return true;
            }

            // Return false if GetEquippedBackpack() returns null.
            _log.Message("No backpack equipped. Can't open any.");
            return false;

        }
        
        public static void OpenBackpack()
        {
            
            _backpackContainer = Player.m_localPlayer.gameObject.GetComponent<Container>();
            
            if (_backpackContainer == null)
                _backpackContainer = Player.m_localPlayer.gameObject.AddComponent<Container>();

            _backpackContainer.m_inventory = _backpackEquipped.GetInventory();
            
            InventoryGui.instance.Show(_backpackContainer);
        }
        
        public static bool CheckForInception(Inventory __instance, ItemDrop.ItemData item)
        {
            if (__instance.m_name.Equals(Backpacks.BackpacksInventoryName))
            {
                // If the item is a backpack...
                if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name))
                {
                    if (Player.m_localPlayer != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$vapok_mod_no_inception");    
                    }

                    // Nope!
                    AdventureBackpacks.Log.Message("You can't put a backpack inside a backpack, silly!");
                    return false;
                }
            }

            return true;
        }
       
        public static void QuickDropBackpack()
        {
            _log.Message("Quick dropping backpack.");

            if (Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;
            var backpack = GetEquippedBackpack();

            if (backpack == null)
                return;
            
            // Unequip and remove backpack from player's back
            // We need to unequip the item BEFORE we drop it, otherwise when we pick it up again the game thinks
            // we had it equipped all along and fails to update player model, resulting in invisible backpack.
            player.RemoveEquipAction(backpack.Item);
            player.UnequipItem(backpack.Item, true);
            player.m_inventory.RemoveItem(backpack.Item);

            // This drops a copy of the backpack itemDrop.itemData
            var itemDrop = ItemDrop.DropItem(backpack.Item, 1, player.transform.position + player.transform.forward + player.transform.up, player.transform.rotation);
            itemDrop.Save();
        }
    }
}