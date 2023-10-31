using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdventureBackpacks.API;

/// <summary>
/// This is a Flags enum for determining Backpack Biomes. This is not representative of Heightmap.Biomes.
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
    /// Special Biome configured for Cheb's Necromancy
    /// </summary>
    Necromancy = 1 << 6
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
    /// </summary>
    public struct BackpackDefinition
    {
        /// <summary>
        /// Asset Bundle Name
        /// </summary>
        public string AssetName;
        /// <summary>
        /// Folder containing the Asset Bundle
        /// </summary>
        public string AssetFolderName;
        /// <summary>
        /// Prefab Name of the Backpack Asset
        /// </summary>
        public string PrefabName;
        /// <summary>
        /// Item Name of the Backpack. Use the $_name localize token.
        /// </summary>
        public string ItemName;
        /// <summary>
        /// Dictionary of Vector2's that contain the x and y sizing of the backpack at each Quality level'
        /// Dictionary key is the Item's Quality level.
        /// Dictionary value is the Vector2 object.
        /// </summary>
        public Dictionary<int,Vector2> BackpackSizeByQuality;
        /// <summary>
        /// Provides the configured weight multiplier that reduces the weight of the items in the backpack.
        /// </summary>
        public float WeightMultiplier;
        /// <summary>
        /// Provides the additional carry weight bonus applied to backpacks.
        /// </summary>
        public int CarryBonus;
        /// <summary>
        /// Provides the Speed Modification that is applied on the backpack.
        /// </summary>
        public float SpeedMod;
        /// <summary>
        /// Provides whether the wearer of the backpack will freeze or not.
        /// </summary>
        public bool EnableFreezing;
        /// <summary>
        /// Provides the configured biomes settings applied to the backpack.
        /// </summary>
        public BackpackBiomes BackpackBiome;

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
        public int Max;
        /// <summary>
        /// Configured Drop Chance
        /// </summary>
        public float Chance;
    }    
}