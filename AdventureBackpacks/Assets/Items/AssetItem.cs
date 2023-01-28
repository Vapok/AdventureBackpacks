using ItemManager;
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
    private const string AssetName = "vapokbackpacks";
    private const string AssetFolderName = "Assets.Bundles";
    private string _prefabName;
    private string _itemName;
    private Item _item;

    public string PrefabName => _prefabName;

    public string ItemName => _itemName;

    public Item Item => _item;

    internal AssetItem(string prefabName, string itemName)
    {
        _prefabName = prefabName;
        _itemName = itemName;
        _item = new Item(AssetName, PrefabName, AssetFolderName)
        {
            Configurable = Configurability.Disabled
        };
        
        SetPersistence();
        ResetPrefabArmor();
    }

    internal void AssignCraftingTable(CraftingTable craftingTable, int stationLevel)
    {
        _item.Crafting.Add(craftingTable,stationLevel);
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
            itemData.m_shared.m_armor = itemData.m_shared.m_armorPerLevel;
            itemDrop.Save();
        }

    }
}

