using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace ItemManager;

[PublicAPI]
public enum CraftingTable
{
	Disabled,
	Inventory,
	[InternalName("piece_workbench")] Workbench,
	[InternalName("piece_cauldron")] Cauldron,
	[InternalName("forge")] Forge,
	[InternalName("piece_artisanstation")] ArtisanTable,
	[InternalName("piece_stonecutter")] StoneCutter,
	[InternalName("piece_magetable")] MageTable,
	[InternalName("blackforge")] BlackForge,
	Custom
}

[PublicAPI]
public enum ConversionPiece
{
	Disabled,
	[InternalName("smelter")] Smelter,
	[InternalName("charcoal_kiln")] CharcoalKiln,
	[InternalName("blastfurnace")] BlastFurnace,
	[InternalName("windmill")] Windmill,
	[InternalName("piece_spinningwheel")] SpinningWheel,
	[InternalName("eitrrefinery")] EitrRefinery,
	Custom
}

public class InternalName : Attribute
{
	public readonly string internalName;
	public InternalName(string internalName) => this.internalName = internalName;
}

[PublicAPI]
public class RequiredResourceList
{
	public readonly List<Requirement> Requirements = new();
	public bool Free = false; // If Requirements empty and Free is true, then it costs nothing. If Requirements empty and Free is false, then it won't be craftable.

	public void Add(string itemName, int amount) => Requirements.Add(new Requirement { itemName = itemName, amount = amount });
	public void Add(string itemName, ConfigEntry<int> amountConfig) => Requirements.Add(new Requirement { itemName = itemName, amountConfig = amountConfig });
}

[PublicAPI]
public class CraftingStationList
{
	public readonly List<CraftingStationConfig> Stations = new();

	public void Add(CraftingTable table, int level) => Stations.Add(new CraftingStationConfig { Table = table, level = level });
	public void Add(string customTable, int level) => Stations.Add(new CraftingStationConfig { Table = CraftingTable.Custom, level = level, custom = customTable });
}

[PublicAPI]
public class ItemRecipe
{
	public readonly RequiredResourceList RequiredItems = new();
	public readonly RequiredResourceList RequiredUpgradeItems = new();
	public readonly CraftingStationList Crafting = new();
	public int CraftAmount = 1;
	public ConfigEntryBase? RecipeIsActive = null;
}

public struct Requirement
{
	public string itemName;
	public int amount;
	public ConfigEntry<int>? amountConfig;
}

public struct CraftingStationConfig
{
	public CraftingTable Table;
	public int level;
	public string? custom;
}

[Flags]
public enum Configurability
{
	Disabled = 0,
	Recipe = 1,
	Stats = 2,
	Drop = 4,
	Full = Recipe | Drop | Stats,
}

public class DropTargets
{
	public readonly List<DropTarget> Drops = new();

	public void Add(string creatureName, float chance, int min = 1, int? max = null)
	{
		Drops.Add(new DropTarget { creature = creatureName, chance = chance, min = min, max = max ?? min });
	}
}

public struct DropTarget
{
	public string creature;
	public int min;
	public int max;
	public float chance;
}

[PublicAPI]
public class Item
{
	private class ItemConfig
	{
		public ConfigEntry<string>? craft;
		public ConfigEntry<string>? upgrade;
		public ConfigEntry<CraftingTable> table = null!;
		public ConfigEntry<int> tableLevel = null!;
		public ConfigEntry<string> customTable = null!;
		public ConfigEntry<int>? maximumTableLevel;
	}

	private static readonly List<Item> registeredItems = new();
	private static readonly Dictionary<ItemDrop, Item> itemDropMap = new();
	private static Dictionary<Item, Dictionary<string, List<Recipe>>> activeRecipes = new();
	private static Dictionary<Recipe, ConfigEntryBase?> hiddenCraftRecipes = new();
	private static Dictionary<Recipe, ConfigEntryBase?> hiddenUpgradeRecipes = new();
	private static Dictionary<Item, Dictionary<string, ItemConfig>> itemCraftConfigs = new();
	private static Dictionary<Item, ConfigEntry<string>> itemDropConfigs = new();
	private Dictionary<CharacterDrop, CharacterDrop.Drop> characterDrops = new();
	private readonly Dictionary<ConfigEntryBase, Action> statsConfigs = new();

	public static Configurability DefaultConfigurability = Configurability.Full;
	public Configurability? Configurable = null;
	private Configurability configurability => Configurable ?? DefaultConfigurability;
	private Configurability configurationVisible = Configurability.Full;

	public readonly GameObject Prefab;

	[Description("Specifies the resources needed to craft the item.\nUse .Add to add resources with their internal ID and an amount.\nUse one .Add for each resource type the item should need.")]
	public RequiredResourceList RequiredItems => this[""].RequiredItems;

	[Description("Specifies the resources needed to upgrade the item.\nUse .Add to add resources with their internal ID and an amount. This amount will be multipled by the item quality level.\nUse one .Add for each resource type the upgrade should need.")]
	public RequiredResourceList RequiredUpgradeItems => this[""].RequiredUpgradeItems;

	[Description("Specifies the crafting station needed to craft the item.\nUse .Add to add a crafting station, using the CraftingTable enum and a minimum level for the crafting station.\nUse one .Add for each crafting station.")]
	public CraftingStationList Crafting => this[""].Crafting;

	[Description("Specifies a config entry which toggles whether a recipe is active.")]
	public ConfigEntryBase? RecipeIsActive
	{
		get => this[""].RecipeIsActive;
		set => this[""].RecipeIsActive = value;
	}

	[Description("Specifies the number of items that should be given to the player with a single craft of the item.\nDefaults to 1.")]
	public int CraftAmount
	{
		get => this[""].CraftAmount;
		set => this[""].CraftAmount = value;
	}

	[Description("Specifies the maximum required crafting station level to upgrade and repair the item.\nDefault is calculated from crafting station level and maximum quality.")]
	public int MaximumRequiredStationLevel = int.MaxValue;

	[Description("Assigns the item as a drop item to a creature.\nUses a creature name, a drop chance and a minimum and maximum amount.")]
	public readonly DropTargets DropsFrom = new();

	internal List<Conversion> Conversions = new();
	internal List<Smelter.ItemConversion> conversions = new();
	public Dictionary<string, ItemRecipe> Recipes = new();

	public ItemRecipe this[string name]
	{
		get
		{
			if (Recipes.TryGetValue(name, out ItemRecipe recipe))
			{
				return recipe;
			}
			return Recipes[name] = new ItemRecipe();
		}
	}

	private LocalizeKey? _name;

	public LocalizeKey Name
	{
		get
		{
			if (_name is { } name)
			{
				return name;
			}

			ItemDrop.ItemData.SharedData data = Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
			if (data.m_name.StartsWith("$"))
			{
				_name = new LocalizeKey(data.m_name);
			}
			else
			{
				string key = "$item_" + Prefab.name.Replace(" ", "_");
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

			ItemDrop.ItemData.SharedData data = Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
			if (data.m_description.StartsWith("$"))
			{
				_description = new LocalizeKey(data.m_description);
			}
			else
			{
				string key = "$itemdesc_" + Prefab.name.Replace(" ", "_");
				_description = new LocalizeKey(key).English(data.m_description);
				data.m_description = key;
			}
			return _description;
		}
	}

	public Item(string assetBundleFileName, string prefabName, string folderName = "assets") : this(PrefabManager.RegisterAssetBundle(assetBundleFileName, folderName), prefabName)
	{
	}

	public Item(AssetBundle bundle, string prefabName) : this(PrefabManager.RegisterPrefab(bundle, prefabName, true), true)
	{
	}

	public Item(GameObject prefab, bool skipRegistering = false)
	{
		if (!skipRegistering)
		{
			PrefabManager.RegisterPrefab(prefab, true);
		}
		Prefab = prefab;
		registeredItems.Add(this);
		itemDropMap[Prefab.GetComponent<ItemDrop>()] = this;
	}

	public void ToggleConfigurationVisibility(Configurability visible)
	{
		void Toggle(ConfigEntryBase cfg, Configurability check)
		{
			foreach (object? tag in cfg.Description.Tags)
			{
				if (tag is ConfigurationManagerAttributes attrs)
				{
					attrs.Browsable = (visible & check) != 0 && (attrs.browsability is null || attrs.browsability());
				}
			}
		}
		void ToggleObj(object obj, Configurability check)
		{
			foreach (FieldInfo field in obj.GetType().GetFields())
			{
				if (field.GetValue(obj) is ConfigEntryBase cfg)
				{
					Toggle(cfg, check);
				}
			}
		}

		configurationVisible = visible;
		if (itemDropConfigs.TryGetValue(this, out ConfigEntry<string> dropCfg))
		{
			Toggle(dropCfg, Configurability.Drop);
		}
		if (itemCraftConfigs.TryGetValue(this, out Dictionary<string, ItemConfig> craftCfgs))
		{
			foreach (ItemConfig craftCfg in craftCfgs.Values)
			{
				ToggleObj(craftCfg, Configurability.Recipe);
			}
		}
		foreach (Conversion conversion in Conversions)
		{
			if (conversion.config is not null)
			{
				ToggleObj(conversion.config, Configurability.Recipe);
			}
		}
		foreach (KeyValuePair<ConfigEntryBase, Action> cfg in statsConfigs)
		{
			Toggle(cfg.Key, Configurability.Stats);
			if ((visible & Configurability.Stats) != 0)
			{
				cfg.Value();
			}
		}
		reloadConfigDisplay();
	}

	private class ConfigurationManagerAttributes
	{
		[UsedImplicitly] public int? Order;
		[UsedImplicitly] public bool? Browsable;
		[UsedImplicitly] public string? Category;
		[UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
		public Func<bool>? browsability;
	}

	[PublicAPI]
	public enum DamageModifier
	{
		Normal,
		Resistant,
		Weak,
		Immune,
		Ignore,
		VeryResistant,
		VeryWeak,
		None
	}

	private static object? configManager;

	private delegate void setDmgFunc(ref HitData.DamageTypes dmg, float value);

	internal static void reloadConfigDisplay() => configManager?.GetType().GetMethod("BuildSettingList")!.Invoke(configManager, Array.Empty<object>());

	internal static void Patch_FejdStartup()
	{
		Assembly? bepinexConfigManager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ConfigurationManager");

		Type? configManagerType = bepinexConfigManager?.GetType("ConfigurationManager.ConfigurationManager");
		configManager = configManagerType == null ? null : BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent(configManagerType);

		if (DefaultConfigurability != Configurability.Disabled)
		{
			bool SaveOnConfigSet = plugin.Config.SaveOnConfigSet;
			plugin.Config.SaveOnConfigSet = false;

			foreach (Item item in registeredItems.Where(i => i.configurability != Configurability.Disabled))
			{
				string nameKey = item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
				string englishName = new Regex("['[\"\\]]").Replace(english.Localize(nameKey), "").Trim();
				string localizedName = Localization.instance.Localize(nameKey).Trim();

				if ((item.configurability & Configurability.Recipe) != 0)
				{
					itemCraftConfigs[item] = new Dictionary<string, ItemConfig>();
					foreach (string configKey in item.Recipes.Keys.DefaultIfEmpty(""))
					{
						int order = 0;

						string configSuffix = configKey == "" ? "" : $" ({configKey})";
						ItemConfig cfg = itemCraftConfigs[item][configKey] = new ItemConfig();

						if (item.Recipes.ContainsKey(configKey) && item.Recipes[configKey].Crafting.Stations.Count > 0)
						{
							List<ConfigurationManagerAttributes> hideWhenNoneAttributes = new();

							cfg.table = config(englishName, "Crafting Station" + configSuffix, item.Recipes[configKey].Crafting.Stations.First().Table, new ConfigDescription($"Crafting station where {englishName} is available.", null, new ConfigurationManagerAttributes { Order = --order, Browsable = (item.configurationVisible & Configurability.Recipe) != 0, Category = localizedName }));
							bool CustomTableBrowsability() => cfg.table.Value == CraftingTable.Custom;
							ConfigurationManagerAttributes customTableAttributes = new() { Order = --order, browsability = CustomTableBrowsability, Browsable = CustomTableBrowsability() && (item.configurationVisible & Configurability.Recipe) != 0, Category = localizedName };
							cfg.customTable = config(englishName, "Custom Crafting Station" + configSuffix, item.Recipes[configKey].Crafting.Stations.First().custom ?? "", new ConfigDescription("", null, customTableAttributes));

							void TableConfigChanged(object o, EventArgs e)
							{
								if (activeRecipes.ContainsKey(item) && activeRecipes[item].TryGetValue(configKey, out List<Recipe> recipes))
								{
									recipes.First().m_enabled = cfg.table.Value != CraftingTable.Disabled;

									if (cfg.table.Value is CraftingTable.Inventory or CraftingTable.Disabled)
									{
										recipes.First().m_craftingStation = null;
									}
									else if (cfg.table.Value is CraftingTable.Custom)
									{
										recipes.First().m_craftingStation = ZNetScene.instance.GetPrefab(cfg.customTable.Value)?.GetComponent<CraftingStation>();
									}
									else
									{
										recipes.First().m_craftingStation = ZNetScene.instance.GetPrefab(getInternalName(cfg.table.Value)).GetComponent<CraftingStation>();
									}
								}
								customTableAttributes.Browsable = cfg.table.Value == CraftingTable.Custom;
								foreach (ConfigurationManagerAttributes attributes in hideWhenNoneAttributes)
								{
									attributes.Browsable = cfg.table.Value != CraftingTable.Disabled;
								}
								reloadConfigDisplay();
							}
							cfg.table.SettingChanged += TableConfigChanged;
							cfg.customTable.SettingChanged += TableConfigChanged;

							bool TableLevelBrowsability() => cfg.table.Value != CraftingTable.Disabled;
							ConfigurationManagerAttributes tableLevelAttributes = new() { Order = --order, browsability = TableLevelBrowsability, Browsable = TableLevelBrowsability() && (item.configurationVisible & Configurability.Recipe) != 0, Category = localizedName };
							hideWhenNoneAttributes.Add(tableLevelAttributes);
							cfg.tableLevel = config(englishName, "Crafting Station Level" + configSuffix, item.Recipes[configKey].Crafting.Stations.First().level, new ConfigDescription($"Required crafting station level to craft {englishName}.", null, tableLevelAttributes));
							cfg.tableLevel.SettingChanged += (_, _) =>
							{
								if (activeRecipes.ContainsKey(item) && activeRecipes[item].TryGetValue(configKey, out List<Recipe> recipes))
								{
									recipes.First().m_minStationLevel = cfg.tableLevel.Value;
								}
							};
							if (item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality > 1)
							{
								cfg.maximumTableLevel = config(englishName, "Maximum Crafting Station Level" + configSuffix, item.MaximumRequiredStationLevel == int.MaxValue ? item.Recipes[configKey].Crafting.Stations.First().level + item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality - 1 : item.MaximumRequiredStationLevel, new ConfigDescription($"Maximum crafting station level to upgrade and repair {englishName}.", null, tableLevelAttributes));
							}

							ConfigEntry<string> itemConfig(string name, string value, string desc)
							{
								bool ItemBrowsability() => cfg.table.Value != CraftingTable.Disabled;
								ConfigurationManagerAttributes attributes = new() { CustomDrawer = drawRequirementsConfigTable, Order = --order, browsability = ItemBrowsability, Browsable = ItemBrowsability() && (item.configurationVisible & Configurability.Recipe) != 0, Category = localizedName };
								hideWhenNoneAttributes.Add(attributes);
								return config(englishName, name, value, new ConfigDescription(desc, null, attributes));
							}

							if ((!item.Recipes[configKey].RequiredItems.Free || item.Recipes[configKey].RequiredItems.Requirements.Count > 0) && item.Recipes[configKey].RequiredItems.Requirements.All(r => r.amountConfig is null))
							{
								cfg.craft = itemConfig("Crafting Costs" + configSuffix, new SerializedRequirements(item.Recipes[configKey].RequiredItems.Requirements).ToString(), $"Item costs to craft {englishName}");
							}
							if (item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxQuality > 1 && (!item.Recipes[configKey].RequiredUpgradeItems.Free || item.Recipes[configKey].RequiredUpgradeItems.Requirements.Count > 0) && item.Recipes[configKey].RequiredUpgradeItems.Requirements.All(r => r.amountConfig is null))
							{
								cfg.upgrade = itemConfig("Upgrading Costs" + configSuffix, new SerializedRequirements(item.Recipes[configKey].RequiredUpgradeItems.Requirements).ToString(), $"Item costs per level to upgrade {englishName}");
							}

							void ConfigChanged(object o, EventArgs e)
							{
								if (ObjectDB.instance && activeRecipes.ContainsKey(item) && activeRecipes[item].TryGetValue(configKey, out List<Recipe> recipes))
								{
									foreach (Recipe recipe in recipes)
									{
										recipe.m_resources = SerializedRequirements.toPieceReqs(ObjectDB.instance, new SerializedRequirements(cfg.craft?.Value ?? ""), new SerializedRequirements(cfg.upgrade?.Value ?? ""));
									}
								}
							}

							if (cfg.craft != null)
							{
								cfg.craft.SettingChanged += ConfigChanged;
							}
							if (cfg.upgrade != null)
							{
								cfg.upgrade.SettingChanged += ConfigChanged;
							}
						}
					}

					if ((item.configurability & Configurability.Drop) != 0)
					{
						ConfigEntry<string> dropConfig = itemDropConfigs[item] = config(englishName, "Drops from", new SerializedDrop(item.DropsFrom.Drops).ToString(), new ConfigDescription($"Creatures {englishName} drops from", null, new ConfigurationManagerAttributes { CustomDrawer = drawDropsConfigTable, Category = localizedName, Browsable = (item.configurationVisible & Configurability.Drop) != 0 }));
						dropConfig.SettingChanged += (_, _) => item.UpdateCharacterDrop();
					}

					for (int i = 0; i < item.Conversions.Count; ++i)
					{
						string prefix = item.Conversions.Count > 1 ? $"{i + 1}. " : "";
						Conversion conversion = item.Conversions[i];
						conversion.config = new Conversion.ConversionConfig();
						int index = i;

						void UpdatePiece()
						{
							if (index >= item.conversions.Count || !ZNetScene.instance)
							{
								return;
							}
							string? newPieceName = conversion.config.piece.Value is not ConversionPiece.Disabled ? conversion.config.piece.Value == ConversionPiece.Custom ? conversion.config.customPiece.Value : getInternalName(conversion.config.piece.Value) : null;
							string? activePiece = conversion.config.activePiece;
							if (conversion.config.activePiece is not null)
							{
								Smelter smelter = ZNetScene.instance.GetPrefab(conversion.config.activePiece).GetComponent<Smelter>();
								int removeIndex = smelter.m_conversion.IndexOf(item.conversions[index]);
								if (removeIndex >= 0)
								{
									foreach (Smelter instantiatedSmelter in Resources.FindObjectsOfTypeAll<Smelter>())
									{
										if (Utils.GetPrefabName(instantiatedSmelter.gameObject) == activePiece)
										{
											instantiatedSmelter.m_conversion.RemoveAt(removeIndex);
										}
									}
								}
								conversion.config.activePiece = null;
							}
							if (item.conversions[index].m_from is not null && conversion.config.piece.Value is not ConversionPiece.Disabled)
							{
								if (ZNetScene.instance.GetPrefab(newPieceName)?.GetComponent<Smelter>() is not null)
								{
									conversion.config.activePiece = newPieceName;
									foreach (Smelter instantiatedSmelter in Resources.FindObjectsOfTypeAll<Smelter>())
									{
										if (Utils.GetPrefabName(instantiatedSmelter.gameObject) == newPieceName)
										{
											instantiatedSmelter.m_conversion.Add(item.conversions[index]);
										}
									}
								}
							}
						}

						conversion.config.input = config(englishName, $"{prefix}Conversion Input Item", conversion.Input, new ConfigDescription($"Duration of conversion to create {englishName}", null, new ConfigurationManagerAttributes { Category = localizedName, Browsable = (item.configurationVisible & Configurability.Recipe) != 0 }));
						conversion.config.input.SettingChanged += (_, _) =>
						{
							if (index < item.conversions.Count && ObjectDB.instance is { } objectDB)
							{
								ItemDrop? inputItem = SerializedRequirements.fetchByName(objectDB, conversion.config.input.Value);
								item.conversions[index].m_from = inputItem;
								UpdatePiece();
							}
						};
						conversion.config.piece = config(englishName, $"{prefix}Conversion Piece", conversion.Piece, new ConfigDescription($"Duration of conversion to create {englishName}", null, new ConfigurationManagerAttributes { Category = localizedName, Browsable = (item.configurationVisible & Configurability.Recipe) != 0 }));
						conversion.config.piece.SettingChanged += (_, _) => UpdatePiece();
						conversion.config.customPiece = config(englishName, $"{prefix}Conversion Custom Piece", conversion.customPiece ?? "", new ConfigDescription($"Duration of conversion to create {englishName}", null, new ConfigurationManagerAttributes { Category = localizedName, Browsable = (item.configurationVisible & Configurability.Recipe) != 0 }));
						conversion.config.customPiece.SettingChanged += (_, _) => UpdatePiece();
					}
				}

				if ((item.configurability & Configurability.Stats) != 0)
				{
					item.statsConfigs.Clear();
					void statcfg<T>(string configName, string description, Func<ItemDrop.ItemData.SharedData, T> readDefault, Action<ItemDrop.ItemData.SharedData, T> setValue)
					{
						ItemDrop.ItemData.SharedData shared = item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
						ConfigEntry<T> cfg = config(englishName, configName, readDefault(shared), new ConfigDescription(description, null, new ConfigurationManagerAttributes { Category = localizedName, Browsable = (item.configurationVisible & Configurability.Stats) != 0 }));
						if ((item.configurationVisible & Configurability.Stats) != 0)
						{
							setValue(shared, cfg.Value);
						}

						string itemName = shared.m_name;
						void ApplyConfig()
						{
							setValue(shared, cfg.Value);

							Inventory[] inventories = Player.m_players.Select(p => p.GetInventory()).Concat(UnityEngine.Object.FindObjectsOfType<Container>().Select(c => c.GetInventory())).Where(c => c is not null).ToArray();
							foreach (ItemDrop.ItemData itemdata in ObjectDB.instance.m_items.Select(p => p.GetComponent<ItemDrop>()).Where(c => c && c.GetComponent<ZNetView>()).Concat(ItemDrop.m_instances).Select(i => i.m_itemData).Concat(inventories.SelectMany(i => i.GetAllItems())))
							{
								if (itemdata.m_shared.m_name == itemName)
								{
									setValue(itemdata.m_shared, cfg.Value);
								}
							}
						}
						
						item.statsConfigs.Add(cfg, ApplyConfig);

						cfg.SettingChanged += (_, _) =>
						{
							if ((item.configurationVisible & Configurability.Stats) != 0)
							{
								ApplyConfig();
							}
						};
					}

					ItemDrop.ItemData.SharedData shared = item.Prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
					ItemDrop.ItemData.ItemType itemType = shared.m_itemType;

					statcfg("Weight", $"Weight of {englishName}.", shared => shared.m_weight, (shared, value) => shared.m_weight = value);
					statcfg("Trader Value", $"Trader value of {englishName}.", shared => shared.m_value, (shared, value) => shared.m_value = value);

					if (itemType is ItemDrop.ItemData.ItemType.Bow or ItemDrop.ItemData.ItemType.Chest or ItemDrop.ItemData.ItemType.Hands or ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shield or ItemDrop.ItemData.ItemType.Shoulder or ItemDrop.ItemData.ItemType.Tool or ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
					{
						statcfg("Durability", $"Durability of {englishName}.", shared => shared.m_maxDurability, (shared, value) => shared.m_maxDurability = value);
						statcfg("Durability per Level", $"Durability gain per level of {englishName}.", shared => shared.m_durabilityPerLevel, (shared, value) => shared.m_durabilityPerLevel = value);
						statcfg("Movement Speed Modifier", $"Movement speed modifier of {englishName}.", shared => shared.m_movementModifier, (shared, value) => shared.m_movementModifier = value);
					}

					if (itemType is ItemDrop.ItemData.ItemType.Bow or ItemDrop.ItemData.ItemType.Shield or ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
					{
						statcfg("Block Armor", $"Block armor of {englishName}.", shared => shared.m_blockPower, (shared, value) => shared.m_blockPower = value);
						statcfg("Block Armor per Level", $"Block armor per level for {englishName}.", shared => shared.m_blockPowerPerLevel, (shared, value) => shared.m_blockPowerPerLevel = value);
						statcfg("Block Force", $"Block force of {englishName}.", shared => shared.m_deflectionForce, (shared, value) => shared.m_deflectionForce = value);
						statcfg("Block Force per Level", $"Block force per level for {englishName}.", shared => shared.m_deflectionForcePerLevel, (shared, value) => shared.m_deflectionForcePerLevel = value);
						statcfg("Parry Bonus", $"Parry bonus of {englishName}.", shared => shared.m_timedBlockBonus, (shared, value) => shared.m_timedBlockBonus = value);
					}
					else if (itemType is ItemDrop.ItemData.ItemType.Chest or ItemDrop.ItemData.ItemType.Hands or ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shoulder)
					{
						statcfg("Armor", $"Armor of {englishName}.", shared => shared.m_armor, (shared, value) => shared.m_armor = value);
						statcfg("Armor per Level", $"Armor per level for {englishName}.", shared => shared.m_armorPerLevel, (shared, value) => shared.m_armorPerLevel = value);
					}

					if (shared.m_skillType is Skills.SkillType.Axes or Skills.SkillType.Pickaxes)
					{
						statcfg("Tool tier", $"Tool tier of {englishName}.", shared => shared.m_toolTier, (shared, value) => shared.m_toolTier = value);
					}

					if (itemType is ItemDrop.ItemData.ItemType.Shield or ItemDrop.ItemData.ItemType.Chest or ItemDrop.ItemData.ItemType.Hands or ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shoulder)
					{
						Dictionary<HitData.DamageType, DamageModifier> modifiers = shared.m_damageModifiers.ToDictionary(d => d.m_type, d => (DamageModifier)(int)d.m_modifier);
						foreach (HitData.DamageType damageType in ((HitData.DamageType[])Enum.GetValues(typeof(HitData.DamageType))).Except(new[] { HitData.DamageType.Chop, HitData.DamageType.Pickaxe, HitData.DamageType.Spirit, HitData.DamageType.Physical, HitData.DamageType.Elemental }))
						{
							statcfg($"{damageType.ToString()} Resistance", $"{damageType.ToString()} resistance of {englishName}.", _ => modifiers.TryGetValue(damageType, out DamageModifier modifier) ? modifier : DamageModifier.None, (shared, value) =>
							{
								HitData.DamageModPair modifier = new() { m_type = damageType, m_modifier = (HitData.DamageModifier)(int)value };
								for (int i = 0; i < shared.m_damageModifiers.Count; ++i)
								{
									if (shared.m_damageModifiers[i].m_type == damageType)
									{
										if (value == DamageModifier.None)
										{
											shared.m_damageModifiers.RemoveAt(i);
										}
										else
										{
											shared.m_damageModifiers[i] = modifier;
										}
										return;
									}
								}
								if (value != DamageModifier.None)
								{
									shared.m_damageModifiers.Add(modifier);
								}
							});
						}
					}

					if (itemType is ItemDrop.ItemData.ItemType.Consumable && shared.m_food > 0)
					{
						statcfg("Health", $"Health value of {englishName}.", shared => shared.m_food, (shared, value) => shared.m_food = value);
						statcfg("Stamina", $"Stamina value of {englishName}.", shared => shared.m_foodStamina, (shared, value) => shared.m_foodStamina = value);
						statcfg("Eitr", $"Eitr value of {englishName}.", shared => shared.m_foodEitr, (shared, value) => shared.m_foodEitr = value);
						statcfg("Duration", $"Duration of {englishName}.", shared => shared.m_foodBurnTime, (shared, value) => shared.m_foodBurnTime = value);
						statcfg("Health Regen", $"Health regen value of {englishName}.", shared => shared.m_foodRegen, (shared, value) => shared.m_foodRegen = value);
					}

					if (shared.m_skillType is Skills.SkillType.BloodMagic)
					{
						statcfg("Health Cost", $"Health cost of {englishName}.", shared => shared.m_attack.m_attackHealth, (shared, value) => shared.m_attack.m_attackHealth = value);
						statcfg("Health Cost Percentage", $"Health cost percentage of {englishName}.", shared => shared.m_attack.m_attackHealthPercentage, (shared, value) => shared.m_attack.m_attackHealthPercentage = value);
					}

					if (shared.m_skillType is Skills.SkillType.BloodMagic or Skills.SkillType.ElementalMagic)
					{
						statcfg("Eitr Cost", $"Eitr cost of {englishName}.", shared => shared.m_attack.m_attackEitr, (shared, value) => shared.m_attack.m_attackEitr = value);
					}

					if (itemType is ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft or ItemDrop.ItemData.ItemType.Bow)
					{
						statcfg("Knockback", $"Knockback of {englishName}.", shared => shared.m_attackForce, (shared, value) => shared.m_attackForce = value);
						statcfg("Backstab Bonus", $"Backstab bonus of {englishName}.", shared => shared.m_backstabBonus, (shared, value) => shared.m_backstabBonus = value);
						statcfg("Attack Stamina", $"Attack stamina of {englishName}.", shared => shared.m_attack.m_attackStamina, (shared, value) => shared.m_attack.m_attackStamina = value);

						void SetDmg(string dmgType, Func<HitData.DamageTypes, float> readDmg, setDmgFunc setDmg)
						{
							statcfg($"{dmgType} Damage", $"{dmgType} damage dealt by {englishName}.", shared => readDmg(shared.m_damages), (shared, val) => setDmg(ref shared.m_damages, val));
							statcfg($"{dmgType} Damage Per Level", $"{dmgType} damage dealt increase per level for {englishName}.", shared => readDmg(shared.m_damagesPerLevel), (shared, val) => setDmg(ref shared.m_damagesPerLevel, val));
						}

						SetDmg("True", dmg => dmg.m_damage, (ref HitData.DamageTypes dmg, float val) => dmg.m_damage = val);
						SetDmg("Slash", dmg => dmg.m_slash, (ref HitData.DamageTypes dmg, float val) => dmg.m_slash = val);
						SetDmg("Pierce", dmg => dmg.m_pierce, (ref HitData.DamageTypes dmg, float val) => dmg.m_pierce = val);
						SetDmg("Blunt", dmg => dmg.m_blunt, (ref HitData.DamageTypes dmg, float val) => dmg.m_blunt = val);
						SetDmg("Chop", dmg => dmg.m_chop, (ref HitData.DamageTypes dmg, float val) => dmg.m_chop = val);
						SetDmg("Pickaxe", dmg => dmg.m_pickaxe, (ref HitData.DamageTypes dmg, float val) => dmg.m_pickaxe = val);
						SetDmg("Fire", dmg => dmg.m_fire, (ref HitData.DamageTypes dmg, float val) => dmg.m_fire = val);
						SetDmg("Poison", dmg => dmg.m_poison, (ref HitData.DamageTypes dmg, float val) => dmg.m_poison = val);
						SetDmg("Frost", dmg => dmg.m_frost, (ref HitData.DamageTypes dmg, float val) => dmg.m_frost = val);
						SetDmg("Lightning", dmg => dmg.m_lightning, (ref HitData.DamageTypes dmg, float val) => dmg.m_lightning = val);
						SetDmg("Spirit", dmg => dmg.m_spirit, (ref HitData.DamageTypes dmg, float val) => dmg.m_spirit = val);

						if (itemType is ItemDrop.ItemData.ItemType.Bow)
						{
							statcfg("Projectiles", $"Number of projectiles that {englishName} shoots at once.", shared => shared.m_attack.m_projectileBursts, (shared, value) => shared.m_attack.m_projectileBursts = value);
							statcfg("Burst Interval", $"Time between the projectiles {englishName} shoots at once.", shared => shared.m_attack.m_burstInterval, (shared, value) => shared.m_attack.m_burstInterval = value);
							statcfg("Minimum Accuracy", $"Minimum accuracy for {englishName}.", shared => shared.m_attack.m_projectileAccuracyMin, (shared, value) => shared.m_attack.m_projectileAccuracyMin = value);
							statcfg("Accuracy", $"Accuracy for {englishName}.", shared => shared.m_attack.m_projectileAccuracy, (shared, value) => shared.m_attack.m_projectileAccuracy = value);
							statcfg("Minimum Velocity", $"Minimum velocity for {englishName}.", shared => shared.m_attack.m_projectileVelMin, (shared, value) => shared.m_attack.m_projectileVelMin = value);
							statcfg("Velocity", $"Velocity for {englishName}.", shared => shared.m_attack.m_projectileVel, (shared, value) => shared.m_attack.m_projectileVel = value);
							statcfg("Maximum Draw Time", $"Time until {englishName} is fully drawn at skill level 0.", shared => shared.m_attack.m_drawDurationMin, (shared, value) => shared.m_attack.m_drawDurationMin = value);
							statcfg("Stamina Drain", $"Stamina drain per second while drawing {englishName}.", shared => shared.m_attack.m_drawStaminaDrain, (shared, value) => shared.m_attack.m_drawStaminaDrain = value);
						}
					}
				}
			}

			if (SaveOnConfigSet)
			{
				plugin.Config.SaveOnConfigSet = true;
				plugin.Config.Save();
			}
		}

		foreach (Item item in registeredItems)
		{
			foreach (KeyValuePair<string, ItemRecipe> kv in item.Recipes)
			{
				foreach (RequiredResourceList resourceList in new[] { kv.Value.RequiredItems, kv.Value.RequiredUpgradeItems })
				{
					for (int i = 0; i < resourceList.Requirements.Count; ++i)
					{
						if ((item.configurability & Configurability.Recipe) != 0 && resourceList.Requirements[i].amountConfig is { } amountCfg)
						{
							int resourceIndex = i;
							void ConfigChanged(object o, EventArgs e)
							{
								if (ObjectDB.instance && activeRecipes.ContainsKey(item) && activeRecipes[item].TryGetValue(kv.Key, out List<Recipe> recipes))
								{
									foreach (Recipe recipe in recipes)
									{
										recipe.m_resources[resourceIndex].m_amount = amountCfg.Value;
									}
								}
							}

							amountCfg.SettingChanged += ConfigChanged;
						}
					}
				}

				if (kv.Value.RecipeIsActive is { } enabledCfg)
				{
					void ConfigChanged(object o, EventArgs e)
					{
						if (ObjectDB.instance && activeRecipes.ContainsKey(item) && activeRecipes[item].TryGetValue(kv.Key, out List<Recipe> recipes))
						{
							foreach (Recipe recipe in recipes)
							{
								recipe.m_enabled = (int)enabledCfg.BoxedValue != 0;
							}
						}
					}

					enabledCfg.GetType().GetEvent(nameof(ConfigEntry<int>.SettingChanged)).AddEventHandler(enabledCfg, new EventHandler(ConfigChanged));
				}
			}
		}
	}

	private static string getInternalName<T>(T value) where T : struct => ((InternalName)typeof(T).GetMember(value.ToString())[0].GetCustomAttributes(typeof(InternalName)).First()).internalName;

	[HarmonyPriority(Priority.Last)]
	internal static void Patch_ObjectDBInit(ObjectDB __instance)
	{
		if (__instance.GetItemPrefab("Wood") == null)
		{
			return;
		}

		hiddenCraftRecipes.Clear();
		hiddenUpgradeRecipes.Clear();

		foreach (Item item in registeredItems)
		{
			activeRecipes[item] = new Dictionary<string, List<Recipe>>();

			itemCraftConfigs.TryGetValue(item, out Dictionary<string, ItemConfig> cfgs);
			foreach (KeyValuePair<string, ItemRecipe> kv in item.Recipes)
			{
				List<Recipe> recipes = new();

				foreach (CraftingStationConfig station in kv.Value.Crafting.Stations)
				{
					ItemConfig? cfg = cfgs?[kv.Key];

					Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
					recipe.name = $"{item.Prefab.name}_Recipe_{station.Table.ToString()}";
					recipe.m_amount = item[kv.Key].CraftAmount;
					recipe.m_enabled = cfg?.table.Value != CraftingTable.Disabled;
					recipe.m_item = item.Prefab.GetComponent<ItemDrop>();
					recipe.m_resources = SerializedRequirements.toPieceReqs(__instance, cfg?.craft == null ? new SerializedRequirements(item[kv.Key].RequiredItems.Requirements) : new SerializedRequirements(cfg.craft.Value), cfg?.upgrade == null ? new SerializedRequirements(item[kv.Key].RequiredUpgradeItems.Requirements) : new SerializedRequirements(cfg.upgrade.Value));
					if ((cfg == null || recipes.Count > 0 ? station.Table : cfg.table.Value) is CraftingTable.Inventory or CraftingTable.Disabled)
					{
						recipe.m_craftingStation = null;
					}
					else if ((cfg == null || recipes.Count > 0 ? station.Table : cfg.table.Value) is CraftingTable.Custom)
					{
						if (ZNetScene.instance.GetPrefab(cfg == null || recipes.Count > 0 ? station.custom : cfg.customTable.Value) is { } craftingTable)
						{
							recipe.m_craftingStation = craftingTable.GetComponent<CraftingStation>();
						}
						else
						{
							Debug.LogWarning($"Custom crafting station '{(cfg == null || recipes.Count > 0 ? station.custom : cfg.customTable.Value)}' does not exist");
						}
					}
					else
					{
						recipe.m_craftingStation = ZNetScene.instance.GetPrefab(getInternalName(cfg == null || recipes.Count > 0 ? station.Table : cfg.table.Value)).GetComponent<CraftingStation>();
					}
					recipe.m_minStationLevel = cfg == null || recipes.Count > 0 ? station.level : cfg.tableLevel.Value;
					recipe.m_enabled = (int)(kv.Value.RecipeIsActive?.BoxedValue ?? 1) != 0;

					recipes.Add(recipe);
					if (!item[kv.Key].RequiredItems.Free && item[kv.Key].RequiredItems.Requirements.Count == 0)
					{
						hiddenCraftRecipes.Add(recipe, kv.Value.RecipeIsActive);
					}
					if (!item[kv.Key].RequiredUpgradeItems.Free && item[kv.Key].RequiredUpgradeItems.Requirements.Count == 0)
					{
						hiddenUpgradeRecipes.Add(recipe, kv.Value.RecipeIsActive);
					}
				}

				activeRecipes[item].Add(kv.Key, recipes);
				__instance.m_recipes.AddRange(recipes);
			}

			item.conversions = new List<Smelter.ItemConversion>();
			for (int i = 0; i < item.Conversions.Count; ++i)
			{
				Conversion conversion = item.Conversions[i];
				item.conversions.Add(new Smelter.ItemConversion
				{
					m_from = SerializedRequirements.fetchByName(ObjectDB.instance, conversion.config?.input.Value ?? conversion.Input),
					m_to = item.Prefab.GetComponent<ItemDrop>()
				});
				ConversionPiece piece = conversion.config?.piece.Value ?? conversion.Piece;
				string? pieceName = null;
				if (piece is not ConversionPiece.Disabled && item.conversions[i].m_from is not null)
				{
					pieceName = piece == ConversionPiece.Custom ? conversion.config?.customPiece.Value ?? conversion.customPiece : getInternalName(piece);
					if (ZNetScene.instance.GetPrefab(pieceName)?.GetComponent<Smelter>() is { } smelter)
					{
						smelter.m_conversion.Add(item.conversions[i]);
					}
					else
					{
						pieceName = null;
					}
				}
				if (conversion.config is not null)
				{
					conversion.config.activePiece = pieceName;
				}
			}
		}
	}

	internal static void Patch_OnAddSmelterInput(ItemDrop.ItemData item, bool __result)
	{
		if (__result)
		{
			Player.m_localPlayer.UnequipItem(item);
		}
	}

	internal static void Patch_MaximumRequiredStationLevel(Recipe __instance, ref int __result, int quality)
	{
		if (itemDropMap.TryGetValue(__instance.m_item, out Item item))
		{
			IEnumerable<ItemConfig> configs;
			if (!itemCraftConfigs.TryGetValue(item, out Dictionary<string, ItemConfig> itemConfigs))
			{
				configs = Enumerable.Empty<ItemConfig>();
			}
			else if (Player.m_localPlayer.GetCurrentCraftingStation() is { } currentCraftingStation)
			{
				string stationName = Utils.GetPrefabName(currentCraftingStation.gameObject);
				configs = itemConfigs.Where(c => c.Value.table.Value switch
				{
					CraftingTable.Inventory or CraftingTable.Disabled => false,
					CraftingTable.Custom => c.Value.customTable.Value == stationName,
					_ => getInternalName(c.Value.table.Value) == stationName
				}).Select(c => c.Value);
			}
			else
			{
				configs = itemConfigs.Values;
			}
			__result = Mathf.Min(Mathf.Max(1, __instance.m_minStationLevel) + (quality - 1), configs.Where(cfg => cfg.maximumTableLevel is not null).Select(cfg => cfg.maximumTableLevel!.Value).DefaultIfEmpty(item.MaximumRequiredStationLevel).Max());
		}
	}

	internal static void Patch_GetAvailableRecipesPrefix(ref Dictionary<Assembly, Dictionary<Recipe, ConfigEntryBase?>>? __state)
	{
		__state ??= new Dictionary<Assembly, Dictionary<Recipe, ConfigEntryBase?>>();
		Dictionary<Recipe, ConfigEntryBase?>? hidden;
		if (InventoryGui.instance.InCraftTab())
		{
			hidden = hiddenCraftRecipes;
		}
		else if (InventoryGui.instance.InUpradeTab())
		{
			hidden = hiddenUpgradeRecipes;
		}
		else
		{
			return;
		}

		foreach (Recipe recipe in hidden.Keys)
		{
			recipe.m_enabled = false;
		}
		__state[Assembly.GetExecutingAssembly()] = hidden;
	}

	internal static void Patch_GetAvailableRecipesFinalizer(Dictionary<Assembly, Dictionary<Recipe, ConfigEntryBase?>> __state)
	{
		if (__state.TryGetValue(Assembly.GetExecutingAssembly(), out Dictionary<Recipe, ConfigEntryBase?> hidden))
		{
			foreach (KeyValuePair<Recipe, ConfigEntryBase?> kv in hidden)
			{
				kv.Key.m_enabled = (int)(kv.Value?.BoxedValue ?? 1) != 0;
			}
		}
	}

	internal static void Patch_ZNetSceneAwake(ZNetScene __instance)
	{
		foreach (Item item in registeredItems)
		{
			item.AssignDropToCreature();
		}
	}

	public void AssignDropToCreature()
	{
		characterDrops.Clear();

		SerializedDrop drops = new(DropsFrom.Drops);
		if (itemDropConfigs.TryGetValue(this, out ConfigEntry<string> config))
		{
			drops = new SerializedDrop(config.Value);
		}
		foreach (KeyValuePair<Character, CharacterDrop.Drop> kv in drops.toCharacterDrops(ZNetScene.m_instance, Prefab))
		{
			if (kv.Key.GetComponent<CharacterDrop>() is not { } characterDrop)
			{
				characterDrop = kv.Key.gameObject.AddComponent<CharacterDrop>();
			}
			characterDrop.m_drops.Add(kv.Value);

			characterDrops.Add(characterDrop, kv.Value);
		}
	}

	public void UpdateCharacterDrop()
	{
		if (ZNetScene.instance)
		{
			foreach (KeyValuePair<CharacterDrop, CharacterDrop.Drop> kv in characterDrops)
			{
				if (kv.Key)
				{
					kv.Key.m_drops.Remove(kv.Value);
				}
			}

			AssignDropToCreature();
		}
	}

	public void Snapshot(float lightIntensity = 1.3f, Quaternion? cameraRotation = null) => SnapshotItem(Prefab.GetComponent<ItemDrop>(), lightIntensity, cameraRotation);

	public static void SnapshotItem(ItemDrop item, float lightIntensity = 1.3f, Quaternion? cameraRotation = null)
	{
		const int layer = 30;

		Camera camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
		camera.backgroundColor = Color.clear;
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.fieldOfView = 0.5f;
		camera.farClipPlane = 10000000;
		camera.cullingMask = 1 << layer;
		camera.transform.rotation = cameraRotation ?? Quaternion.Euler(90, 0, 45);

		Light topLight = new GameObject("Light", typeof(Light)).GetComponent<Light>();
		topLight.transform.rotation = Quaternion.Euler(150, 0, -5f);
		topLight.type = LightType.Directional;
		topLight.cullingMask = 1 << layer;
		topLight.intensity = lightIntensity;

		Rect rect = new(0, 0, 64, 64);

		GameObject visual = UnityEngine.Object.Instantiate(item.transform.Find("attach").gameObject);
		foreach (Transform child in visual.GetComponentsInChildren<Transform>())
		{
			child.gameObject.layer = layer;
		}

		Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
		Vector3 min = renderers.Aggregate(Vector3.positiveInfinity, (cur, renderer) => renderer is ParticleSystemRenderer ? cur : Vector3.Min(cur, renderer.bounds.min));
		Vector3 max = renderers.Aggregate(Vector3.negativeInfinity, (cur, renderer) => renderer is ParticleSystemRenderer ? cur : Vector3.Max(cur, renderer.bounds.max));
		Vector3 size = max - min;

		camera.targetTexture = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
		float maxDim = Mathf.Max(size.x, size.z);
		float minDim = Mathf.Min(size.x, size.z);
		float yDist = (maxDim + minDim) / Mathf.Sqrt(2) / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad);
		Transform transform = camera.transform;
		transform.position = ((min + max) / 2) with { y = max.y } + new Vector3(0, yDist, 0);
		topLight.transform.position = transform.position + new Vector3(-2, 0, 0.2f) / 3 * -yDist;

		camera.Render();

		RenderTexture currentRenderTexture = RenderTexture.active;
		RenderTexture.active = camera.targetTexture;

		Texture2D texture = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
		texture.ReadPixels(rect, 0, 0);
		texture.Apply();

		RenderTexture.active = currentRenderTexture;

		item.m_itemData.m_shared.m_icons = new[] { Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f)) };

		UnityEngine.Object.DestroyImmediate(visual);
		camera.targetTexture.Release();

		UnityEngine.Object.Destroy(camera);
		UnityEngine.Object.Destroy(topLight);
	}

	private static bool CheckItemIsUpgrade(InventoryGui gui) => gui.m_selectedRecipe.Value?.m_quality > 0;

	internal static IEnumerable<CodeInstruction> Transpile_InventoryGui(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> instrs = instructions.ToList();
		FieldInfo amountField = AccessTools.DeclaredField(typeof(Recipe), nameof(Recipe.m_amount));
		for (int i = 0; i < instrs.Count; ++i)
		{
			yield return instrs[i];
			if (i > 1 && instrs[i - 2].opcode == OpCodes.Ldfld && instrs[i - 2].OperandIs(amountField) && instrs[i - 1].opcode == OpCodes.Ldc_I4_1 && instrs[i].operand is Label)
			{
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Item), nameof(CheckItemIsUpgrade)));
				yield return new CodeInstruction(OpCodes.Brtrue, instrs[i].operand);
			}
		}
	}

	private static void drawRequirementsConfigTable(ConfigEntryBase cfg)
	{
		bool locked = cfg.Description.Tags.Select(a => a.GetType().Name == "ConfigurationManagerAttributes" ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a) : null).FirstOrDefault(v => v != null) ?? false;

		List<Requirement> newReqs = new();
		bool wasUpdated = false;

		int RightColumnWidth = (int)(configManager?.GetType().GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic)!.GetGetMethod(true).Invoke(configManager, Array.Empty<object>()) ?? 130);

		GUILayout.BeginVertical();
		foreach (Requirement req in new SerializedRequirements((string)cfg.BoxedValue).Reqs)
		{
			GUILayout.BeginHorizontal();

			int amount = req.amount;
			if (int.TryParse(GUILayout.TextField(amount.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 40 }), out int newAmount) && newAmount != amount && !locked)
			{
				amount = newAmount;
				wasUpdated = true;
			}

			string newItemName = GUILayout.TextField(req.itemName, new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 40 - 21 - 21 - 9 });
			string itemName = locked ? req.itemName : newItemName;
			wasUpdated = wasUpdated || itemName != req.itemName;

			if (GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
			{
				wasUpdated = true;
			}
			else
			{
				newReqs.Add(new Requirement { amount = amount, itemName = itemName });
			}

			if (GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 }) && !locked)
			{
				wasUpdated = true;
				newReqs.Add(new Requirement { amount = 1, itemName = "" });
			}

			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if (wasUpdated)
		{
			cfg.BoxedValue = new SerializedRequirements(newReqs).ToString();
		}
	}

	private static void drawDropsConfigTable(ConfigEntryBase cfg)
	{
		bool locked = cfg.Description.Tags.Select(a => a.GetType().Name == "ConfigurationManagerAttributes" ? (bool?)a.GetType().GetField("ReadOnly")?.GetValue(a) : null).FirstOrDefault(v => v != null) ?? false;

		List<DropTarget> newDrops = new();
		bool wasUpdated = false;

		int RightColumnWidth = (int)(configManager?.GetType().GetProperty("RightColumnWidth", BindingFlags.Instance | BindingFlags.NonPublic)!.GetGetMethod(true).Invoke(configManager, Array.Empty<object>()) ?? 130);

		GUILayout.BeginVertical();
		foreach (DropTarget drop in new SerializedDrop((string)cfg.BoxedValue).Drops.DefaultIfEmpty(new DropTarget { min = 1, max = 1, creature = "", chance = 1 }))
		{
			GUILayout.BeginHorizontal();

			string newCreature = GUILayout.TextField(drop.creature, new GUIStyle(GUI.skin.textField) { fixedWidth = RightColumnWidth - 21 - 21 - 6 });
			string creature = locked ? drop.creature : newCreature;
			wasUpdated = wasUpdated || creature != drop.creature;

			bool wasDeleted = GUILayout.Button("x", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });
			bool wasAdded = GUILayout.Button("+", new GUIStyle(GUI.skin.button) { fixedWidth = 21 });

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			GUILayout.Label("Chance: ");
			float chance = drop.chance;
			if (float.TryParse(GUILayout.TextField((chance * 100).ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField) { fixedWidth = 45 }), out float newChance) && !Mathf.Approximately(newChance / 100, chance) && !locked)
			{
				chance = newChance;
				wasUpdated = true;
			}
			GUILayout.Label("% Amount: ");

			int min = drop.min;
			if (int.TryParse(GUILayout.TextField(min.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 35 }), out int newMin) && newMin != min && !locked)
			{
				min = newMin;
				wasUpdated = true;
			}

			GUILayout.Label(" - ");

			int max = drop.max;
			if (int.TryParse(GUILayout.TextField(max.ToString(), new GUIStyle(GUI.skin.textField) { fixedWidth = 35 }), out int newMax) && newMax != max && !locked)
			{
				max = newMax;
				wasUpdated = true;
			}

			if (wasDeleted && !locked)
			{
				wasUpdated = true;
			}
			else
			{
				newDrops.Add(new DropTarget { creature = creature, min = min, max = max, chance = chance });
			}

			if (wasAdded && !locked)
			{
				wasUpdated = true;
				newDrops.Add(new DropTarget { min = 1, max = 1, creature = "", chance = 1 });
			}

			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();

		if (wasUpdated)
		{
			cfg.BoxedValue = new SerializedDrop(newDrops).ToString();
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
				return new Requirement { itemName = parts[0], amount = parts.Length > 1 && int.TryParse(parts[1], out int amount) ? amount : 1 };
			}).ToList();
		}

		public override string ToString()
		{
			return string.Join(",", Reqs.Select(r => $"{r.itemName}:{r.amount}"));
		}

		public static ItemDrop? fetchByName(ObjectDB objectDB, string name)
		{
			ItemDrop? item = objectDB.GetItemPrefab(name)?.GetComponent<ItemDrop>();
			if (item == null)
			{
				Debug.LogWarning($"The required item '{name}' does not exist.");
			}
			return item;
		}

		public static Piece.Requirement[] toPieceReqs(ObjectDB objectDB, SerializedRequirements craft, SerializedRequirements upgrade)
		{
			ItemDrop? ResItem(Requirement r) => fetchByName(objectDB, r.itemName);

			Dictionary<string, Piece.Requirement?> resources = craft.Reqs.Where(r => r.itemName != "").ToDictionary(r => r.itemName, r => ResItem(r) is { } item ? new Piece.Requirement { m_amount = r.amountConfig?.Value ?? r.amount, m_resItem = item, m_amountPerLevel = 0 } : null);
			foreach (Requirement req in upgrade.Reqs.Where(r => r.itemName != ""))
			{
				if ((!resources.TryGetValue(req.itemName, out Piece.Requirement? requirement) || requirement == null) && ResItem(req) is { } item)
				{
					requirement = resources[req.itemName] = new Piece.Requirement { m_resItem = item, m_amount = 0 };
				}

				if (requirement != null)
				{
					requirement.m_amountPerLevel = req.amountConfig?.Value ?? req.amount;
				}
			}

			return resources.Values.Where(v => v != null).ToArray()!;
		}
	}

	private class SerializedDrop
	{
		public readonly List<DropTarget> Drops;

		public SerializedDrop(List<DropTarget> drops) => Drops = drops;

		public SerializedDrop(string drops)
		{
			Drops = (drops == "" ? Array.Empty<string>() : drops.Split(',')).Select(r =>
			{
				string[] parts = r.Split(':');
				if (parts.Length <= 2 || !int.TryParse(parts[2], out int min))
				{
					min = 1;
				}
				if (parts.Length <= 3 || !int.TryParse(parts[3], out int max))
				{
					max = min;
				}
				return new DropTarget { creature = parts[0], chance = parts.Length > 1 && float.TryParse(parts[1], out float chance) ? chance : 1, min = min, max = max };
			}).ToList();
		}

		public override string ToString()
		{
			return string.Join(",", Drops.Select(r => $"{r.creature}:{r.chance.ToString(CultureInfo.InvariantCulture)}:{r.min}" + (r.min == r.max ? "" : $":{r.max}")));
		}

		private static Character? fetchByName(ZNetScene netScene, string name)
		{
			Character? character = netScene.GetPrefab(name)?.GetComponent<Character>();
			if (character == null)
			{
				Debug.LogWarning($"The drop target character '{name}' does not exist.");
			}
			return character;
		}

		public Dictionary<Character, CharacterDrop.Drop> toCharacterDrops(ZNetScene netScene, GameObject item)
		{
			Dictionary<Character, CharacterDrop.Drop> drops = new();
			foreach (DropTarget drop in Drops)
			{
				if (fetchByName(netScene, drop.creature) is { } character)
				{
					drops[character] = new CharacterDrop.Drop { m_prefab = item, m_amountMin = drop.min, m_amountMax = drop.max, m_chance = drop.chance };
				}
			}

			return drops;
		}
	}

	private static Localization? _english;
	private static Localization english => _english ??= LocalizationCache.ForLanguage("English");

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
					_configSync = Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " ItemManager");
					configSyncType.GetField("CurrentVersion").SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
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

		configSync?.GetType().GetMethod("AddConfigEntry")!.MakeGenericMethod(typeof(T)).Invoke(configSync, new object[] { configEntry });

		return configEntry;
	}

	private static ConfigEntry<T> config<T>(string group, string name, T value, string description) => config(group, name, value, new ConfigDescription(description));
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

[PublicAPI]
public static class PrefabManager
{
	static PrefabManager()
	{
		Harmony harmony = new("org.bepinex.helpers.ItemManager");
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ObjectDBInit))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ObjectDBInit))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_ObjectDBInit))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_ObjectDBInit))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_FejdStartup))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Patch_ZNetSceneAwake))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ZNetSceneAwake))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe)), transpiler: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Transpile_InventoryGui))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Player), nameof(Player.GetAvailableRecipes)), prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_GetAvailableRecipesPrefix))), finalizer: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_GetAvailableRecipesFinalizer))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Recipe), nameof(Recipe.GetRequiredStationLevel)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_MaximumRequiredStationLevel))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Smelter), nameof(Smelter.OnAddFuel)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_OnAddSmelterInput))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Smelter), nameof(Smelter.OnAddOre)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.Patch_OnAddSmelterInput))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.SetupLanguage)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocalizationCache), nameof(LocalizationCache.LocalizationPostfix))));
		harmony.Patch(AccessTools.DeclaredMethod(typeof(Localization), nameof(Localization.LoadCSV)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LocalizeKey), nameof(LocalizeKey.AddLocalizedKeys))));
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
			assets = bundleCache[id] = Resources.FindObjectsOfTypeAll<AssetBundle>().FirstOrDefault(a => a.name == assetBundleFileName) ?? AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + $".{folderName}." + assetBundleFileName));
		}

		return assets;
	}

	private static readonly List<GameObject> prefabs = new();
	private static readonly List<GameObject> ZnetOnlyPrefabs = new();

	public static GameObject RegisterPrefab(string assetBundleFileName, string prefabName, string folderName = "assets") => RegisterPrefab(RegisterAssetBundle(assetBundleFileName, folderName), prefabName);

	public static GameObject RegisterPrefab(AssetBundle assets, string prefabName, bool addToObjectDb = false) => RegisterPrefab(assets.LoadAsset<GameObject>(prefabName), addToObjectDb);

	public static GameObject RegisterPrefab(GameObject prefab, bool addToObjectDb = false)
	{
		if (addToObjectDb)
		{
			prefabs.Add(prefab);
		}
		else
		{
			ZnetOnlyPrefabs.Add(prefab);
		}

		return prefab;
	}

	[HarmonyPriority(Priority.VeryHigh)]
	private static void Patch_ObjectDBInit(ObjectDB __instance)
	{
		foreach (GameObject prefab in prefabs)
		{
			if (!__instance.m_items.Contains(prefab))
			{
				__instance.m_items.Add(prefab);
			}

			void RegisterStatusEffect(StatusEffect? statusEffect)
			{
				if (statusEffect is not null && !__instance.GetStatusEffect(statusEffect.name))
				{
					__instance.m_StatusEffects.Add(statusEffect);
				}
			}

			ItemDrop.ItemData.SharedData shared = prefab.GetComponent<ItemDrop>().m_itemData.m_shared;
			RegisterStatusEffect(shared.m_attackStatusEffect);
			RegisterStatusEffect(shared.m_consumeStatusEffect);
			RegisterStatusEffect(shared.m_equipStatusEffect);
			RegisterStatusEffect(shared.m_setStatusEffect);
		}

		__instance.UpdateItemHashes();
	}

	[HarmonyPriority(Priority.VeryHigh)]
	private static void Patch_ZNetSceneAwake(ZNetScene __instance)
	{
		foreach (GameObject prefab in prefabs.Concat(ZnetOnlyPrefabs))
		{
			__instance.m_prefabs.Add(prefab);
		}
	}
}

[PublicAPI]
public class Conversion
{
	internal class ConversionConfig
	{
		public ConfigEntry<string> input = null!;
		public string? activePiece;
		public ConfigEntry<ConversionPiece> piece = null!;
		public ConfigEntry<string> customPiece = null!;
	}

	public string Input = null!;
	public ConversionPiece Piece;
	internal string? customPiece = null;

	public string? Custom
	{
		get => customPiece;
		set
		{
			customPiece = value;
			Piece = ConversionPiece.Custom;
		}
	}

	internal ConversionConfig? config;

	public Conversion(Item outputItem)
	{
		outputItem.Conversions.Add(this);
	}
}