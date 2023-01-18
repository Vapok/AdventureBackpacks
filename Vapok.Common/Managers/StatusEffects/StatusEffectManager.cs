using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Vapok.Common.Shared;

namespace Vapok.Common.Managers.StatusEffects;

[PublicAPI]
[Description("The ItemDrop effect to apply the status effect")]
public enum EffectType
{
    Equip,
    Attack,
    Consume,
    Set
}

public struct SE_Item
{
    public StatusEffect Effect;
    public EffectType Type;
}

[PublicAPI]
public class CustomSE
{
    private static readonly List<CustomSE> RegisteredEffects = new();
    private static readonly Dictionary<StatusEffect, CustomSE> CustomEffectMap = new();
    internal static readonly List<StatusEffect> CustomSEs = new();
    internal static readonly Dictionary<SE_Item, string> AddToPrefabs = new();

    [Description("Instance of the StatusEffect.")]
    public readonly StatusEffect Effect;

    [Description("Set the type of effect you are adding. If the item can attack, consume, or be equipped, change this to correspond.")]
    public EffectType Type;

    private string _folderName = "icons";
    private AssetBundle? _assetBundle;

    [Description("Sets the icon for the StatusEffect. Must be 64x64")]
    public string? Icon
    {
        get => IconName;
        set
        {
            IconName = value;
            IconSprite = IconName is null ? null : loadSprite(IconName);
            Effect.m_icon = IconSprite;
        }
    }

    [Description("Sets the icon for the StatusEffect. Must be 64x64")]
    public Sprite? IconSprite = null;

    private string? IconName = null;

    private LocalizeKey? _name;

    [Description("Sets the in-game name for the StatusEffect")]
    public LocalizeKey Name
    {
        get
        {
            if (_name is { } name)
            {
                return name;
            }

            StatusEffect data = Effect;
            if (data.m_name.StartsWith("$"))
            {
                _name = new LocalizeKey(data.m_name);
            }
            else
            {
                string key = "$statuseffect_" + Effect.name.Replace(" ", "_");
                _name = new LocalizeKey(key).English(data.m_name);
                data.m_name = key;
            }

            return _name;
        }
    }


    private static Localization? _english;

    private static Localization english
    {
        get
        {
            if (_english == null)
            {
                _english = new Localization();
                _english.SetupLanguage("English");
            }

            return _english;
        }
    }

    public CustomSE(string assetBundleFileName, string customEffectName, string folderName = "assets") : this(
        EffectManager.RegisterAssetBundle(assetBundleFileName, folderName), customEffectName)
    {
    }

    public CustomSE(AssetBundle bundle, string customEffectName)
    {
        Effect = EffectManager.RegisterCustomSE(bundle, customEffectName);
        _assetBundle = bundle;
        RegisteredEffects.Add(this);
        CustomEffectMap[Effect] = this;
    }

    public CustomSE(string customEffectName)
    {
        Effect = ScriptableObject.CreateInstance<StatusEffect>();
        EffectManager.RegisterCustomSE(Effect, customEffectName);
        RegisteredEffects.Add(this);
        CustomEffectMap[Effect] = this;
    }
    public CustomSE(Enums.StatusEffects statusEffect, string customEffectName)
    {
        switch (statusEffect)
        {
            case Enums.StatusEffects.Burning:
                Effect = ScriptableObject.CreateInstance<SE_Burning>();
                break;
            case Enums.StatusEffects.Cozy:
                Effect = ScriptableObject.CreateInstance<SE_Cozy>();
                break;
            case Enums.StatusEffects.Demister:
                Effect = ScriptableObject.CreateInstance<SE_Demister>();
                break;
            case Enums.StatusEffects.Finder:
                Effect = ScriptableObject.CreateInstance<SE_Finder>();
                break;
            case Enums.StatusEffects.Frost:
                Effect = ScriptableObject.CreateInstance<SE_Frost>();
                break;
            case Enums.StatusEffects.Harpooned:
                Effect = ScriptableObject.CreateInstance<SE_Harpooned>();
                break;
            case Enums.StatusEffects.HealthUpgrade:
                Effect = ScriptableObject.CreateInstance<SE_HealthUpgrade>();
                break;
            case Enums.StatusEffects.Poison:
                Effect = ScriptableObject.CreateInstance<SE_Poison>();
                break;
            case Enums.StatusEffects.Puke:
                Effect = ScriptableObject.CreateInstance<SE_Puke>();
                break;
            case Enums.StatusEffects.Rested:
                Effect = ScriptableObject.CreateInstance<SE_Rested>();
                break;
            case Enums.StatusEffects.Shield:
                Effect = ScriptableObject.CreateInstance<SE_Shield>();
                break;
            case Enums.StatusEffects.Smoke:
                Effect = ScriptableObject.CreateInstance<SE_Smoke>();
                break;
            case Enums.StatusEffects.Spawn:
                Effect = ScriptableObject.CreateInstance<SE_Spawn>();
                break;
            case Enums.StatusEffects.Stats:
                Effect = ScriptableObject.CreateInstance<SE_Stats>();
                break;
            case Enums.StatusEffects.Wet:
                Effect = ScriptableObject.CreateInstance<SE_Wet>();
                break;
            default:
                Effect = ScriptableObject.CreateInstance<StatusEffect>();
                break;
        }
            
        EffectManager.RegisterCustomSE(Effect, customEffectName);
        RegisteredEffects.Add(this);
        CustomEffectMap[Effect] = this;
    }

    private byte[]? ReadEmbeddedFileBytes(string name)
    {
        using MemoryStream stream = new();
        if (Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name +
                                                                      $"{(_folderName == "" ? "" : ".") + _folderName}." +
                                                                      name) is not { } assemblyStream)
        {
            return null;
        }

        assemblyStream.CopyTo(stream);
        return stream.ToArray();
    }

    private Texture2D? loadTexture(string name)
    {
        if (ReadEmbeddedFileBytes(name) is { } textureData)
        {
            Texture2D texture = new(0, 0);
            texture.LoadImage(textureData);
            return texture;
        }

        return null;
    }

    private Sprite loadSprite(string name)
    {
        if (loadTexture(name) is { } texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.zero);
        }

        if (_assetBundle?.LoadAsset<Sprite>(name) is { } sprite)
        {
            return sprite;
        }

        throw new FileNotFoundException($"Could not find a file named {name} for the effect icon");
    }

    [Description("Adds the CustomSE to a prefab by adding it on ZNetScene awake. The effect type will be used to determine when the effect applies.")]
    public void AddSEToPrefab(CustomSE customSE, string prefabName)
    {
        SE_Item test = new()
        {
            Effect = customSE.Effect, Type = customSE.Type
        };
        AddToPrefabs.Add(test, prefabName);
    }

    private static BaseUnityPlugin? _plugin;

    private static BaseUnityPlugin plugin
    {
        get
        {
            if (_plugin is null)
            {
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
            }

            return _plugin;
        }
    }

    private static bool hasConfigSync = true;
    private static object? _configSync;

    private static object? configSync
    {
        get
        {
            if (_configSync == null && hasConfigSync)
            {
                if (Assembly.GetExecutingAssembly().GetType("ServerSync.ConfigSync") is { } configSyncType)
                {
                    _configSync =
                        Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " ItemManager");
                    configSyncType.GetField("CurrentVersion")
                        .SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
                    configSyncType.GetProperty("IsLocked")!.SetValue(_configSync, true);
                }
                else
                {
                    hasConfigSync = false;
                }
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

[PublicAPI]
public class LocalizeKey
{
    public readonly string Key;

    public LocalizeKey(string key) => Key = key.Replace("$", "");

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
}

public static class EffectManager
{
    static EffectManager()
    {
        Harmony harmony = new("org.bepinex.helpers.StatusEffectManager");
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(EffectManager), nameof(Patch_ObjectDBInit))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(EffectManager),
                nameof(Patch_ZNetSceneAwake))));
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
                    .GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name +
                                               $"{(folderName == "" ? "" : ".") + folderName}." +
                                               assetBundleFileName));
        }

        return assets;
    }


    public static StatusEffect RegisterCustomSE(string assetBundleFileName, string customEffectName,
        string folderName = "assets") =>
        RegisterCustomSE(RegisterAssetBundle(assetBundleFileName, folderName), customEffectName);

    public static StatusEffect RegisterCustomSE(AssetBundle assets, string customEffectName)
    {
        StatusEffect customSE = (StatusEffect)assets.LoadAsset<ScriptableObject>(customEffectName);

        CustomSE.CustomSEs.Add(customSE);
        return customSE;
    }

    public static StatusEffect RegisterCustomSE(StatusEffect customSE, string customEffectName)
    {
        customSE.name = customEffectName;
        CustomSE.CustomSEs.Add(customSE);
        return customSE;
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void Patch_ObjectDBInit(ObjectDB __instance)
    {
        foreach (StatusEffect? statusEffect in CustomSE.CustomSEs)
        {
            if (!__instance.m_StatusEffects.Contains(statusEffect))
            {
                __instance.m_StatusEffects.Add(statusEffect);
            }
        }

        __instance.UpdateItemHashes();
    }

    [HarmonyPriority(Priority.VeryHigh)]
    private static void Patch_ZNetSceneAwake(ZNetScene __instance)
    {
        foreach (KeyValuePair<SE_Item, string> valuePair in CustomSE.AddToPrefabs)
        {
            try
            {
                GameObject? prefab = __instance.GetPrefab(valuePair.Value);
                ItemDrop? itemDrop =
                    prefab ? prefab.GetComponent<ItemDrop>() : prefab.GetComponentInChildren<ItemDrop>();
                Aoe? aoe = prefab ? prefab.GetComponent<Aoe>() : prefab.GetComponentInChildren<Aoe>();
                EffectArea? effectArea =
                    prefab ? prefab.GetComponent<EffectArea>() : prefab.GetComponentInChildren<EffectArea>();
                if (itemDrop)
                {
                    switch (valuePair.Key.Type)
                    {
                        case EffectType.Equip:
                            itemDrop.m_itemData.m_shared.m_equipStatusEffect =
                                valuePair.Key.Effect;
                            break;
                        case EffectType.Attack:
                            itemDrop.m_itemData.m_shared.m_attackStatusEffect =
                                valuePair.Key.Effect;
                            break;
                        case EffectType.Consume:
                            itemDrop.m_itemData.m_shared.m_consumeStatusEffect =
                                valuePair.Key.Effect;
                            break;
                        case EffectType.Set:
                            itemDrop.m_itemData.m_shared.m_setSize = 1;
                            itemDrop.m_itemData.m_shared.m_setName = valuePair.Key.Effect.name;
                            itemDrop.m_itemData.m_shared.m_setStatusEffect =
                                valuePair.Key.Effect;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                else if (aoe)
                {
                    aoe.m_statusEffect = valuePair.Key.Effect.name;
                }

                else if (effectArea)
                {
                    effectArea.m_statusEffect = valuePair.Key.Effect.name;
                }
                else
                {
                    LogManager.Log.Warning(
                        $"The prefab '{prefab.name}' does not have an ItemDrop, AOE, or EffectArea component. Cannot add the StatusEffect to the prefab.");
                }
            }
            catch (Exception e)
            {
                LogManager.Log.Warning(
                    $"BROKE - {e.Message}");
            }
        }
    }
}