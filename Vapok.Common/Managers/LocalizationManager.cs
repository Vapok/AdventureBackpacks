using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using YamlDotNet.Serialization;


namespace Vapok.Common.Managers.LocalizationManager;

[PublicAPI]
public class Localizer
{
	private static bool _initialized = false;
	
	private static readonly Dictionary<string, Dictionary<string, Func<string>>> PlaceholderProcessors = new();

	private static readonly Dictionary<string, Dictionary<string, string>> loadedTexts = new();

	private static readonly ConditionalWeakTable<Localization, string> localizationLanguage = new();

	private static readonly List<WeakReference<Localization>> localizationObjects = new();

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
				_plugin = (BaseUnityPlugin)BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(types.First(t => t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));
			}
			return _plugin;
		}
	}

	private static readonly List<string> fileExtensions = new() { ".json", ".yml" };

	private static void UpdatePlaceholderText(Localization localization, string key)
	{
		localizationLanguage.TryGetValue(localization, out string language);
		string text = loadedTexts[language][key];
		if (PlaceholderProcessors.TryGetValue(key, out Dictionary<string, Func<string>> textProcessors))
		{
			text = textProcessors.Aggregate(text, (current, kv) => current.Replace("{" + kv.Key + "}", kv.Value()));
		}
		LogManager.Log.Debug($"Localization Update: Key: {key} Value: [{text}]");
		localization.AddWord(key, text);
	}

	public static void AddPlaceholder<T>(string key, string placeholder, ConfigEntry<T> config, Func<T, string>? convertConfigValue = null) where T : notnull
	{
		convertConfigValue ??= val => val.ToString();
		if (!PlaceholderProcessors.ContainsKey(key))
		{
			PlaceholderProcessors[key] = new Dictionary<string, Func<string>>();
		}
		void UpdatePlaceholder()
		{
			PlaceholderProcessors[key][placeholder] = () => convertConfigValue(config.Value);
			UpdatePlaceholderText(Localization.instance, key);
		}
		config.SettingChanged += (_, _) => UpdatePlaceholder();
		if (loadedTexts.ContainsKey(Localization.instance.GetSelectedLanguage()))
		{
			UpdatePlaceholder();
		}
	}

	public static void AddText(string key, string text)
	{
		List<WeakReference<Localization>> remove = new();
		foreach (WeakReference<Localization> reference in localizationObjects)
		{
			if (reference.TryGetTarget(out Localization localization))
			{
				Dictionary<string, string> texts = loadedTexts[localizationLanguage.GetOrCreateValue(localization)];
				if (!localization.m_translations.ContainsKey(key))
				{
					texts[key] = text;
					localization.AddWord(key, text);
				}
			}
			else
			{
				remove.Add(reference);
			}
		}
		foreach (WeakReference<Localization> reference in remove)
		{
			localizationObjects.Remove(reference);
		}
	}

	public static void Load() => LoadLocalization(Localization.instance, Localization.instance.GetSelectedLanguage());

	private static void LoadLocalization(Localization __instance, string language)
	{
		if (!localizationLanguage.Remove(__instance))
		{
			localizationObjects.Add(new WeakReference<Localization>(__instance));
		}
		
		
		localizationLanguage.Add(__instance, language);

		Dictionary<string, string> localizationFiles = new();
		
		var languageFilesFound = Directory
			.GetFiles(Path.GetDirectoryName(Paths.PluginPath)!, $"{plugin.Info.Metadata.Name}.*",
				SearchOption.AllDirectories).Where(f => fileExtensions.IndexOf(Path.GetExtension(f)) >= 0);

		foreach (var languageFile in languageFilesFound)
		{
			var languageKey = Path.GetFileNameWithoutExtension(languageFile).Split('.')[1];
			if (localizationFiles.ContainsKey(languageKey))
			{
				LogManager.Log.Warning($"{languageKey} has already been added. {languageFile} not added.");
				continue;
			}
			localizationFiles.Add(languageKey, languageFile);
		}

		if (LoadTranslationFromAssembly("English") is not { } englishAssemblyData)
		{
			throw new Exception($"Found no English localizations in mod {plugin.Info.Metadata.Name}. Expected an embedded resource translations/English.json or translations/English.yml.");
		}

		Dictionary<string, string>? localizationTexts = new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>?>(System.Text.Encoding.UTF8.GetString(englishAssemblyData));
		if (localizationTexts is null)
		{
			throw new Exception($"Localization for mod {plugin.Info.Metadata.Name} failed: Localization file was empty.");
		}

		string? localizationData = null;
		if (language != "English")
		{
			if (localizationFiles.ContainsKey(language))
			{
				localizationData = File.ReadAllText(localizationFiles[language]);
			}
			else if (LoadTranslationFromAssembly(language) is { } languageAssemblyData)
			{
				localizationData = System.Text.Encoding.UTF8.GetString(languageAssemblyData);
			}
		}
		if (localizationData is null && localizationFiles.ContainsKey("English"))
		{
			localizationData = File.ReadAllText(localizationFiles["English"]);
		}

		if (localizationData is not null)
		{
			foreach (KeyValuePair<string, string> kv in new DeserializerBuilder().IgnoreFields().Build().Deserialize<Dictionary<string, string>?>(localizationData) ?? new Dictionary<string, string>())
			{
				localizationTexts[kv.Key] = kv.Value;
			}
		}

		loadedTexts[language] = localizationTexts;
		foreach (KeyValuePair<string, string> s in localizationTexts)
		{
			UpdatePlaceholderText(__instance, s.Key);
		}
	}

	static Localizer()
	{
		Harmony harmony = new("org.bepinex.helpers.LocalizationManager");
		harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), nameof(Patch_FejdStartup))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.LoadCSV)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Localizer), nameof(LoadLocalization))));
	}

	public static void Init()
	{
		if (_initialized == false)
			_initialized = true;
	}

	[HarmonyPriority(Priority.First)]
	private static void Patch_FejdStartup()
	{
		Load();
	}
	
	private static byte[]? LoadTranslationFromAssembly(string language)
	{
		foreach (string extension in fileExtensions)
		{
			if (ReadEmbeddedFileBytes("translations." + language + extension) is { } data)
			{
				return data;
			}
			if (ReadEmbeddedFileBytes("Translations." + language + extension) is { } data2)
			{
				return data2;
			}
		}

		return null;
	}

	public static byte[]? ReadEmbeddedFileBytes(string resourceFileName, Assembly? containingAssembly = null)
	{
		using MemoryStream stream = new();
		containingAssembly ??= Assembly.GetCallingAssembly();
		if (containingAssembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(resourceFileName, StringComparison.Ordinal)) is { } name)
		{
			containingAssembly.GetManifestResourceStream(name)?.CopyTo(stream);
		}
		return stream.Length == 0 ? null : stream.ToArray();
	}
}