using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdventureBackpacks.API;

/// <summary>
/// This is a Flags enum for determining Backpack Biomes. This is not representative of Heightmap.Biomes.
/// This is instead a way of creating sets of Effects. Default Adventure Backpacks will utilize Biome names.
/// Custom External Effects can pick and choose between the Custom Effect flags as well.
/// </summary>
[Flags]
public enum BackpackBiomes : uint
{
    /// <summary>
    /// None, no backpack effects are applied.
    /// </summary>
    None = 0,
    /// <summary>
    /// Meadows Backpack Effects
    /// </summary>
    Meadows = 1 << 0,
    /// <summary>
    /// Black Forest Backpack Effects
    /// </summary>
    BlackForest = 1 << 1,
    /// <summary>
    /// Swamp Backpack Effects
    /// </summary>
    Swamp = 1 << 2,
    /// <summary>
    /// Mountains Backpack Effects
    /// </summary>
    Mountains = 1 << 3,
    /// <summary>
    /// Plains Backpack Effects
    /// </summary>
    Plains = 1 << 4,
    /// <summary>
    /// Mistlands Backpack Effects
    /// </summary>
    Mistlands = 1 << 5,
    /// <summary>
    /// Ashland Backpack Effects
    /// </summary>
    Ashlands = 1 << 6,
    /// <summary>
    /// DeepNorth Backpack Effects
    /// </summary>
    DeepNorth = 1 << 7,
    /// <summary>
    /// Custom Biome for use with External Effects
    /// </summary>
    EffectBiome1 = 1 << 20,
    /// <summary>
    /// Custom Biome for use with External Effects
    /// </summary>
    EffectBiome2 = 1 << 21,
    /// <summary>
    /// Custom Biome for use with External Effects
    /// </summary>
    EffectBiome3 = 1 << 22,
    /// <summary>
    /// Custom Biome for use with External Effects
    /// </summary>
    EffectBiome4 = 1 << 23,
    /// <summary>
    /// Custom Biome for use with External Effects
    /// </summary>
    EffectBiome5 = 1 << 24,
}

// ReSharper disable once InconsistentNaming
public partial class ABAPI
{
    /// <summary>
    /// Instanced Backpack Information
    /// </summary>
    public struct Backpack
    {
        /// <summary>
        /// Backpack Name
        /// </summary>
        public string Name;
        /// <summary>
        /// ItemData representative of the Backpack
        /// </summary>
        public ItemDrop.ItemData ItemData;
        /// <summary>
        /// Inventory object of the configured backpack
        /// </summary>
        public Inventory Inventory;
        /// <summary>
        /// Definition of the Backpack item
        /// </summary>
        public BackpackDefinition Definition;
    }

    /// <summary>
    /// Backpack Definition Settings
    /// Use this object to create backpack definitions for adding Backpacks to Adventure Backpacks.
    /// You can use either the GameObject directly, or provide your AssetBundle object and the PrefabName.
    /// </summary>
    public class BackpackDefinition
    {
        /// <summary>
        /// Backpack GameObject
        /// (Not required if AssetBundle and Prefab Name are provided)
        /// </summary>
        public GameObject BackPackGo;
        /// <summary>
        /// Asset Bundle containing the Backpack
        /// (Not required if GameObject is provided)
        /// </summary>
        public AssetBundle AssetBundle;
        /// <summary>
        /// Prefab Name of the Backpack Asset
        /// (Not required if GameObject is provided)
        /// </summary>
        public string PrefabName;
        /// <summary>
        /// Item Name of the Backpack. Use the $_name localize token.
        /// (e.g. $vapok_mod_rugged_backpack)
        /// </summary>
        public string ItemName;
        /// <summary>
        /// Custom Configuration Section Name in Adventure Backpacks.
        /// If left empty, will use default section name.
        /// (e.g. Backpack: {$backpack_itemname} )
        /// </summary>
        public string ConfigSection = "";
        /// <summary>
        /// Name of Crafting Table that the Backpack can be crafted at.
        /// If left empty, will disable crafting at any table.
        /// Can also include custom table names.
        /// (e.g. piece_workbench, forge, piece_stonecutter)
        /// </summary>
        public string CraftingTable;
        /// <summary>
        /// Minimum Level of Crafting Table Station before Bag can be crafted
        /// (e.g. 1, 2, etc.)
        /// </summary>
        public int StationLevel;
        /// <summary>
        /// Max Crafting Station Level to upgrade, and repair.
        /// (e.g. 1, 2, etc.)
        /// </summary>
        public int MaxRequiredStationLevel;
        /// <summary>
        /// List of Recipe Ingredients.
        /// </summary>
        public readonly List<RecipeIngredient> RecipeIngredients = new ();
        /// <summary>
        /// List of Ingredients for Upgrading.
        /// </summary>
        public readonly List<RecipeIngredient> UpgradeIngredients = new ();
        /// <summary>
        /// List of Ingredients for Upgrading.
        /// </summary>
        public readonly List<DropTarget> DropsFrom = new ();
        /// <summary>
        /// Dictionary of StatusEffect's to apply to the backpack when equipped.
        /// Dictionary Key is the BackpackBiome that needs to be applied to the Backpack for Effect to be activated.
        /// Dictionary Value is a Key Value Pair of a Status Effect to apply:
        /// Key of the KVP is the actual Status Effect
        /// Value of the KVP is the default int Quality level that of the backpack before the effect is applied.
        /// (examples of the quality level: 1 - 4)
        /// Use GetRegisteredStatusEffects() to determine which status effect Adventure Backpacks is aware of.
        /// Use RegisterEffect() first to register new status effects.
        /// </summary>
        public readonly Dictionary<BackpackBiomes,KeyValuePair<StatusEffect,int>> EffectsToApply = new ();
        /// <summary>
        /// Dictionary of Vector2's that contain the x and y sizing of the backpack at each Quality level'
        /// Dictionary key is the Item's Quality level.
        /// Dictionary value is the Vector2 object.
        /// </summary>
        public Dictionary<int,Vector2> BackpackSizeByQuality = new ();
        /// <summary>
        /// When backpack is equipped, set what the Set Effect Status Effect Should.
        /// If left empty, none will be equipped.
        /// Note: Be sure this is one of the effects included in EffectsToApply.
        /// </summary>
        public StatusEffect ItemSetStatusEffect;
        /// <summary>
        /// Provides the configured weight multiplier that reduces the weight of the items in the backpack.
        /// For registering a new backpack, this is the default value.
        /// </summary>
        public float WeightMultiplier;
        /// <summary>
        /// Provides the additional carry weight bonus applied to backpacks.
        /// For registering a new backpack, this is the default value.
        /// </summary>
        public int CarryBonus;
        /// <summary>
        /// Provides the Speed Modification that is applied on the backpack.
        /// For registering a new backpack, this is the default value.
        /// </summary>
        public float SpeedMod;
        /// <summary>
        /// Provides whether the wearer of the backpack will freeze or not.
        /// </summary>
        public bool EnableFreezing;
        /// <summary>
        /// Provides the configured biomes settings applied to the backpack.
        /// This is flag enum.
        /// (e.g. "BackpackBiomes.Meadows" or "BackpackBiomes.Meadows | BackpackBiomes.BlackForest" to select multiple biomes.
        /// </summary>
        public BackpackBiomes BackpackBiome;
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BackpackDefinition()
        {

        }

        /// <summary>
        /// Use this constructor when adding a backpack using the GameObject
        /// The item should have ItemDrop.ItemData on the item, and it should be an item that is utilizing the Shoulder slot.
        /// Equipped Detection won't detect if not in the shoulder slot.
        /// TODO: Make this more flexible for additional slots through AzuEPI
        /// </summary>
        /// <param name="backPackGo">GameObject of </param>
        public BackpackDefinition(GameObject backPackGo)
        {
            BackPackGo = backPackGo;
        }

        /// <summary>
        /// Use this constructor when adding a backpack using the AssetBundle and Prefab Name
        /// The item should have ItemDrop.ItemData on the item, and it should be an item that is utilizing the Shoulder slot.
        /// Equipped Detection won't detect if not in the shoulder slot.
        /// TODO: Make this more flexible for additional slots through AzuEPI
        /// </summary>
        /// <param name="assetBundle">Provide the Asset Bundle that contains the backpack prefab</param>
        /// <param name="prefabName">Prefab name of the backpack</param>
        public BackpackDefinition(AssetBundle assetBundle, string prefabName)
        {
            AssetBundle = assetBundle;
            PrefabName = prefabName;
        }

    }
    
    /// <summary>
    /// Configuration of Drop Target
    /// </summary>
    public struct DropTarget
    {
        /// <summary>
        /// Prefab name of creature
        /// </summary>
        public string Creature;
        /// <summary>
        /// Min number of items that can drop.
        /// </summary>
        public int Min;
        /// <summary>
        /// Maximum number of items that can drop.
        /// </summary>
        public int? Max;
        /// <summary>
        /// Configured Drop Chance
        /// </summary>
        public float Chance;

        /// <summary>
        /// Drop Target Constructor
        /// </summary>
        /// <param name="creature">Prefab name of Creature</param>
        /// <param name="chance">Chance to Drop Float</param>
        /// <param name="min">Minimum amount to drop.</param>
        /// <param name="max">Max amount to drop.</param>
        public DropTarget(string creature, float chance, int min = 1, int? max = null)
        {
            Creature = creature;
            Chance = chance;
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Create anew EffectDefinition in order to register status effects.
    /// </summary>
    public struct EffectDefinition
    {
        /// <summary>
        /// This is the Effect Name.
        /// (e.g. "$some_effect_name").
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// This is the Localized Translated Effect Name.
        /// This is used in places like the Configuration and in the HUD/GUI
        /// (e.g. "Water Resistance").
        /// </summary>
        public readonly string LocalizedName;
        /// <summary>
        /// This is the registered Effect name
        /// (e.g. SetEffect_NecromancyArmor or SE_Demister)
        /// </summary>
        public readonly string EffectName;
        /// <summary>
        /// Description of the Status Effect. Shows up in Configuration.
        /// </summary>
        public readonly string Description;
        /// <summary>
        /// This is your actual Status Effect from your own asset bundle or from another source.
        /// As long as it's of the type SE_Stats, you can use it.
        /// </summary>
        public readonly StatusEffect StatusEffect;

        /// <summary>
        /// Create anew EffectDefinition in order to register status effects.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="localizedName"></param>
        /// <param name="effectName"></param>
        /// <param name="description"></param>
        /// <param name="statusEffect"></param>
        public EffectDefinition(string name, string localizedName, string effectName, string description, StatusEffect statusEffect)
        {
            Name = name;
            LocalizedName = localizedName;
            EffectName = effectName;
            Description = description;
            StatusEffect = statusEffect;
        }
    }
    /// <summary>
    /// Defines a Recipe Ingredient
    /// </summary>
    public struct RecipeIngredient
    {
        /// <summary>
        /// Prefab Name of Item to include as a recipe ingredient
        /// </summary>
        public string ItemPrefabName;
        /// <summary>
        /// Amount of Item required.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Create Ingredient Object
        /// </summary>
        /// <param name="itemPrefabName">Item Prefab Name</param>
        /// <param name="quantity">Amount of item to consume.</param>
        public RecipeIngredient(string itemPrefabName, int quantity)
        {
            ItemPrefabName = itemPrefabName;
            Quantity = quantity;
        }
    }
}