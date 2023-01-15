/* BackpackComponent.cs
 * 
 *
 */

using System;
using System.Linq;
using AdventureBackpacks.Assets;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;


// Setting this .cs file to the same namespace as JotunnBackpacks.cs, so that I can call methods from within JotunnBackpacks.cs here.
namespace AdventureBackpacks.Components
{
    public class BackpackComponent : CustomItemData
    {
        public static string OldPluginID = "JotunnBackpacks";
        public static string OldPluginCustomData = "JotunnBackpacks#JotunnBackpacks.BackpackComponent";

        public Inventory BackpackInventory;

        private ILogIt _log = AdventureBackpacks.Log;

        public void SetInventory(Inventory inventoryInstance)
        {
            BackpackInventory = inventoryInstance;
            Save(BackpackInventory); // This writes the new data to the ItemData object, which will be saved whenever game saves the ItemData object.
        }

        public Inventory GetInventory()
        {
            return BackpackInventory;
        }

        public string Serialize()
        {
            _log.Debug($"[Serialize()] Starting..");
            // Store the Inventory as a ZPackage
            ZPackage pkg = new ZPackage();

            if (BackpackInventory == null)
                BackpackInventory = Backpacks.NewInventoryInstance(Item.m_shared.m_name);

            BackpackInventory.Save(pkg);

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
                if (BackpackInventory is null)
                {
                    _log.Debug($"[Deserialize()] backpack null");
                    // Figure out which backpack type we are deserializing data for by accessing the ItemData of the base class.
                    var type = Item.m_shared.m_name;
                    BackpackInventory = Backpacks.NewInventoryInstance(type);
                }

                //Save data to Value
                Value = data;
                _log.Debug($"[Deserialize()] Value = {Value}");
                // Deserialising saved inventory data and storing it into the newly initialised Inventory instance.
                ZPackage pkg = new ZPackage(data);
                BackpackInventory.Load(pkg);

                Save(BackpackInventory);

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
                if (BackpackInventory != null)
                {
                    return;
                }

                //Check to see if we have old EIDF Component Data
                var oldBackpackData = Item.Data(OldPluginID);
                if (oldBackpackData != null)
                {
                    if ( oldBackpackData.ItemData.m_customData.ContainsKey(OldPluginCustomData))
                    {
                        var oldBackpack = oldBackpackData.ItemData.m_customData[OldPluginCustomData];

                        BackpackInventory = Backpacks.NewInventoryInstance(name);
                        Value = oldBackpack;
                        Deserialize(Value);
                    }
                }
            }
        }

        public override void Load()
        {
            _log.Debug($"[Load] Starting");

            if (!string.IsNullOrEmpty(Value))
            {
                _log.Debug($"[FirstLoad] Value = {Value}");
                Deserialize(Value);
            }
            else
            {
                if (BackpackInventory == null)
                {
                    _log.Debug($"[Load] Backpack null, creating...");
                    var name = Item.m_shared.m_name;
                    BackpackInventory = Backpacks.NewInventoryInstance(name);
                }
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
            BackpackInventory = backpack;
            Save();
        }

        public CustomItemData Clone()
        {
            return MemberwiseClone() as CustomItemData;
        }
    }

    public static class BackpackExtensions
    {
        public static GameObject InitializeCustomData(this ItemDrop.ItemData itemData)
        {
            var prefab = itemData.m_dropPrefab;
            if (prefab != null)
            {
                var itemDropPrefab = prefab.GetComponent<ItemDrop>();
                var instanceData = itemData.Data().GetOrCreate<BackpackComponent>();

                var prefabData = itemDropPrefab.m_itemData.Data().GetOrCreate<BackpackComponent>();

                instanceData.Save(prefabData.BackpackInventory);
                
                return itemDropPrefab.gameObject;
            }

            return null;
        }
    }
}
