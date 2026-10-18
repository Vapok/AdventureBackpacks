using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AdventureBackpacks.API.Client
{
    /// <summary>
    /// Lightweight, zero-dependency client wrapper for AdventureBackpacks API.
    /// Drop this file into your mod project to interact with AdventureBackpacks without adding assembly references or repacking DLLs.
    /// </summary>
    public static class ABAPIClient
    {
        private static bool _initialized;
        private static bool _isAvailable;
        private static Type _apiType;

        private static MethodInfo _isLoadedMethod;
        private static MethodInfo _isBackpackMethod;
        private static MethodInfo _isBackpackEquippedMethod;
        private static MethodInfo _canOpenBackpackMethod;
        private static MethodInfo _isBackpackOpenMethod;
        private static MethodInfo _isThisBackpackEquippedMethod;
        private static MethodInfo _getEquippedBackpackInventoryMethod;
        private static MethodInfo _tryGetEquippedBackpackInventoryMethod;
        private static MethodInfo _getBackpackInventoryMethod;
        private static MethodInfo _tryGetBackpackInventoryMethod;
        private static MethodInfo _getAllBackpackInventoriesMethod;
        private static MethodInfo _tryGetAllBackpackInventoriesMethod;
        private static MethodInfo _openBackpackMethod;
        private static MethodInfo _openBackpackWithGroupMethod;
        private static MethodInfo _isOpenWithCraftingStationEnabledMethod;
        private static MethodInfo _isOpenWithInventoryEnabledMethod;
        private static MethodInfo _isEnableContainerTabsEnabledMethod;
        private static MethodInfo _hasActiveExternalContainerMethod;
        private static MethodInfo _isCraftFromBackpackEnabledMethod;
        private static MethodInfo _isCraftingContextActiveMethod;
        private static MethodInfo _getConsumptionPriorityMethod;
        private static MethodInfo _isLeaveOneItemInBackpackEnabledMethod;
        private static MethodInfo _isDisplayTotalIngredientCountEnabledMethod;
        private static MethodInfo _suppressNativeCraftingMethod;
        private static MethodInfo _unsuppressNativeCraftingMethod;
        private static MethodInfo _wouldStorePickupMethod;
        private static MethodInfo _wouldStorePickupWithInventoryMethod;
        private static MethodInfo _registerVisualBundleMethod;
        private static MethodInfo _registerVisualBundleWithDefaultMethod;
        private static Type _modelStyleType;
        private static MethodInfo _registerVisualBundleStyleMethod;
        private static MethodInfo _registerVisualBundleStyleWithDefaultMethod;

        /// <summary>
        /// Defines the visual model geometry style for a backpack.
        /// </summary>
        public enum ModelStyle
        {
            Modern = 0,
            Classic = 1
        }

        /// <summary>
        /// Identifies the progression backpacks in Adventure Backpacks.
        /// </summary>
        public enum BackpackName
        {
            /// <summary>Meadows tier backpack.</summary>
            Meadows,
            /// <summary>Black Forest tier backpack.</summary>
            BlackForest,
            /// <summary>Swamp tier backpack.</summary>
            Swamp,
            /// <summary>Mountains tier backpack.</summary>
            Mountains,
            /// <summary>Plains tier backpack.</summary>
            Plains,
            /// <summary>Mistlands tier backpack.</summary>
            Mistlands,
            /// <summary>Ashlands tier backpack.</summary>
            Ashlands,
            /// <summary>Deep North tier backpack.</summary>
            DeepNorth
        }

        public static bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _isAvailable;
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (assemblies[i].GetName().Name == "AdventureBackpacks")
                {
                    _apiType = assemblies[i].GetType("AdventureBackpacks.API.ABAPI");
                    if (_apiType != null)
                    {
                        _isAvailable = true;
                        _isLoadedMethod = _apiType.GetMethod("IsLoaded", BindingFlags.Public | BindingFlags.Static);
                        _isBackpackMethod = _apiType.GetMethod("IsBackpack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ItemDrop.ItemData) }, null);
                        _isBackpackEquippedMethod = _apiType.GetMethod("IsBackpackEquipped", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _canOpenBackpackMethod = _apiType.GetMethod("CanOpenBackpack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _isBackpackOpenMethod = _apiType.GetMethod("IsBackpackOpen", BindingFlags.Public | BindingFlags.Static);
                        _isThisBackpackEquippedMethod = _apiType.GetMethod("IsThisBackpackEquipped", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(ItemDrop.ItemData) }, null);
                        _getEquippedBackpackInventoryMethod = _apiType.GetMethod("GetEquippedBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _tryGetEquippedBackpackInventoryMethod = _apiType.GetMethod("TryGetEquippedBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(Inventory).MakeByRefType() }, null);
                        _getBackpackInventoryMethod = _apiType.GetMethod("GetBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ItemDrop.ItemData) }, null);
                        _tryGetBackpackInventoryMethod = _apiType.GetMethod("TryGetBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ItemDrop.ItemData), typeof(Inventory).MakeByRefType() }, null);
                        _getAllBackpackInventoriesMethod = _apiType.GetMethod("GetAllBackpackInventories", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _tryGetAllBackpackInventoriesMethod = _apiType.GetMethod("TryGetAllBackpackInventories", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(List<Inventory>).MakeByRefType() }, null);
                        _openBackpackMethod = _apiType.GetMethod("OpenBackpack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(InventoryGui) }, null);
                        _openBackpackWithGroupMethod = _apiType.GetMethod("OpenBackpack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(InventoryGui), typeof(int) }, null);
                        _isOpenWithCraftingStationEnabledMethod = _apiType.GetMethod("IsOpenWithCraftingStationEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isOpenWithInventoryEnabledMethod = _apiType.GetMethod("IsOpenWithInventoryEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isEnableContainerTabsEnabledMethod = _apiType.GetMethod("IsEnableContainerTabsEnabled", BindingFlags.Public | BindingFlags.Static);
                        _hasActiveExternalContainerMethod = _apiType.GetMethod("HasActiveExternalContainer", BindingFlags.Public | BindingFlags.Static);
                        _isCraftFromBackpackEnabledMethod = _apiType.GetMethod("IsCraftFromBackpackEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isCraftingContextActiveMethod = _apiType.GetMethod("IsCraftingContextActive", BindingFlags.Public | BindingFlags.Static);
                        _getConsumptionPriorityMethod = _apiType.GetMethod("GetConsumptionPriority", BindingFlags.Public | BindingFlags.Static);
                        _isLeaveOneItemInBackpackEnabledMethod = _apiType.GetMethod("IsLeaveOneItemInBackpackEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isDisplayTotalIngredientCountEnabledMethod = _apiType.GetMethod("IsDisplayTotalIngredientCountEnabled", BindingFlags.Public | BindingFlags.Static);
                        _suppressNativeCraftingMethod = _apiType.GetMethod("SuppressNativeCrafting", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                        _unsuppressNativeCraftingMethod = _apiType.GetMethod("UnsuppressNativeCrafting", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                        _wouldStorePickupMethod = _apiType.GetMethod("WouldStorePickup", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(ItemDrop.ItemData) }, null);
                        _wouldStorePickupWithInventoryMethod = _apiType.GetMethod("WouldStorePickup", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(ItemDrop.ItemData), typeof(Inventory).MakeByRefType() }, null);
                        _registerVisualBundleMethod = _apiType.GetMethod("RegisterVisualBundle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string), typeof(AssetBundle) }, null);
                        _registerVisualBundleWithDefaultMethod = _apiType.GetMethod("RegisterVisualBundle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string), typeof(AssetBundle), typeof(bool) }, null);

                        _modelStyleType = assemblies[i].GetType("AdventureBackpacks.API.ModelStyle");
                        if (_modelStyleType != null)
                        {
                            _registerVisualBundleStyleMethod = _apiType.GetMethod("RegisterVisualBundle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string), _modelStyleType, typeof(AssetBundle) }, null);
                            _registerVisualBundleStyleWithDefaultMethod = _apiType.GetMethod("RegisterVisualBundle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(string), _modelStyleType, typeof(AssetBundle), typeof(bool) }, null);
                        }
                    }
                    break;
                }
            }
        }

        public static bool IsLoaded()
        {
            EnsureInitialized();
            if (_isLoadedMethod != null)
            {
                return (bool)_isLoadedMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsBackpack(ItemDrop.ItemData itemData)
        {
            EnsureInitialized();
            if (_isBackpackMethod != null && itemData != null)
            {
                return (bool)_isBackpackMethod.Invoke(null, new object[] { itemData });
            }
            return false;
        }

        public static bool IsBackpackEquipped(Player player)
        {
            EnsureInitialized();
            if (_isBackpackEquippedMethod != null && player != null)
            {
                return (bool)_isBackpackEquippedMethod.Invoke(null, new object[] { player });
            }
            return false;
        }

        public static bool CanOpenBackpack(Player player)
        {
            EnsureInitialized();
            if (_canOpenBackpackMethod != null && player != null)
            {
                return (bool)_canOpenBackpackMethod.Invoke(null, new object[] { player });
            }
            return false;
        }

        public static bool IsBackpackOpen()
        {
            EnsureInitialized();
            if (_isBackpackOpenMethod != null)
            {
                return (bool)_isBackpackOpenMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsThisBackpackEquipped(Player player, ItemDrop.ItemData itemData)
        {
            EnsureInitialized();
            if (_isThisBackpackEquippedMethod != null && player != null && itemData != null)
            {
                return (bool)_isThisBackpackEquippedMethod.Invoke(null, new object[] { player, itemData });
            }
            return false;
        }

        public static Inventory GetEquippedBackpackInventory(Player player)
        {
            EnsureInitialized();
            if (_getEquippedBackpackInventoryMethod != null && player != null)
            {
                return (Inventory)_getEquippedBackpackInventoryMethod.Invoke(null, new object[] { player });
            }
            return null;
        }

        public static bool TryGetEquippedBackpackInventory(Player player, out Inventory inventory)
        {
            inventory = null;
            EnsureInitialized();
            if (_tryGetEquippedBackpackInventoryMethod != null && player != null)
            {
                object[] args = new object[] { player, null };
                bool result = (bool)_tryGetEquippedBackpackInventoryMethod.Invoke(null, args);
                inventory = (Inventory)args[1];
                return result;
            }
            return false;
        }

        public static Inventory GetBackpackInventory(ItemDrop.ItemData itemData)
        {
            EnsureInitialized();
            if (_getBackpackInventoryMethod != null && itemData != null)
            {
                return (Inventory)_getBackpackInventoryMethod.Invoke(null, new object[] { itemData });
            }
            return null;
        }

        public static bool TryGetBackpackInventory(ItemDrop.ItemData itemData, out Inventory inventory)
        {
            inventory = null;
            EnsureInitialized();
            if (_tryGetBackpackInventoryMethod != null && itemData != null)
            {
                object[] args = new object[] { itemData, null };
                bool result = (bool)_tryGetBackpackInventoryMethod.Invoke(null, args);
                inventory = (Inventory)args[1];
                return result;
            }
            return false;
        }

        public static List<Inventory> GetAllBackpackInventories(Player player)
        {
            EnsureInitialized();
            if (_getAllBackpackInventoriesMethod != null && player != null)
            {
                return (List<Inventory>)_getAllBackpackInventoriesMethod.Invoke(null, new object[] { player });
            }
            return new List<Inventory>();
        }

        public static bool TryGetAllBackpackInventories(Player player, out List<Inventory> inventories)
        {
            inventories = null;
            EnsureInitialized();
            if (_tryGetAllBackpackInventoriesMethod != null && player != null)
            {
                object[] args = new object[] { player, null };
                bool result = (bool)_tryGetAllBackpackInventoriesMethod.Invoke(null, args);
                inventories = (List<Inventory>)args[1];
                return result;
            }
            return false;
        }

        public static void OpenBackpack(Player player, InventoryGui gui)
        {
            EnsureInitialized();
            if (_openBackpackMethod != null && player != null && gui != null)
            {
                _openBackpackMethod.Invoke(null, new object[] { player, gui });
            }
        }

        public static void OpenBackpack(Player player, InventoryGui gui, int activeGroup)
        {
            EnsureInitialized();
            if (_openBackpackWithGroupMethod != null && player != null && gui != null)
            {
                _openBackpackWithGroupMethod.Invoke(null, new object[] { player, gui, activeGroup });
            }
            else if (_openBackpackMethod != null && player != null && gui != null)
            {
                _openBackpackMethod.Invoke(null, new object[] { player, gui });
            }
        }

        public static bool IsOpenWithCraftingStationEnabled()
        {
            EnsureInitialized();
            if (_isOpenWithCraftingStationEnabledMethod != null)
            {
                return (bool)_isOpenWithCraftingStationEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsOpenWithInventoryEnabled()
        {
            EnsureInitialized();
            if (_isOpenWithInventoryEnabledMethod != null)
            {
                return (bool)_isOpenWithInventoryEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsEnableContainerTabsEnabled()
        {
            EnsureInitialized();
            if (_isEnableContainerTabsEnabledMethod != null)
            {
                return (bool)_isEnableContainerTabsEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool HasActiveExternalContainer()
        {
            EnsureInitialized();
            if (_hasActiveExternalContainerMethod != null)
            {
                return (bool)_hasActiveExternalContainerMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsCraftFromBackpackEnabled()
        {
            EnsureInitialized();
            if (_isCraftFromBackpackEnabledMethod != null)
            {
                return (bool)_isCraftFromBackpackEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsCraftingContextActive()
        {
            EnsureInitialized();
            if (_isCraftingContextActiveMethod != null)
            {
                return (bool)_isCraftingContextActiveMethod.Invoke(null, null);
            }
            return false;
        }

        public static string GetConsumptionPriority()
        {
            EnsureInitialized();
            if (_getConsumptionPriorityMethod != null)
            {
                return (string)_getConsumptionPriorityMethod.Invoke(null, null);
            }
            return "PlayerInventoryFirst";
        }

        public static bool IsLeaveOneItemInBackpackEnabled()
        {
            EnsureInitialized();
            if (_isLeaveOneItemInBackpackEnabledMethod != null)
            {
                return (bool)_isLeaveOneItemInBackpackEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static bool IsDisplayTotalIngredientCountEnabled()
        {
            EnsureInitialized();
            if (_isDisplayTotalIngredientCountEnabledMethod != null)
            {
                return (bool)_isDisplayTotalIngredientCountEnabledMethod.Invoke(null, null);
            }
            return false;
        }

        public static void SuppressNativeCrafting(string modIdentifier)
        {
            EnsureInitialized();
            if (_suppressNativeCraftingMethod != null && !string.IsNullOrEmpty(modIdentifier))
            {
                _suppressNativeCraftingMethod.Invoke(null, new object[] { modIdentifier });
            }
        }

        public static void UnsuppressNativeCrafting(string modIdentifier)
        {
            EnsureInitialized();
            if (_unsuppressNativeCraftingMethod != null && !string.IsNullOrEmpty(modIdentifier))
            {
                _unsuppressNativeCraftingMethod.Invoke(null, new object[] { modIdentifier });
            }
        }

        /// <summary>
        /// Determines whether Adventure Backpacks would store a picked up item into the equipped backpack of the specified player.
        /// Returns false if Adventure Backpacks is not available or if the item would not be stored.
        /// </summary>
        /// <param name="player">Player, usually Player.m_localPlayer</param>
        /// <param name="item">ItemDrop.ItemData candidate for pickup</param>
        /// <returns>True if the item would be stored in the backpack, otherwise false.</returns>
        public static bool WouldStorePickup(Player player, ItemDrop.ItemData item)
        {
            EnsureInitialized();
            if (!_isAvailable || _wouldStorePickupMethod == null)
                return false;

            try
            {
                return (bool)_wouldStorePickupMethod.Invoke(null, new object[] { player, item });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether Adventure Backpacks would store a picked up item into the equipped backpack of the specified player,
        /// and outputs the backpack inventory if accepted.
        /// Returns false if Adventure Backpacks is not available or if the item would not be stored.
        /// </summary>
        /// <param name="player">Player, usually Player.m_localPlayer</param>
        /// <param name="item">ItemDrop.ItemData candidate for pickup</param>
        /// <param name="backpackInventory">When true, contains the target backpack inventory; otherwise, null.</param>
        /// <returns>True if the item would be stored in the backpack, otherwise false.</returns>
        public static bool WouldStorePickup(Player player, ItemDrop.ItemData item, out Inventory backpackInventory)
        {
            backpackInventory = null;
            EnsureInitialized();
            if (!_isAvailable || _wouldStorePickupWithInventoryMethod == null)
                return false;

            try
            {
                object[] args = new object[] { player, item, null };
                bool result = (bool)_wouldStorePickupWithInventoryMethod.Invoke(null, args);
                if (result)
                {
                    backpackInventory = args[2] as Inventory;
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registers an AssetBundle replacement for a specific backpack model under a named Texture/Visual Pack.
        /// </summary>
        /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
        /// <param name="targetBundleName">The base bundle name to override (e.g. "backpack_meadows", "classic_backpack_meadows").</param>
        /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
        /// <returns>True if successfully registered, false otherwise.</returns>
        public static bool RegisterVisualBundle(string packName, string targetBundleName, AssetBundle overrideBundle)
        {
            return RegisterVisualBundle(packName, targetBundleName, overrideBundle, false);
        }

        public static bool RegisterVisualBundle(string packName, string targetBundleName, AssetBundle overrideBundle, bool isDefault)
        {
            EnsureInitialized();
            if (!_isAvailable)
                return false;

            try
            {
                if (_registerVisualBundleWithDefaultMethod != null)
                {
                    _registerVisualBundleWithDefaultMethod.Invoke(null, new object[] { packName, targetBundleName, overrideBundle, isDefault });
                    return true;
                }

                if (_registerVisualBundleMethod != null)
                {
                    _registerVisualBundleMethod.Invoke(null, new object[] { packName, targetBundleName, overrideBundle });
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registers an AssetBundle replacement for a specific backpack and model style under a named Texture/Visual Pack.
        /// </summary>
        /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
        /// <param name="backpackId">The backpack identifier to override (e.g. "backpack_meadows").</param>
        /// <param name="modelStyle">The model style this override applies to (Modern or Classic).</param>
        /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
        /// <param name="isDefault">Optional: If true and no player configuration is set, this pack will be the default selection for this backpack and style.</param>
        /// <returns>True if successfully registered, false otherwise.</returns>
        public static bool RegisterVisualBundle(string packName, string backpackId, ModelStyle modelStyle, AssetBundle overrideBundle, bool isDefault = false)
        {
            EnsureInitialized();
            if (!_isAvailable || _modelStyleType == null)
                return false;

            try
            {
                object boxedStyle = Enum.ToObject(_modelStyleType, (int)modelStyle);

                if (_registerVisualBundleStyleWithDefaultMethod != null)
                {
                    _registerVisualBundleStyleWithDefaultMethod.Invoke(null, new object[] { packName, backpackId, boxedStyle, overrideBundle, isDefault });
                    return true;
                }

                if (_registerVisualBundleStyleMethod != null)
                {
                    _registerVisualBundleStyleMethod.Invoke(null, new object[] { packName, backpackId, boxedStyle, overrideBundle });
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Registers an AssetBundle replacement for a specific progression backpack and model style under a named Texture/Visual Pack.
        /// </summary>
        /// <param name="packName">Display name of the texture/visual pack (e.g. "Native HD", "Willybach HD").</param>
        /// <param name="backpack">The strongly typed progression backpack to override.</param>
        /// <param name="modelStyle">The model style this override applies to (Modern or Classic).</param>
        /// <param name="overrideBundle">The loaded AssetBundle containing replacement assets.</param>
        /// <param name="isDefault">Optional: If true and no player configuration is set, this pack will be the default selection for this backpack and style.</param>
        /// <returns>True if successfully registered, false otherwise.</returns>
        public static bool RegisterVisualBundle(string packName, BackpackName backpack, ModelStyle modelStyle, AssetBundle overrideBundle, bool isDefault = false)
        {
            return RegisterVisualBundle(packName, backpack.ToBackpackId(), modelStyle, overrideBundle, isDefault);
        }
    }

    /// <summary>
    /// Extension methods for Adventure Backpacks client API types.
    /// </summary>
    public static class BackpackExtensions
    {
        /// <summary>
        /// Converts the strongly typed BackpackName to its internal canonical identifier string.
        /// </summary>
        /// <param name="backpack">The BackpackName enum value.</param>
        /// <returns>The internal string identifier (e.g. "backpack_meadows").</returns>
        public static string ToBackpackId(this ABAPIClient.BackpackName backpack)
        {
            return backpack switch
            {
                ABAPIClient.BackpackName.Meadows => "backpack_meadows",
                ABAPIClient.BackpackName.BlackForest => "backpack_black_forest",
                ABAPIClient.BackpackName.Swamp => "backpack_swamp",
                ABAPIClient.BackpackName.Mountains => "backpack_mountains",
                ABAPIClient.BackpackName.Plains => "backpack_plains",
                ABAPIClient.BackpackName.Mistlands => "backpack_mistlands",
                ABAPIClient.BackpackName.Ashlands => "backpack_ashlands",
                ABAPIClient.BackpackName.DeepNorth => "backpack_deepnorth",
                _ => "backpack_meadows"
            };
        }
    }
}
