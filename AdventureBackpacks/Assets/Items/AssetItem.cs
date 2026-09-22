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

    internal AssetItem(string assetName, string prefabName, string itemName, string registerAs = null)
    {
        try
        {
            AssetName = assetName;
            PrefabName = string.IsNullOrEmpty(registerAs) ? prefabName : registerAs;
            ItemName = itemName;

            //registerAs keeps the old prefab name for saves and other mods. Renaming the loaded asset is enough.
            if (string.IsNullOrEmpty(registerAs))
            {
                _item = new Item(AssetName, prefabName, _assetFolderName)
                {
                    Configurable = Configurability.Disabled
                };
            }
            else
            {
                var bundle = ItemManager.PrefabManager.RegisterAssetBundle(AssetName, _assetFolderName);
                var prefab = bundle.LoadAsset<GameObject>(prefabName);
                if (prefab == null)
                {
                    //Bundle may still carry the original prefab
                    prefab = bundle.LoadAsset<GameObject>(registerAs);
                    if (prefab == null)
                        throw new Exception($"neither '{prefabName}' nor '{registerAs}' found in bundle '{AssetName}'");
                    AdventureBackpacks.Log?.Info($"Bundle '{AssetName}' has no '{prefabName}', using '{registerAs}'.");
                }
                prefab.name = registerAs;
                ApplyRegisteredIdentity(prefab, registerAs, itemName);
                _item = new Item(prefab)
                {
                    Configurable = Configurability.Disabled
                };
            }
            
            SetupItem();
        }
        catch (System.Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"Error initializing item '{itemName}' ({prefabName} from {assetName}): {ex.Message}");
        }
    }

    //Bundles from another project may come without icon or name key.
    private static void ApplyRegisteredIdentity(GameObject prefab, string registerAs, string itemName)
    {
        var shared = prefab.GetComponent<ItemDrop>()?.m_itemData?.m_shared;
        if (shared == null)
            return;

        shared.m_name = itemName;

        if (shared.m_icons != null && shared.m_icons.Length > 0 && shared.m_icons[0] != null)
            return;

        var sprite = LoadEmbeddedIcon(registerAs) ?? LoadAnyEmbeddedIcon(registerAs);
        if (sprite == null)
        {
            AdventureBackpacks.Log?.Warning($"Prefab '{registerAs}' has no icon and no embedded fallback. Using a blank one.");
            var blank = new Texture2D(1, 1);
            blank.SetPixel(0, 0, Color.clear);
            blank.Apply();
            sprite = Sprite.Create(blank, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        shared.m_icons = new[] { sprite };
    }

    private const string IconResourceFolder = ".Assets.Icons.";

    //Any icon is better than a blank square.
    private static Sprite LoadAnyEmbeddedIcon(string forPrefab)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.Contains(IconResourceFolder) || !resource.EndsWith(".png"))
                continue;

            var sprite = LoadEmbeddedIcon(resource);
            if (sprite == null)
                continue;

            AdventureBackpacks.Log?.Warning($"Prefab '{forPrefab}' has no icon. Borrowing '{resource}'.");
            return sprite;
        }
        return null;
    }

    private static Sprite LoadEmbeddedIcon(string name)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = name.Contains(IconResourceFolder) ? name : $"{assembly.GetName().Name}{IconResourceFolder}{name}.png";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;

        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        //ImageConversionModule needs netstandard 2.1, call it via reflection.
        var loadImage = HarmonyLib.AccessTools.Method("UnityEngine.ImageConversion:LoadImage", new[] { typeof(Texture2D), typeof(byte[]) });
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (loadImage == null || !(bool)loadImage.Invoke(null, new object[] { texture, bytes }))
            return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
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

