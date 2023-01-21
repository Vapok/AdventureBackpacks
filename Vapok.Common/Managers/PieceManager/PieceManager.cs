using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Vapok.Common.Managers.PieceManager;

[PublicAPI]
public enum CraftingTable
{
    None,
    [InternalName("piece_workbench")] Workbench,
    [InternalName("piece_cauldron")] Cauldron,
    [InternalName("forge")] Forge,
    [InternalName("piece_artisanstation")] ArtisanTable,
    [InternalName("piece_stonecutter")] StoneCutter,
    Custom
}

public class InternalName : Attribute
{
    public readonly string internalName;
    public InternalName(string internalName) => this.internalName = internalName;
}

[PublicAPI]
public class ExtensionList
{
    public readonly List<ExtensionConfig> ExtensionStations = new();

    public void Set(CraftingTable table, int maxStationDistance = 5) => ExtensionStations.Add(new ExtensionConfig { Table = table, maxStationDistance = maxStationDistance});
    public void Set(string customTable, int maxStationDistance = 5) => ExtensionStations.Add(new ExtensionConfig { Table = CraftingTable.Custom, custom = customTable, maxStationDistance = maxStationDistance });
}

public struct ExtensionConfig
{
    public CraftingTable Table;
    public float maxStationDistance;
    public string? custom;
}

[PublicAPI]
public class CraftingStationList
{
    public readonly List<CraftingStationConfig> Stations = new();

    public void Set(CraftingTable table) => Stations.Add(new CraftingStationConfig { Table = table });
    public void Set(string customTable) => Stations.Add(new CraftingStationConfig { Table = CraftingTable.Custom, custom = customTable });
}

public struct CraftingStationConfig
{
    public CraftingTable Table;
    public int level;
    public string? custom;
}

[PublicAPI]
public enum BuildPieceCategory
{
    Misc = 0,
    Crafting = 1,
    Building = 2,
    Furniture = 3,
    All = 100,
    Custom
}

[PublicAPI]
public class RequiredResourcesList
{
    public readonly List<Requirement> Requirements = new();

    public void Add(string item, int amount, bool recover) => Requirements.Add(new Requirement
        { itemName = item, amount = amount, recover = recover });
}

public struct Requirement
{
    public string itemName;
    public int amount;
    public bool recover;
}

public struct SpecialProperties
{
    [Description("Admins should be the only ones that can build this piece.")]
    public bool AdminOnly;
    [Description("Turns off generating a config for this build piece.")]
    public bool NoConfig;
}

[PublicAPI]
public class BuildingPieceCategoryList
{
    public readonly List<BuildPieceTableConfig> BuildPieceCategories = new();

    public void Add(BuildPieceCategory category) => BuildPieceCategories.Add(
        new BuildPieceTableConfig
            { Category = category });

    public void Add(string customCategory) => BuildPieceCategories.Add(new BuildPieceTableConfig
        { Category = BuildPieceCategory.Custom, custom = customCategory });
}

public struct BuildPieceTableConfig
{
    public BuildPieceCategory Category;
    public string? custom;
}

[PublicAPI]
public class BuildPiece
{
    private class PieceConfig
    {
        public ConfigEntry<string> craft = null!;
        public ConfigEntry<BuildPieceCategory> category = null!;
        public ConfigEntry<string> customCategory = null!;
        public ConfigEntry<CraftingTable> extensionTable = null!;
        public ConfigEntry<string> customExtentionTable = null!;
        public ConfigEntry<float> maxStationDistance = null!;
        public ConfigEntry<CraftingTable> table = null!;
        public ConfigEntry<string> customTable = null!;
    }

    internal static readonly List<BuildPiece> registeredPieces = new();
    private static Dictionary<BuildPiece, PieceConfig> pieceConfigs = new();

    [Description("Disables generation of the configs for your pieces. This is global, this turns it off for all pieces in your mod.")]
    public static bool ConfigurationEnabled = true;

    public readonly GameObject Prefab;
    
    [Description("Specifies the resources needed to craft the piece.\nUse .Add to add resources with their internal ID and an amount.\nUse one .Add for each resource type the building piece should need.")]
    public readonly RequiredResourcesList RequiredItems = new();

    [Description("Sets the category for the building piece.")]
    public readonly BuildingPieceCategoryList Category = new();

    [Description(
        "Specifies the crafting station needed to build your piece.\nUse .Add to add a crafting station, using the CraftingTable enum and a minimum level for the crafting station.")]
    public CraftingStationList Crafting = new();
    
    [Description("Makes this piece a station extension")]
    public ExtensionList Extension = new();
    
    [Description("Change the extended/special properties of your build piece.")]
    public SpecialProperties SpecialProperties;

    private LocalizeKey? _name;

    public LocalizeKey Name
    {
        get
        {
            if (_name is { } name)
            {
                return name;
            }

            global::Piece data = Prefab.GetComponent<global::Piece>();
            if (data.m_name.StartsWith("$"))
            {
                _name = new LocalizeKey(data.m_name);
            }
            else
            {
                string key = "$piece_" + Prefab.name.Replace(" ", "_");
                _name = new LocalizeKey(key).English(data.m_name);
                data.m_name = key;
            }

            return _name;
        }
    }

    private LocalizeKey? _description;

    public LocalizeKey Description
    {
        get
        {
            if (_description is { } description)
            {
                return description;
            }

            global::Piece data = Prefab.GetComponent<global::Piece>();
            if (data.m_description.StartsWith("$"))
            {
                _description = new LocalizeKey(data.m_description);
            }
            else
            {
                string key = "$piece_" + Prefab.name.Replace(" ", "_") + "_description";
                _description = new LocalizeKey(key).English(data.m_description);
                data.m_description = key;
            }

            return _description;
        }
    }

    public BuildPiece(string assetBundleFileName, string prefabName, string folderName = "assets") : this(
        PiecePrefabManager.RegisterAssetBundle(assetBundleFileName, folderName), prefabName)
    {
    }

    public BuildPiece(AssetBundle bundle, string prefabName, bool addToCustom = false, string customPieceTable = "")
    {
        if (addToCustom)
        {
            Prefab = PiecePrefabManager.RegisterPrefab(bundle, prefabName, false, true, customPieceTable);
        }
        else
        {
            Prefab = PiecePrefabManager.RegisterPrefab(bundle, prefabName, true);
        }

        registeredPieces.Add(this);
    }

    private class ConfigurationManagerAttributes
    {
        [UsedImplicitly] public int? Order;
        [UsedImplicitly] public bool? Browsable;
        [UsedImplicitly] public string? Category;
        [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
    }

    private static object? configManager;

    internal static void Patch_FejdStartup()
    {
        Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");

        Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
        configManager = configManagerType == null ? null : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);
        void ReloadConfigDisplay() => configManagerType?.GetMethod("BuildSettingList")!.Invoke(configManager, Array.Empty<object>());
        
        
        if (ConfigurationEnabled)
        {
            bool SaveOnConfigSet = plugin.Config.SaveOnConfigSet;
            plugin.Config.SaveOnConfigSet = false;
            foreach (BuildPiece piece in registeredPieces)
            {
                if (piece.SpecialProperties.NoConfig) continue;
                PieceConfig cfg = pieceConfigs[piece] = new PieceConfig();
                global::Piece piecePrefab = piece.Prefab.GetComponent<global::Piece>();
                string pieceName = piecePrefab.m_name;
                string englishName = new Regex("['[\"\\]]").Replace(english.Localize(pieceName), "").Trim();
                string localizedName = Localization.instance.Localize(pieceName).Trim();

                int order = 0;

                cfg.category = config(localizedName, "Build Table Category",
                    piece.Category.BuildPieceCategories.First().Category,
                    new ConfigDescription($"Build Category where {localizedName} is available.", null,
                        new ConfigurationManagerAttributes { Order = --order, Category = localizedName }));
                ConfigurationManagerAttributes customTableAttributes = new()
                {
                    Order = --order, Browsable = cfg.category.Value == BuildPieceCategory.Custom,
                    Category = localizedName
                };
                cfg.customCategory = config(localizedName, "Custom Build Category",
                    piece.Category.BuildPieceCategories.First().custom ?? "",
                    new ConfigDescription("", null, customTableAttributes));

                void BuildTableConfigChanged(object o, EventArgs e)
                {
                    if (registeredPieces.Count > 0)
                    {
                        if (cfg.category.Value is BuildPieceCategory.Custom)
                        {
                            // broken
                            // piece.Prefab.GetComponent<Piece>().m_category = (Piece.PieceCategory)ZNetScene.instance.GetPrefab(cfg.customCategory.Value)?.GetComponent<Piece>().m_category;
                        }
                        else
                        {
                            piecePrefab.m_category = (global::Piece.PieceCategory)cfg.category.Value;
                        }
                    }

                    customTableAttributes.Browsable = cfg.category.Value == BuildPieceCategory.Custom;
                    ReloadConfigDisplay();
                }

                cfg.category.SettingChanged += BuildTableConfigChanged;
                cfg.customCategory.SettingChanged += BuildTableConfigChanged;

                if (cfg.category.Value != BuildPieceCategory.Custom)
                {
                    piecePrefab.m_category = (global::Piece.PieceCategory)cfg.category.Value;
                }

                if (piece.Extension.ExtensionStations.Count > 0)
                {
                    StationExtension pieceExtensionComp = piece.Prefab.GetOrAddComponent<StationExtension>();
                    cfg.extensionTable = config(localizedName, "Extends Station",
                        piece.Extension.ExtensionStations.First().Table,
                        new ConfigDescription($"Crafting station that {localizedName} extends.", null,
                            new ConfigurationManagerAttributes { Order = --order }));
                    cfg.customExtentionTable = config(localizedName, "Custom Extend Station",
                        piece.Extension.ExtensionStations.First().custom ?? "",
                        new ConfigDescription("", null, customTableAttributes));
                    cfg.maxStationDistance = config(localizedName, "Max Station Distance",
                        piece.Extension.ExtensionStations.First().maxStationDistance,
                        new ConfigDescription($"Distance from the station that {localizedName} can be placed.", null,
                            new ConfigurationManagerAttributes { Order = --order }));
                    List<ConfigurationManagerAttributes> hideWhenNoneAttributes = new();

                    void ExtensionTableConfigChanged(object o, EventArgs e)
                    {
                        if (piece.RequiredItems.Requirements.Count > 0)
                        {
                            switch (cfg.extensionTable.Value)
                            {
                                case CraftingTable.Custom:
                                    pieceExtensionComp.m_craftingStation = ZNetScene.instance
                                        .GetPrefab(cfg.customExtentionTable.Value)?.GetComponent<CraftingStation>();
                                    break;
                                default:
                                    pieceExtensionComp.m_craftingStation = ZNetScene.instance
                                        .GetPrefab(
                                            ((InternalName)typeof(CraftingTable).GetMember(cfg.extensionTable.Value
                                                .ToString())[0].GetCustomAttributes(typeof(InternalName)).First())
                                            .internalName).GetComponent<CraftingStation>();
                                    break;
                            }

                            pieceExtensionComp.m_maxStationDistance = cfg.maxStationDistance.Value;
                        }

                        customTableAttributes.Browsable = cfg.extensionTable.Value == CraftingTable.Custom;
                        foreach (ConfigurationManagerAttributes attributes in hideWhenNoneAttributes)
                        {
                            attributes.Browsable = cfg.extensionTable.Value != CraftingTable.None;
                        }

                        ReloadConfigDisplay();
                        plugin.Config.Save();
                    }

                    cfg.extensionTable.SettingChanged += ExtensionTableConfigChanged;
                    cfg.customExtentionTable.SettingChanged += ExtensionTableConfigChanged;
                    cfg.maxStationDistance.SettingChanged += ExtensionTableConfigChanged;

                    ConfigurationManagerAttributes tableLevelAttributes = new()
                        { Order = --order, Browsable = cfg.extensionTable.Value != CraftingTable.None };
                    hideWhenNoneAttributes.Add(tableLevelAttributes);
                }

                if (piece.Crafting.Stations.Count > 0)
                {
                    List<ConfigurationManagerAttributes> hideWhenNoneAttributes = new();

                    cfg.table = config(localizedName, "Crafting Station", piece.Crafting.Stations.First().Table,
                        new ConfigDescription($"Crafting station where {localizedName} is available.", null,
                            new ConfigurationManagerAttributes { Order = --order }));
                    cfg.customTable = config(localizedName, "Custom Crafting Station",
                        piece.Crafting.Stations.First().custom ?? "",
                        new ConfigDescription("", null, customTableAttributes));

                    void TableConfigChanged(object o, EventArgs e)
                    {
                        if (piece.RequiredItems.Requirements.Count > 0)
                        {
                            switch (cfg.table.Value)
                            {
                                case CraftingTable.None:
                                    piecePrefab.m_craftingStation = null;
                                    break;
                                case CraftingTable.Custom:
                                    piecePrefab.m_craftingStation = ZNetScene.instance.GetPrefab(cfg.customTable.Value)
                                        ?.GetComponent<CraftingStation>();
                                    break;
                                default:
                                    piecePrefab.m_craftingStation = ZNetScene.instance
                                        .GetPrefab(
                                            ((InternalName)typeof(CraftingTable).GetMember(cfg.table.Value.ToString())
                                                [0].GetCustomAttributes(typeof(InternalName)).First()).internalName)
                                        .GetComponent<CraftingStation>();
                                    break;
                            }
                        }

                        customTableAttributes.Browsable = cfg.table.Value == CraftingTable.Custom;
                        foreach (ConfigurationManagerAttributes attributes in hideWhenNoneAttributes)
                        {
                            attributes.Browsable = cfg.table.Value != CraftingTable.None;
                        }

                        ReloadConfigDisplay();
                        plugin.Config.Save();
                    }

                    cfg.table.SettingChanged += TableConfigChanged;
                    cfg.customTable.SettingChanged += TableConfigChanged;

                    ConfigurationManagerAttributes tableLevelAttributes = new()
                        { Order = --order, Browsable = cfg.table.Value != CraftingTable.None };
                    hideWhenNoneAttributes.Add(tableLevelAttributes);
                }

                ConfigEntry<string> itemConfig(string name, string value, string desc)
                {
                    ConfigurationManagerAttributes attributes = new()
                        { CustomDrawer = DrawConfigTable, Order = --order, Category = localizedName };
                    return config(localizedName, name, value, new ConfigDescription(desc, null, attributes));
                }

                cfg.craft = itemConfig("Crafting Costs",
                    new SerializedRequirements(piece.RequiredItems.Requirements).ToString(),
                    $"Item costs to craft {localizedName}");
                cfg.craft.SettingChanged += (_, _) =>
                {
                    if (ObjectDB.instance && ObjectDB.instance.GetItemPrefab("Wood") != null)
                    {
                        global::Piece.Requirement[] requirements =
                            SerializedRequirements.toPieceReqs(new SerializedRequirements(cfg.craft.Value));
                        piecePrefab.m_resources = requirements;
                        foreach (global::Piece instantiatedPiece in UnityEngine.Object.FindObjectsOfType<global::Piece>())
                        {
                            if (instantiatedPiece.m_name == pieceName)
                            {
                                instantiatedPiece.m_resources = requirements;
                            }
                        }
                    }
                };
            }
            if (SaveOnConfigSet)
            {
                plugin.Config.SaveOnConfigSet = true;
                plugin.Config.Save();
            }
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_ObjectDBInit(ObjectDB __instance)
    {
        if (__instance.GetItemPrefab("Wood") == null)
        {
            return;
        }

        foreach (BuildPiece piece in registeredPieces)
        {
            pieceConfigs.TryGetValue(piece, out PieceConfig? cfg);
            piece.Prefab.GetComponent<global::Piece>().m_resources = SerializedRequirements.toPieceReqs(cfg == null
                ? new SerializedRequirements(piece.RequiredItems.Requirements)
                : new SerializedRequirements(cfg.craft.Value));
            foreach (ExtensionConfig station in piece.Extension.ExtensionStations)
            {
                switch ((cfg == null || piece.Extension.ExtensionStations.Count > 0 ? station.Table : cfg.extensionTable.Value))
                {
                    case CraftingTable.None:
                        piece.Prefab.GetComponent<StationExtension>().m_craftingStation = null;
                        break;
                    case CraftingTable.Custom when ZNetScene.instance.GetPrefab(cfg == null || piece.Extension.ExtensionStations.Count > 0 ? station.custom : cfg.customExtentionTable.Value) is { } craftingTable:
                        piece.Prefab.GetComponent<StationExtension>().m_craftingStation = craftingTable.GetComponent<CraftingStation>();
                        break;
                    case CraftingTable.Custom:
                        LogManager.Log.Warning($"Custom crafting station '{(cfg == null || piece.Extension.ExtensionStations.Count > 0 ? station.custom : cfg.customExtentionTable.Value)}' does not exist");
                        break;
                    default:
                    {
                        if (cfg != null && cfg.table.Value == CraftingTable.None)
                        {
                            piece.Prefab.GetComponent<StationExtension>().m_craftingStation = null;
                        }
                        else
                        {
                            piece.Prefab.GetComponent<StationExtension>().m_craftingStation = ZNetScene.instance
                                .GetPrefab(((InternalName)typeof(CraftingTable).GetMember(
                                    (cfg == null || piece.Extension.ExtensionStations.Count > 0 ? station.Table : cfg.extensionTable.Value)
                                    .ToString())[0].GetCustomAttributes(typeof(InternalName)).First()).internalName)
                                .GetComponent<CraftingStation>();
                        }

                        break;
                    }
                }
            }
            foreach (CraftingStationConfig station in piece.Crafting.Stations)
            {
                switch ((cfg == null || piece.Crafting.Stations.Count > 0 ? station.Table : cfg.table.Value))
                {
                    case CraftingTable.None:
                        piece.Prefab.GetComponent<global::Piece>().m_craftingStation = null;
                        break;
                    case CraftingTable.Custom when ZNetScene.instance.GetPrefab(cfg == null || piece.Crafting.Stations.Count > 0 ? station.custom : cfg.customTable.Value) is { } craftingTable:
                        piece.Prefab.GetComponent<global::Piece>().m_craftingStation = craftingTable.GetComponent<CraftingStation>();
                        break;
                    case CraftingTable.Custom:
                        LogManager.Log.Warning($"Custom crafting station '{(cfg == null || piece.Crafting.Stations.Count > 0 ? station.custom : cfg.customTable.Value)}' does not exist");
                        break;
                    default:
                    {
                        if (cfg != null && cfg.table.Value == CraftingTable.None)
                        {
                            piece.Prefab.GetComponent<global::Piece>().m_craftingStation = null;
                        }
                        else
                        {
                            piece.Prefab.GetComponent<global::Piece>().m_craftingStation = ZNetScene.instance
                                .GetPrefab(((InternalName)typeof(CraftingTable).GetMember(
                                    (cfg == null || piece.Crafting.Stations.Count > 0 ? station.Table : cfg.table.Value)
                                    .ToString())[0].GetCustomAttributes(typeof(InternalName)).First()).internalName)
                                .GetComponent<CraftingStation>();
                        }

                        break;
                    }
                }
            }

        }
    }

    private static void DrawConfigTable(ConfigEntryBase cfg)
    {
        bool locked = cfg.Description.Tags
            .Select(a =>
                a.GetType().Name == "ConfigurationManagerAttributes"
                    ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)
                    : null).FirstOrDefault(v => v != null) ?? false;

        List<Requirement> newReqs = new();
        bool wasUpdated = false;

        int RightColumnWidth = (int)(configManager?.GetType().GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic)!.GetGetMethod(true).Invoke(configManager, Array.Empty<object>()) ?? 130);

        GUILayout.BeginVertical();
        foreach (Requirement req in new SerializedRequirements((string)cfg.BoxedValue).Reqs)
        {
            GUILayout.BeginHorizontal();

            int amount = req.amount;
            if (int.TryParse(
                    GUILayout.TextField(amount.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }),
                    out int newAmount) && newAmount != amount && !locked)
            {
                amount = newAmount;
                wasUpdated = true;
            }

            string newItemName = GUILayout.TextField(req.itemName, new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 40 - 67 - 21 - 21 - 12 });
            string itemName = locked ? req.itemName : newItemName;
            wasUpdated = wasUpdated || itemName != req.itemName;

            bool recover = req.recover;
            if (GUILayout.Toggle(req.recover, "Recover", new GUIStyle(GUI.skin.toggle) { fixedWidth = 67 }) !=
                req.recover)
            {
                recover = !recover;
                wasUpdated = true;
            }

            if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                wasUpdated = true;
            }
            else
            {
                newReqs.Add(new Requirement { amount = amount, itemName = itemName, recover = recover });
            }

            if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
            {
                wasUpdated = true;
                newReqs.Add(new Requirement { amount = 1, itemName = "", recover = false });
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        if (wasUpdated)
        {
            cfg.BoxedValue = new SerializedRequirements(newReqs).ToString();
        }
    }

    private class SerializedRequirements
    {
        public readonly List<Requirement> Reqs;

        public SerializedRequirements(List<Requirement> reqs) => Reqs = reqs;

        public SerializedRequirements(string reqs)
        {
            Reqs = reqs.Split(',').Select(r =>
            {
                string[] parts = r.Split(':');
                return new Requirement
                {
                    itemName = parts[0],
                    amount = parts.Length > 1 && int.TryParse(parts[1], out int amount) ? amount : 1,
                    recover = parts.Length <= 2 || !bool.TryParse(parts[2], out bool recover) || recover,
                };
            }).ToList();
        }

        public override string ToString()
        {
            return string.Join(",", Reqs.Select(r => $"{r.itemName}:{r.amount}:{r.recover}"));
        }

        public static global::Piece.Requirement[] toPieceReqs(SerializedRequirements craft)
        {
            ItemDrop? ResItem(Requirement r)
            {
                ItemDrop? item = ObjectDB.instance.GetItemPrefab(r.itemName)?.GetComponent<ItemDrop>();
                if (item == null)
                {
                    LogManager.Log.Warning($"The required item '{r.itemName}' does not exist.");
                }

                return item;
            }

            Dictionary<string, global::Piece.Requirement?> resources = craft.Reqs.Where(r => r.itemName != "")
                .ToDictionary(r => r.itemName,
                    r => ResItem(r) is { } item
                        ? new global::Piece.Requirement { m_amount = r.amount, m_resItem = item, m_recover = r.recover }
                        : null);

            return resources.Values.Where(v => v != null).ToArray()!;
        }
    }

    private static Localization? _english;

    private static Localization english => _english ??= LocalizationCache.ForLanguage("English");

    internal static BaseUnityPlugin? _plugin = null!;

    internal static BaseUnityPlugin plugin
    {
        get
        {
            if (_plugin is not null) return _plugin;
            IEnumerable<TypeInfo> types;
            try
            {
                types = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
            }

            _plugin = (BaseUnityPlugin)BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(types.First(t =>
                t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));

            return _plugin;
        }
    }

    private static bool hasConfigSync = true;
    private static object? _configSync;

    private static object? configSync
    {
        get
        {
            if (_configSync != null || !hasConfigSync) return _configSync;
            if (Assembly.GetExecutingAssembly().GetType("ServerSync.ConfigSync") is { } configSyncType)
            {
                _configSync = Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " PieceManager");
                configSyncType.GetField("CurrentVersion")
                    .SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
                configSyncType.GetProperty("IsLocked")!.SetValue(_configSync, true);
            }
            else
            {
                hasConfigSync = false;
            }

            return _configSync;
        }
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
    {
        ConfigEntry<T> configEntry = plugin.Config.Bind(group, name, value, description);

        configSync?.GetType().GetMethod("AddConfigEntry")!.MakeGenericMethod(typeof(T))
            .Invoke(configSync, new object[] { configEntry });

        return configEntry;
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, string description) =>
        config(group, name, value, new ConfigDescription(description));
}

public static class GoExtensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : UnityEngine.Component => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
}


[PublicAPI]
public class LocalizeKey
{
    private static readonly List<LocalizeKey> keys = new();

    public readonly string Key;
    public readonly Dictionary<string, string> Localizations = new();

    public LocalizeKey(string key) => Key = key.Replace("$", "");
    
    public void Alias(string alias)
    {
        Localizations.Clear();
        if (!alias.Contains("$"))
        {
            alias = $"${alias}";
        }
        Localizations["alias"] = alias;
        Localization.instance.AddWord(Key, Localization.instance.Localize(alias));
    }

    public LocalizeKey English(string key) => addForLang("English", key);
    public LocalizeKey Swedish(string key) => addForLang("Swedish", key);
    public LocalizeKey French(string key) => addForLang("French", key);
    public LocalizeKey Italian(string key) => addForLang("Italian", key);
    public LocalizeKey German(string key) => addForLang("German", key);
    public LocalizeKey Spanish(string key) => addForLang("Spanish", key);
    public LocalizeKey Russian(string key) => addForLang("Russian", key);
    public LocalizeKey Romanian(string key) => addForLang("Romanian", key);
    public LocalizeKey Bulgarian(string key) => addForLang("Bulgarian", key);
    public LocalizeKey Macedonian(string key) => addForLang("Macedonian", key);
    public LocalizeKey Finnish(string key) => addForLang("Finnish", key);
    public LocalizeKey Danish(string key) => addForLang("Danish", key);
    public LocalizeKey Norwegian(string key) => addForLang("Norwegian", key);
    public LocalizeKey Icelandic(string key) => addForLang("Icelandic", key);
    public LocalizeKey Turkish(string key) => addForLang("Turkish", key);
    public LocalizeKey Lithuanian(string key) => addForLang("Lithuanian", key);
    public LocalizeKey Czech(string key) => addForLang("Czech", key);
    public LocalizeKey Hungarian(string key) => addForLang("Hungarian", key);
    public LocalizeKey Slovak(string key) => addForLang("Slovak", key);
    public LocalizeKey Polish(string key) => addForLang("Polish", key);
    public LocalizeKey Dutch(string key) => addForLang("Dutch", key);
    public LocalizeKey Portuguese_European(string key) => addForLang("Portuguese_European", key);
    public LocalizeKey Portuguese_Brazilian(string key) => addForLang("Portuguese_Brazilian", key);
    public LocalizeKey Chinese(string key) => addForLang("Chinese", key);
    public LocalizeKey Japanese(string key) => addForLang("Japanese", key);
    public LocalizeKey Korean(string key) => addForLang("Korean", key);
    public LocalizeKey Hindi(string key) => addForLang("Hindi", key);
    public LocalizeKey Thai(string key) => addForLang("Thai", key);
    public LocalizeKey Abenaki(string key) => addForLang("Abenaki", key);
    public LocalizeKey Croatian(string key) => addForLang("Croatian", key);
    public LocalizeKey Georgian(string key) => addForLang("Georgian", key);
    public LocalizeKey Greek(string key) => addForLang("Greek", key);
    public LocalizeKey Serbian(string key) => addForLang("Serbian", key);
    public LocalizeKey Ukrainian(string key) => addForLang("Ukrainian", key);

    private LocalizeKey addForLang(string lang, string value)
    {
        Localizations[lang] = value;
        if (Localization.instance.GetSelectedLanguage() == lang)
        {
            Localization.instance.AddWord(Key, value);
        }
        else if (lang == "English" && !Localization.instance.m_translations.ContainsKey(Key))
        {
            Localization.instance.AddWord(Key, value);
        }
        return this;
    }

    [HarmonyPriority(Priority.LowerThanNormal)]
    internal static void AddLocalizedKeys(Localization __instance, string language)
    {
        foreach (LocalizeKey key in keys)
        {
            if (key.Localizations.TryGetValue(language, out string Translation) || key.Localizations.TryGetValue("English", out Translation))
            {
                __instance.AddWord(key.Key, Translation);
            }
            else if (key.Localizations.TryGetValue("alias", out string alias))
            {
                Localization.instance.AddWord(key.Key, Localization.instance.Localize(alias));
            }
        }
    }
}

public static class LocalizationCache
{
    private static readonly Dictionary<string, Localization> localizations = new();

    internal static void LocalizationPostfix(Localization __instance, string language)
    {
        if (localizations.FirstOrDefault(l => l.Value == __instance).Key is { } oldValue)
        {
            localizations.Remove(oldValue);
        }
        if (!localizations.ContainsKey(language))
        {
            localizations.Add(language, __instance);
        }
    }

    public static Localization ForLanguage(string? language = null)
    {
        if (localizations.TryGetValue(language ?? PlayerPrefs.GetString("language", "English"), out Localization localization))
        {
            return localization;
        }
        localization = new Localization();
        if (language is not null)
        {
            localization.SetupLanguage(language);
        }
        return localization;
    }
}


public class AdminSyncing
{
    private static bool isServer;
    [HarmonyPriority(Priority.VeryHigh)]
    internal static void AdminStatusSync(ZNet __instance)
    {
        isServer = __instance.IsServer();
        ZRoutedRpc.instance.Register<ZPackage>(BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync", RPC_AdminPieceAddRemove);

        IEnumerator WatchAdminListChanges()
        {
            List<string> currentList = new(ZNet.instance.m_adminList.GetList());
            for (;;)
            {
                yield return new WaitForSeconds(30);
                if (!ZNet.instance.m_adminList.GetList().SequenceEqual(currentList))
                {
                    currentList = new List<string>(ZNet.instance.m_adminList.GetList());
                    List<ZNetPeer> adminPeer = ZNet.instance.GetPeers().Where(p =>
                        ZNet.instance.ListContainsId(ZNet.instance.m_adminList,p.m_rpc.GetSocket().GetHostName())).ToList();
                    List<ZNetPeer> nonAdminPeer = ZNet.instance.GetPeers().Except(adminPeer).ToList();
                    SendAdmin(nonAdminPeer, false);
                    SendAdmin(adminPeer, true);

                    void SendAdmin(List<ZNetPeer> peers, bool isAdmin)
                    {
                        ZPackage package = new();
                        package.Write(isAdmin);
                        ZNet.instance.StartCoroutine(sendZPackage(peers, package));
                    }
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }

        if (isServer)
        {
            ZNet.instance.StartCoroutine(WatchAdminListChanges());
        }
    }

    private static IEnumerator sendZPackage(List<ZNetPeer> peers, ZPackage package)
    {
        if (!ZNet.instance)
        {
            yield break;
        }
        const int compressMinSize = 10000;

        if (package.GetArray() is { LongLength: > compressMinSize } rawData)
        {
            ZPackage compressedPackage = new();
            compressedPackage.Write(4);
            MemoryStream output = new();
            using (DeflateStream deflateStream = new(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                deflateStream.Write(rawData, 0, rawData.Length);
            }
            compressedPackage.Write(output.ToArray());
            package = compressedPackage;
        }
        List<IEnumerator<bool>> writers = peers.Where(peer => peer.IsReady()).Select(p => TellPeerAdminStatus(p, package)).ToList();
        writers.RemoveAll(writer => !writer.MoveNext());
        while (writers.Count > 0)
        {
            yield return null;
            writers.RemoveAll(writer => !writer.MoveNext());
        }
    }
    
    private static IEnumerator<bool> TellPeerAdminStatus(ZNetPeer peer, ZPackage package)
    {
        if (ZRoutedRpc.instance is not { } rpc)
        {
            yield break;
        }
        
        SendPackage(package);

        void SendPackage(ZPackage pkg)
        {
            string method = BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync";
            if (isServer)
            {
                peer.m_rpc.Invoke(method, pkg);
            }
            else
            {
                rpc.InvokeRoutedRPC(peer.m_server ? 0 : peer.m_uid, method, pkg);
            }
        }
    }

    internal static void RPC_AdminPieceAddRemove(long sender, ZPackage package)
    {
        ZNetPeer? currentPeer = ZNet.instance.GetPeer(sender);
        bool admin = false;
        try
        {
            admin = package.ReadBool();
        }
        catch
        {
            // ignore
        }

        if (isServer)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody,
                BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync", new ZPackage());
            if (ZNet.instance.ListContainsId(ZNet.instance.m_adminList,currentPeer.m_rpc.GetSocket().GetHostName()))
            {
                ZPackage pkg = new();
                pkg.Write(true);
                currentPeer.m_rpc.Invoke(BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync", pkg);
            }
        }
        else
        {
            // Remove everything they shouldn't be able to build by disabling and removing.
            foreach (BuildPiece piece in BuildPiece.registeredPieces)
            {
                if (!piece.SpecialProperties.AdminOnly) continue;
                global::Piece piecePrefab = piece.Prefab.GetComponent<global::Piece>();
                string pieceName = piecePrefab.m_name;
                string localizedName = Localization.instance.Localize(pieceName).Trim();
                if (!ObjectDB.instance || ObjectDB.instance.GetItemPrefab("Wood") == null) continue;
                foreach (global::Piece instantiatedPiece in UnityEngine.Object.FindObjectsOfType<global::Piece>())
                {
                    if (admin)
                    {
                        if (instantiatedPiece.m_name == pieceName)
                        {
                            instantiatedPiece.m_enabled = true;
                        }
                    }
                    else
                    {
                        if (instantiatedPiece.m_name == pieceName)
                        {
                            instantiatedPiece.m_enabled = false;
                        }
                    }
                }

                List<GameObject>? hammerPieces = ObjectDB.instance.GetItemPrefab("Hammer").GetComponent<ItemDrop>()
                    .m_itemData.m_shared.m_buildPieces
                    .m_pieces;
                if (admin)
                {
                    if (!hammerPieces.Contains(ZNetScene.instance.GetPrefab(piecePrefab.name)))
                        hammerPieces.Add(ZNetScene.instance.GetPrefab(piecePrefab.name));
                }
                else
                {
                    if (hammerPieces.Contains(ZNetScene.instance.GetPrefab(piecePrefab.name)))
                        hammerPieces.Remove(ZNetScene.instance.GetPrefab(piecePrefab.name));
                }
            }
        }
    }

}

[HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
class RegisterClientRPCPatch
{
    private static void Postfix(ZNet __instance, ZNetPeer peer)
    {
        if (!__instance.IsServer() || __instance.m_adminList == null)
        {
            peer.m_rpc.Register<ZPackage>(BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync",
                RPC_InitialAdminSync);
        }
        else
        {
            ZPackage packge = new();
            packge.Write(__instance.ListContainsId(__instance.m_adminList,peer.m_rpc.GetSocket().GetHostName()));

            peer.m_rpc.Invoke(BuildPiece._plugin?.Info.Metadata.Name + " PMAdminStatusSync", packge);
        }
    }

    private static void RPC_InitialAdminSync(ZRpc rpc, ZPackage package) =>
        AdminSyncing.RPC_AdminPieceAddRemove(0, package);
}

public static class PiecePrefabManager
{
    static PiecePrefabManager()
    {
        Harmony harmony = new("org.bepinex.helpers.PieceManager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(BuildPiece),
                nameof(BuildPiece.Patch_FejdStartup))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)),
            new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PiecePrefabManager),
                nameof(Patch_ZNetSceneAwake))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PiecePrefabManager),
                nameof(RefFixPatch_ZNetSceneAwake))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNet), nameof(ZNet.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(AdminSyncing),
                nameof(AdminSyncing.AdminStatusSync))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PiecePrefabManager),
                nameof(Patch_ObjectDBInit))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PiecePrefabManager),
                nameof(Patch_ObjectDBInit))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(BuildPiece),
                nameof(BuildPiece.Patch_ObjectDBInit))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.SetupLanguage)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocalizationCache),
                nameof(LocalizationCache.LocalizationPostfix))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.LoadCSV)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocalizeKey),
                nameof(LocalizeKey.AddLocalizedKeys))));
    }

    private struct BundleId
    {
        [UsedImplicitly] public string assetBundleFileName;
        [UsedImplicitly] public string folderName;
    }

    private static readonly Dictionary<BundleId, AssetBundle> bundleCache = new();

    public static AssetBundle RegisterAssetBundle(string assetBundleFileName, string folderName = "assets")
    {
        BundleId id = new() { assetBundleFileName = assetBundleFileName, folderName = folderName };
        if (!bundleCache.TryGetValue(id, out AssetBundle assets))
        {
            assets = bundleCache[id] =
                Resources.FindObjectsOfTypeAll<AssetBundle>().FirstOrDefault(a => a.name == assetBundleFileName) ??
                AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + $".{folderName}." +
                                               assetBundleFileName));
        }

        return assets;
    }

    private static readonly List<GameObject> piecePrefabs = new();
    private static readonly Dictionary<GameObject, string> customPiecePrefabs = new();
    private static readonly List<GameObject> ZnetOnlyPrefabs = new();

    public static GameObject RegisterPrefab(string assetBundleFileName, string prefabName,
        string folderName = "assets") =>
        RegisterPrefab(RegisterAssetBundle(assetBundleFileName, folderName), prefabName);

    public static GameObject RegisterPrefab(AssetBundle assets, string prefabName, bool addToPieceTable = false,
        bool addToCustomPieceTable = false, string customPieceTable = "")
    {
        GameObject prefab = assets.LoadAsset<GameObject>(prefabName);

        if (addToPieceTable)
        {
            piecePrefabs.Add(prefab);
        }
        else if (addToCustomPieceTable)
        {
            customPiecePrefabs.Add(prefab, customPieceTable);
        }
        else
        {
            ZnetOnlyPrefabs.Add(prefab);
        }

        return prefab;
    }

    /* Sprites Only! */
    public static Sprite RegisterSprite(string assetBundleFileName, string prefabName,
        string folderName = "assets") =>
        RegisterSprite(RegisterAssetBundle(assetBundleFileName, folderName), prefabName);

    public static Sprite RegisterSprite(AssetBundle assets, string prefabName)
    {
        Sprite prefab = assets.LoadAsset<Sprite>(prefabName);
        return prefab;
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void Patch_ZNetSceneAwake(ZNetScene __instance)
    {
        foreach (GameObject prefab in piecePrefabs.Concat(ZnetOnlyPrefabs).Concat(customPiecePrefabs.Keys))
        {
            if (!__instance.m_prefabs.Contains(prefab)) 
                __instance.m_prefabs.Add(prefab);
            
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void RefFixPatch_ZNetSceneAwake(ZNetScene __instance)
    {
        foreach (GameObject prefab in piecePrefabs.Concat(ZnetOnlyPrefabs).Concat(customPiecePrefabs.Keys))
        {
            if (__instance.m_prefabs.Contains(prefab))
            {
                if (prefab.GetComponent<StationExtension>())
                {
                    prefab.GetComponent<global::Piece>().m_isUpgrade = true;
                    prefab.GetComponent<StationExtension>().m_connectionPrefab = __instance.GetPrefab("piece_workbench_ext3").GetComponent<StationExtension>().m_connectionPrefab;
                    prefab.GetComponent<StationExtension>().m_connectionOffset = __instance.GetPrefab("piece_workbench_ext3").GetComponent<StationExtension>().m_connectionOffset;
                }
            }
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void Patch_ObjectDBInit(ObjectDB __instance)
    {
        if (__instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces is not
            { } hammerPieces)
        {
            return;
        }

        foreach (KeyValuePair<GameObject, string> customPiecePrefab in customPiecePrefabs)
        {
            if (__instance.GetItemPrefab(customPiecePrefab.Value)?.GetComponent<ItemDrop>().m_itemData.m_shared
                    .m_buildPieces is not
                { } customPieces)
            {
                continue;
            }

            if (customPieces.m_pieces.Contains(customPiecePrefab.Key))
            {
                continue;
            }

            customPieces.m_pieces.Add(customPiecePrefab.Key);
        }

        foreach (GameObject prefab in piecePrefabs)
        {
            if (hammerPieces.m_pieces.Contains(prefab))
            {
                return;
            }

            hammerPieces.m_pieces.Add(prefab);
        }
    }
}