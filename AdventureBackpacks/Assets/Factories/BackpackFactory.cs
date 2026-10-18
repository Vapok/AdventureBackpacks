using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Assets.Items.BackpackItems;
using AdventureBackpacks.Configuration;
using BepInEx.Configuration;
using UnityEngine;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Factories;


internal class BackpackFactory : AssetFactory
{
    private static HashSet<BackpackItem> _backpackItems = new();
    private static bool _initialized;
    private static List<ABAPI.BackpackDefinition> _externalBackpacks = new();
    
    internal static IEnumerable<BackpackItem> BackpackItems => _backpackItems;
    

    internal BackpackFactory(ILogIt logger, ConfigSyncBase configSync) : base(logger, configSync)
    {
        if (!_initialized)
        {
            BackpackItem.SetConfig(configSync);
            BackpackItem.SetLogger(logger);
            _initialized = true;
        }
    }
    
    public static void RegisterExternalBackpack(ABAPI.BackpackDefinition backpackDefinition)
    {
        _externalBackpacks.Add(backpackDefinition);
    }


    private string ResolveModelBundle(string baseBundleName, string itemName, out ConfigEntry<ModelStyle> styleEntry, out ConfigEntry<string> texturePackEntry)
    {
        string configSection = $"Backpack: {itemName}";
        string englishSection = BackpackItem.SafeGetTranslation("English", configSection);
        string localizedCategory = Localization.instance?.Localize(configSection) ?? configSection;

        ConfigEntry<ModelStyle> modelStyle = null;
        ConfigSyncBase.UnsyncedConfig(englishSection, "Model Style", ModelStyle.Modern,
            new ConfigDescription("Select the visual model style for this backpack. Modern features detailed models. Classic uses the original mod model. (Requires Game Restart)",
                null,
                new ConfigurationManagerAttributes { Category = localizedCategory, Order = 20, DispName = "Model Style (Requires Game Restart)" }), ref modelStyle);

        if (modelStyle != null)
        {
            modelStyle.SettingChanged += (_, _) =>
            {
                AdventureBackpacks.Log?.Message($"Model Style changed for '{englishSection}' to {modelStyle.Value}. A game restart is required for changes to take effect.");
                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Adventure Backpacks: Game restart required for model changes to take effect.");
                }
            };
        }

        styleEntry = modelStyle;
        ModelStyle activeStyle = modelStyle?.Value ?? ModelStyle.Modern;

        string[] availablePacks = VisualPackRegistry.GetAvailablePacksFor(baseBundleName, activeStyle);
        string defaultPack = VisualPackRegistry.GetDefaultPackFor(baseBundleName, activeStyle);

        ConfigEntry<string> texturePack = null;
        ConfigSyncBase.UnsyncedConfig(englishSection, "Texture Pack", defaultPack,
            new ConfigDescription("Select the active texture/visual pack for this backpack. 'Native' uses default mod assets. (Requires Game Restart)",
                new AcceptableValueList<string>(availablePacks),
                new ConfigurationManagerAttributes { Category = localizedCategory, Order = 19, DispName = "Texture Pack (Requires Game Restart)" }), ref texturePack);

        if (texturePack != null)
        {
            texturePack.SettingChanged += (_, _) =>
            {
                AdventureBackpacks.Log?.Message($"Texture Pack changed for '{englishSection}' to '{texturePack.Value}'. A game restart is required for changes to take effect.");
                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Adventure Backpacks: Game restart required for texture pack changes to take effect.");
                }
            };
        }

        texturePackEntry = texturePack;

        string selectedPack = texturePack?.Value ?? VisualPackRegistry.Native;
        string resolvedBundle = VisualPackRegistry.ResolveBundle(selectedPack, baseBundleName, activeStyle);

        if (!string.Equals(selectedPack, VisualPackRegistry.Native, StringComparison.OrdinalIgnoreCase))
        {
            AdventureBackpacks.Log?.Message($"[AdventureBackpacks] '{itemName}' using texture pack '{selectedPack}' ({resolvedBundle})");
        }

        return resolvedBundle;
    }

    private T CreateBackpack<T>(string baseBundleName, string prefabName, string itemName, Func<string, string, string, T> factory) where T : BackpackItem
    {
        string bundleName = ResolveModelBundle(baseBundleName, itemName, out ConfigEntry<ModelStyle> modelStyle, out ConfigEntry<string> texturePack);
        T backpack = factory(bundleName, prefabName, itemName);
        backpack.ModelStyle = modelStyle;
        backpack.TexturePack = texturePack;
        return backpack;
    }

    internal override void CreateAssets()
    {
        _backpackItems.Add(CreateBackpack("backpack_meadows", "BackpackMeadows", "$vapok_mod_item_backpack_meadows",
            (bundle, prefab, item) => new BackpackMeadows(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_black_forest", "BackpackBlackForest", "$vapok_mod_item_backpack_blackforest",
            (bundle, prefab, item) => new BackpackBlackForest(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_swamp", "BackpackSwamp", "$vapok_mod_item_backpack_swamp",
            (bundle, prefab, item) => new BackpackSwamp(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_mountains", "BackpackMountains", "$vapok_mod_item_backpack_mountains",
            (bundle, prefab, item) => new BackpackMountains(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_plains", "BackpackPlains", "$vapok_mod_item_backpack_plains",
            (bundle, prefab, item) => new BackpackPlains(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_mistlands", "BackpackMistlands", "$vapok_mod_item_backpack_mistlands",
            (bundle, prefab, item) => new BackpackMistlands(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_ashlands", "BackpackAshlands", "$vapok_mod_item_backpack_ashlands",
            (bundle, prefab, item) => new BackpackAshlands(bundle, prefab, item)));

        _backpackItems.Add(CreateBackpack("backpack_deepnorth", "BackpackDeepNorth", "$vapok_mod_item_backpack_deepnorth",
            (bundle, prefab, item) => new BackpackDeepNorth(bundle, prefab, item)));

        _backpackItems.Add(new LegacyIronBackpack("vapokbackpacks", "CapeIronBackpack", "$vapok_mod_item_rugged_backpack"));
        _backpackItems.Add(new LegacySilverBackpack("vapokbackpacks", "CapeSilverBackpack", "$vapok_mod_item_arctic_backpack"));

        foreach (ABAPI.BackpackDefinition backpackDefinition in _externalBackpacks)
        {
            if (_backpackItems.Any(x => x.ItemName.Equals(backpackDefinition.ItemName))) return;
            
            ExternalBackpack newBackpack = backpackDefinition.BackPackGo != null ? 
                new ExternalBackpack(backpackDefinition, backpackDefinition.BackPackGo) : 
                new ExternalBackpack(backpackDefinition);
            
            _backpackItems.Add(newBackpack);
        }
    }

    internal static List<string> BackpackTypes()
    {
        return BackpackItems.Select(x => x.ItemName).ToList();
    }

    internal static void ApplyUpgraderResources(ObjectDB objectDB)
    {
        if (objectDB == null || objectDB.m_recipes == null)
            return;

        foreach (BackpackItem backpack in _backpackItems)
        {
            if (backpack.UpgraderIngredients.Count == 0)
                continue;

            List<Recipe> recipes = objectDB.m_recipes
                .Where(r => r != null && r.m_item != null && r.m_item.gameObject.name.Equals(backpack.PrefabName))
                .ToList();

            foreach (Recipe recipe in recipes)
            {
                List<Piece.Requirement> resourceList = recipe.m_resources.ToList();
                bool modified = false;

                foreach (KeyValuePair<string, int> upgraderIng in backpack.UpgraderIngredients)
                {
                    if (resourceList.Any(x => x.m_resItem != null && x.m_resItem.gameObject.name.Equals(upgraderIng.Key) && x.m_upgraderResource))
                        continue;

                    GameObject prefab = objectDB.GetItemPrefab(upgraderIng.Key);
                    if (prefab == null)
                        continue;

                    ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                    if (itemDrop == null)
                        continue;

                    resourceList.Add(new Piece.Requirement
                    {
                        m_resItem = itemDrop,
                        m_amount = upgraderIng.Value,
                        m_upgraderResource = true,
                        m_amountPerLevel = 0,
                        m_recover = false
                    });
                    modified = true;
                }

                if (modified)
                {
                    recipe.m_resources = resourceList.ToArray();
                }
            }
        }
    }
}