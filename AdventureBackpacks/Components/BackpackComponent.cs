/* BackpackComponent.cs */

using System;
using System.Diagnostics;
using AdventureBackpacks.Assets;
using AdventureBackpacks.Extensions;
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
            if (Player.m_localPlayer != null && Player.m_localPlayer.IsThisBackpackEquipped(Item))
            {
                var container = Player.m_localPlayer.gameObject.GetComponent<Container>();
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

            // Return the data to be deserialized in the method below
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
                        catch {}
                        item.Save(pkg);
                    }
                }
            }
        }

        // This code is run on game start for objects with a BackpackComponent, and it converts the inventory info from string format (ZPackage) to object format (Inventory) so the game can use it.
        public void Deserialize(string data)
        {
            _log.Debug($"[Deserialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting.. data length: {data?.Length ?? 0}");
            try
            {
                var type = Item.m_shared.m_name;
                if (!Backpacks.TryGetBackpackItemByName(type, out var backpackDef))
                    return;

                var targetSize = backpackDef.GetInventorySize(Item.m_quality);
                if (_backpackInventory == null || _backpackInventory.m_width != targetSize.x || _backpackInventory.m_height != targetSize.y)
                {
                    _backpackInventory = Backpacks.NewInventoryInstance(type, Item.m_quality);
                }
                else
                {
                    _backpackInventory.m_inventory.Clear();
                }

                _log.Debug($"[Deserialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Value Before = {Value}");
                Value = data;
                _log.Debug($"[Deserialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Value After = {Value}");

                // Deserialising saved inventory data and storing it into the Inventory instance.
                ZPackage pkg = new ZPackage(data);
                _log.Debug($"[Deserialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Inventory Count Before Load: {_backpackInventory.m_inventory.Count}");
                _backpackInventory.Load(pkg);
                
                _log.Debug($"[Deserialize() - {Item.m_shared.m_name}-Q{Item.m_quality}] Inventory Count After Load: {_backpackInventory.m_inventory.Count}");
                
                SyncContainerInventory();

                //Update Status Effects
                _statusEffects = Backpacks.UpdateStatusEffects(Item);
            }
            catch (Exception ex)
            {
                _log.Error($" - {Item.m_shared.m_name} Backpack info is corrupt!\n{ex}");
            }
        }

        public override void FirstLoad()
        {
            var name = Item.m_shared.m_name;
            _log.Debug($"[FirstLoad - {Item.m_shared.m_name}-Q{Item.m_quality}] {name}");
            
            // Check whether the item created is of a type contained in backpackTypes
            if (Backpacks.BackpackTypes.Contains(name))
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    Deserialize(Value);
                }
                else if (Item.m_customData.ContainsKey(OldPluginCustomData) && !string.IsNullOrEmpty(Item.m_customData[OldPluginCustomData]))
                {
                    var oldBackpack = Item.m_customData[OldPluginCustomData];
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
            _log.Debug($"[Load - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting. Called by:\n{new StackTrace()}");
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

            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting Value = {Value}");
            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Starting backpack count {_backpackInventory.m_inventory.Count}");
            Value = Serialize();
            _log.Debug($"[Save() - {Item.m_shared.m_name}-Q{Item.m_quality}] Ending backpack count {_backpackInventory.m_inventory.Count}");
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