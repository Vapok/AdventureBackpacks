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
    
    private readonly string _assetFolderName = "Assets.Bundles";
    private readonly Item _item;

    public string AssetName { get; }
    public string PrefabName { get; }

    public string ItemName { get; }

    public Item Item => _item;

    internal AssetItem(GameObject goItem, string itemName)
    {
        try
        {
            PrefabName = goItem != null ? goItem.name : string.Empty;
            ItemName = itemName;

            _item = new Item(goItem)
            {
                Configurable = Configurability.Disabled
            };
            
            SetupItem();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing item '{itemName}': {ex.Message}");
        }
    }

    internal AssetItem(AssetBundle bundle, string prefabName, string itemName)
    {
        try
        {
            PrefabName = prefabName;
            ItemName = itemName;
            
            _item = new Item(bundle, prefabName)
            {
                Configurable = Configurability.Disabled
            };
            
            SetupItem();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing item '{itemName}' from bundle: {ex.Message}");
        }
    }

    internal AssetItem(string assetName, string prefabName, string itemName)
    {
        try
        {
            AssetName = assetName;
            PrefabName = prefabName;
            ItemName = itemName;

            _item = new Item(AssetName, PrefabName, _assetFolderName)
            {
                Configurable = Configurability.Disabled
            };
            
            SetupItem();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing item '{itemName}' ({prefabName} from {assetName}): {ex.Message}");
        }
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
        return _item?.Prefab != null ? _item.Prefab.GetComponent<ItemDrop>() : null;
    }

    internal void RegisterShaderSwap(MaterialReplacer.ShaderType shaderType = MaterialReplacer.ShaderType.PieceShader)
    {
        if (!ConfigRegistry.ReplaceShader.Value)
            return;

        if (_item?.Prefab != null)
        {
            MaterialReplacer.RegisterGameObjectForShaderSwap(_item.Prefab, shaderType);
        }
    }

    internal void SetPersistence()
    {
        if (_item?.Prefab != null)
        {
            var znetView = _item.Prefab.GetComponent<ZNetView>();
            if (znetView != null)
                znetView.m_persistent = true;
        }
    }

    internal void ResetPrefabArmor()
    {
        ItemDrop itemDrop = GetItemDrop();
        if (itemDrop == null)
            return;

        ItemDrop.ItemData itemData = itemDrop.m_itemData;
        if (itemData != null)
        {
            itemDrop.m_autoPickup = true;
            if (itemData.m_shared != null)
            {
                itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel;
            }
        }
    }
}

