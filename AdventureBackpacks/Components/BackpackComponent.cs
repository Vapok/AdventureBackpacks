/* BackpackComponent.cs */

using System;
using System.Diagnostics;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Extensions;
using AdventureBackpacks.Patches;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.StatusEffects;

namespace AdventureBackpacks.Components
{
    public class BackpackComponent : CustomItemData
    {
        public static string OldPluginCustomData = "JotunnBackpacks#JotunnBackpacks.BackpackComponent";

        private Inventory _backpackInventory;
        private CustomSE _statusEffects;

        public bool IsLoadingInventory = false;
        public bool IsEmptyingBackpack = false;

        private ILogIt _log = AdventureBackpacks.Log;

        public void SetInventory(Inventory inventoryInstance)
        {
            _backpackInventory = inventoryInstance;
            SyncContainerInventory();
            Save(_backpackInventory); 
        }

        public void SyncContainerInventory()
        {
            if (Player.m_localPlayer != null && Player.m_localPlayer.IsThisBackpackEquipped(Item) && InventoryGuiPatches.BackpackIsOpen)
            {
                var container = Player.m_localPlayer.GetBackpackContainerProxy(false);
                if (container != null && _backpackInventory != null)
                {
                    container.m_inventory = _backpackInventory;
                    container.m_width = _backpackInventory.m_width;
                    container.m_height = _backpackInventory.m_height;
                    if (Item?.m_shared?.m_icons != null && Item.m_shared.m_icons.Length > 0)
                        container.m_bkg = Item.m_shared.m_icons[0];
                }
            }
        }

        public bool InventoryNeedsValidating(Vector2i backpackDimension)
        {
            if (_backpackInventory == null)
                return false;

            return _backpackInventory.m_width != backpackDimension.x || _backpackInventory.m_height != backpackDimension.y;
        }
        
        public Inventory GetInventory()
        {
            return _backpackInventory;
        }

        public void UpdateContainerSizing(ref Container backpackContainer)
        {
            var inventory = GetInventory();
            if (backpackContainer == null || inventory == null)
                return;
            
            backpackContainer.m_inventory = inventory;
            backpackContainer.m_name = Item?.m_shared?.m_name ?? inventory.GetName();
            backpackContainer.m_width = inventory.m_width;
            backpackContainer.m_height = inventory.m_height;
            if (Item?.m_shared?.m_icons != null && Item.m_shared.m_icons.Length > 0)
                backpackContainer.m_bkg = Item.m_shared.m_icons[0];
        }

        public string Serialize()
        {
            _log.Debug($"[Serialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting..");
            // Store the Inventory as a ZPackage
            ZPackage pkg = new ZPackage();

            if (_backpackInventory == null)
                _backpackInventory = Backpacks.NewInventoryInstance(Item.m_shared.m_name, Item.m_quality);

            try
            {
                _backpackInventory.Save(pkg);
            }
            catch (Exception ex)
            {
                _log.Warning($"[Serialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Standard Inventory.Save threw an exception ({ex.GetType().Name}: {ex.Message}). Falling back to direct item serialization.");
                pkg.Clear();
                SaveInventoryDirect(_backpackInventory, pkg);
            }

            string data = pkg.GetBase64();
            Value = data;
            _log.Debug($"[Serialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Value = {Value}");

            return data;
        }

        private void SaveInventoryDirect(Inventory inventory, ZPackage pkg)
        {
            if (inventory == null || pkg == null)
                return;

            const int currentVersion = 109;
            pkg.Write(currentVersion);
            var items = inventory.m_inventory;
            pkg.Write((ushort)(items?.Count ?? 0));
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Data()?.Save();
                        }
                        catch (Exception ex)
                        {
                            _log.Warning($"Failed to save item data for {item.m_shared?.m_name}: {ex.Message}");
                        }
                        item.Save(pkg);
                    }
                }
            }
        }

        public void Deserialize(string data)
        {
            try
            {
                var itemName = Item?.m_shared?.m_name ?? "UnknownBackpack";
                var quality = Item?.m_quality ?? 1;

                _log.Debug($"[Deserialize() - {itemName}-Q{quality}] Starting.. data length: {data?.Length ?? 0}");

                if (Item?.m_shared == null || !Backpacks.TryGetBackpackItemByName(itemName, out var backpackDef))
                    return;

                var targetSize = backpackDef.GetInventorySize(quality);
                if (_backpackInventory == null || _backpackInventory.m_width != targetSize.x || _backpackInventory.m_height != targetSize.y)
                {
                    _backpackInventory = Backpacks.NewInventoryInstance(itemName, quality);
                }
                else
                {
                    _backpackInventory.m_inventory?.Clear();
                }

                _log.Debug($"[Deserialize() - {itemName}-Q{quality}] Value Before = {Value}");
                Value = data;
                _log.Debug($"[Deserialize() - {itemName}-Q{quality}] Value After = {Value}");

                ZPackage pkg = new ZPackage(data);
                _log.Debug($"[Deserialize() - {itemName}-Q{quality}] Inventory Count Before Load: {_backpackInventory?.m_inventory?.Count ?? 0}");
                _backpackInventory?.Load(pkg);
                
                _log.Debug($"[Deserialize() - {itemName}-Q{quality}] Inventory Count After Load: {_backpackInventory?.m_inventory?.Count ?? 0}");
                
                SyncContainerInventory();

                _statusEffects = Backpacks.UpdateStatusEffects(Item);
            }
            catch (Exception ex)
            {
                var itemName = Item?.m_shared?.m_name ?? "UnknownBackpack";
                _log.Error($" - {itemName} Backpack info is corrupt!\n{ex}");
            }
        }

        public override void FirstLoad()
        {
            if (Item?.m_shared == null)
                return;

            var name = Item.m_shared.m_name;
            _log.Debug($"[FirstLoad - {Item.m_shared.m_name}-Q{Item.m_quality}] {name}");
            
            if (Backpacks.BackpackTypes.Contains(name))
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    Deserialize(Value);
                }
                else if (Item.m_customData != null && Item.m_customData.TryGetValue(OldPluginCustomData, out var oldBackpack) && !string.IsNullOrEmpty(oldBackpack))
                {
                    Value = oldBackpack;
                    Deserialize(Value);
                }
                else if (_backpackInventory == null)
                {
                    _log.Debug($"[FirstLoad - {Item.m_shared.m_name}-Q{Item.m_quality}] Backpack null, creating...");
                    _backpackInventory = Backpacks.NewInventoryInstance(name, Item.m_quality);
                    Serialize();
                    _statusEffects = Backpacks.UpdateStatusEffects(Item);
                }
            }
        }
    
        public override void Load()
        {
            if (Item?.m_shared == null)
                return;

            _log.Debug($"[Load - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting.");
            IsLoadingInventory = true;

            if (!string.IsNullOrEmpty(Value))
            {
                _log.Debug($"[Load - {Item.m_shared.m_name}-Q{Item.m_quality}] Value = {Value}");
                Deserialize(Value);
            }
            else
            {
                if (_backpackInventory == null)
                {
                    _log.Debug($"[Load - {Item.m_shared.m_name}-Q{Item.m_quality}] Backpack null, creating...");
                    var name = Item.m_shared.m_name;
                    _backpackInventory = Backpacks.NewInventoryInstance(name, Item.m_quality);
                }
                
                Serialize();
                _statusEffects = Backpacks.UpdateStatusEffects(Item);
            }
            IsLoadingInventory = false;
        }

        public override void Save()
        {
            if (_backpackInventory == null)
            {
                Serialize();
                return;
            }

            if (Item?.m_shared == null)
                return;

            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting Value = {Value}");
            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting backpack count {_backpackInventory.m_inventory?.Count ?? 0}");
            Value = Serialize();
            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Ending backpack count {_backpackInventory.m_inventory?.Count ?? 0}");
        }

        public void Save(Inventory backpack)
        {
            if (backpack == null)
            {
                _log.Warning($"[Save(Inventory) - {Item.m_shared.m_name}-Q{Item.m_quality}] Ignoring save: inventory argument was null.");
                return;
            }

            _log.Debug($"[Save(Inventory) - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting backpack count {backpack.m_inventory.Count}");
            _backpackInventory = backpack;
            Save();
        }

        public CustomItemData Clone()
        {
            return MemberwiseClone() as CustomItemData;
        }
    }
}