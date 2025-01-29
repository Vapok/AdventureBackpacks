using System;
using AdventureBackpacks.Configuration;
using ItemManager;
using UnityEngine;
using Vapok.Common.Managers.PieceManager;
using CraftingTable = ItemManager.CraftingTable;

namespace AdventureBackpacks.Assets.Items;

internal interface IAssetItem
{
    string PrefabName { get; }
    string ItemName { get; }
    Item Item { get; }
}
internal abstract class AssetItem : IAssetItem
{
    private string AssetName = "vapokbackpacks";
    private string AssetFolderName = "Assets.Bundles";
    private string _prefabName;
    private string _itemName;
    private Item _item;

    public string PrefabName => _prefabName;

    public string ItemName => _itemName;

    public Item Item => _item;

    internal AssetItem(GameObject goItem, string itemName)
    {
        _prefabName = goItem.name;
        _itemName = itemName;

        _item = new Item(goItem)
        {
            Configurable = Configurability.Disabled
        };
        
        SetupItem();
    }

    internal AssetItem(AssetBundle bundle, string prefabName, string itemName)
    {
        _prefabName = prefabName;
        _itemName = itemName;
        
        _item = new Item(bundle,prefabName)
        {
            Configurable = Configurability.Disabled
        };
        
        SetupItem();
    }

    internal AssetItem(string prefabName, string itemName)
    {
        _prefabName = prefabName;
        _itemName = itemName;

        _item = new Item(AssetName, PrefabName, AssetFolderName)
        {
            Configurable = Configurability.Disabled
        };
        
        SetupItem();
    }

    private void SetupItem()
    {
        SetPersistence();
        ResetPrefabArmor();
    }

    internal void AssignCraftingTable(CraftingTable craftingTable, int stationLevel)
    {
        _item.Crafting.Add(craftingTable,stationLevel);
    }

    internal void AssignCraftingTable(string craftingTable, int stationLevel)
    {
        if (Enum.TryParse<CraftingTable>(craftingTable, true, out var tableEnum))
        {
            _item.Crafting.Add(tableEnum,stationLevel);    
        }
        else
        {
            _item.Crafting.Add(craftingTable,stationLevel);
        }
    }

    internal void AddRecipeIngredient(string prefabName, int quantity)
    {
        _item.RequiredItems.Add(prefabName,quantity);
    }

    internal void AddUpgradeIngredient(string prefabName, int quantity)
    {
        _item.RequiredUpgradeItems.Add(prefabName,quantity);
    }

    internal ItemDrop GetItemDrop()
    {
        return _item?.Prefab.GetComponent<ItemDrop>();
    }

    internal void RegisterShaderSwap(MaterialReplacer.ShaderType shaderType = MaterialReplacer.ShaderType.PieceShader)
    {
        if (!ConfigRegistry.ReplaceShader.Value)
            return;
        
        MaterialReplacer.RegisterGameObjectForShaderSwap(_item.Prefab,shaderType);
    }

    internal void SetPersistence()
    {
        _item.Prefab.GetComponent<ZNetView>().m_persistent = true;
    }

    internal void ResetPrefabArmor()
    {
        var itemDrop = GetItemDrop();
        var itemData = itemDrop.m_itemData;
        if (itemData != null)
        {
            itemDrop.m_autoPickup = true;
            itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel;
            itemDrop.Save();
        }

    }
}

