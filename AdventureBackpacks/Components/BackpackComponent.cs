/* BackpackComponent.cs
 * 
 *
 */

using System;
using AdventureBackpacks.Assets;
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

        private ILogIt _log = AdventureBackpacks.Log;

        public void SetInventory(Inventory inventoryInstance)
        {
            _backpackInventory = inventoryInstance;
            Save(_backpackInventory); 
        }

        public Inventory GetInventory()
        {
            return _backpackInventory;
        }

        public string Serialize()
        {
            _log.Debug($"[Serialize()] Starting..");
            // Store the Inventory as a ZPackage
            ZPackage pkg = new ZPackage();

            if (_backpackInventory == null)
                _backpackInventory = Backpacks.NewInventoryInstance(Item.m_shared.m_name, Item.m_quality);

            _backpackInventory.Save(pkg);

            string data = pkg.GetBase64();
            Value = data;
            _log.Debug($"[Serialize()] Value = {Value}");

            // Return the data to be deserialized in the method below
            return data;
        }

        // This code is run on game start for objects with a BackpackComponent, and it converts the inventory info from string format (ZPackage) to object format (Inventory) so the game can use it.
        public void Deserialize(string data)
        {
            _log.Debug($"[Deserialize()] Starting..");
            try
            {
                //Always Fetch new Inventory Instance to resize backpack.
                //We're going to load the data anyways.
                var type = Item.m_shared.m_name;
                _backpackInventory = Backpacks.NewInventoryInstance(type, Item.m_quality);

                //Save data to Value
                Value = data;
                
                _log.Debug($"[Deserialize()] Value = {Value}");
                // Deserialising saved inventory data and storing it into the newly initialised Inventory instance.
                ZPackage pkg = new ZPackage(data);
                _backpackInventory.Load(pkg);
                
                //Update Status Effects
                _statusEffects = Backpacks.UpdateStatusEffects(Item);
            }
            catch (Exception ex)
            {
                _log.Error($"Backpack info is corrupt!\n{ex}");
            }
        }

        public override void FirstLoad()
        {
            var name = Item.m_shared.m_name;
            _log.Debug($"[FirstLoad] {name}");
            
            // Check whether the item created is of a type contained in backpackTypes
            if (Backpacks.BackpackTypes.Contains(name))
            {
                if (_backpackInventory == null)
                {
                    _backpackInventory = Backpacks.NewInventoryInstance(name, Item.m_quality);
                    
                    if (!string.IsNullOrEmpty(Value))
                    {
                        Deserialize(Value);
                    }

                    //Check to see if we have old Jotunn Backpack Component Data
                    if (Item.m_customData.ContainsKey(OldPluginCustomData) && string.IsNullOrEmpty(Value))
                    {
                        var oldBackpack = Item.m_customData[OldPluginCustomData];
                        Value = oldBackpack;
                        Deserialize(Value);
                    }
                    else
                    {
                        _log.Debug($"[Load] Backpack null, creating...");
                        Serialize();
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(Value))
                    {
                        Serialize();
                    }
                }
            }
        }
    
        public override void Load()
        {
            _log.Debug($"[Load] Starting");

            if (!string.IsNullOrEmpty(Value))
            {
                _log.Debug($"[Load] Value = {Value}");
                Deserialize(Value);
            }
            else
            {
                if (_backpackInventory == null)
                {
                    _log.Debug($"[Load] Backpack null, creating...");
                    var name = Item.m_shared.m_name;
                    _backpackInventory = Backpacks.NewInventoryInstance(name, Item.m_quality);
                }
                
                Serialize();
            }
        }

        public override void Save()
        {
            _log.Debug($"[Save()] Starting Value = {Value}");
            Value = Serialize();
        }

        public void Save(Inventory backpack)
        {
            _log.Debug($"[Save(Inventory)] Starting backpack count {backpack.m_inventory.Count}");
            _backpackInventory = backpack;
            Save();
        }

        public CustomItemData Clone()
        {
            return MemberwiseClone() as CustomItemData;
        }
    }
}
