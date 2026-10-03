using System;
using System.Collections.Generic;
using System.Reflection;

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
        private static MethodInfo _isThisBackpackEquippedMethod;
        private static MethodInfo _getEquippedBackpackInventoryMethod;
        private static MethodInfo _tryGetEquippedBackpackInventoryMethod;
        private static MethodInfo _getBackpackInventoryMethod;
        private static MethodInfo _tryGetBackpackInventoryMethod;
        private static MethodInfo _getAllBackpackInventoriesMethod;
        private static MethodInfo _tryGetAllBackpackInventoriesMethod;
        private static MethodInfo _openBackpackMethod;
        private static MethodInfo _isCraftFromBackpackEnabledMethod;
        private static MethodInfo _isCraftingContextActiveMethod;
        private static MethodInfo _getConsumptionPriorityMethod;
        private static MethodInfo _isLeaveOneItemInBackpackEnabledMethod;
        private static MethodInfo _isDisplayTotalIngredientCountEnabledMethod;
        private static MethodInfo _suppressNativeCraftingMethod;
        private static MethodInfo _unsuppressNativeCraftingMethod;

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
                        _isThisBackpackEquippedMethod = _apiType.GetMethod("IsThisBackpackEquipped", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(ItemDrop.ItemData) }, null);
                        _getEquippedBackpackInventoryMethod = _apiType.GetMethod("GetEquippedBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _tryGetEquippedBackpackInventoryMethod = _apiType.GetMethod("TryGetEquippedBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(Inventory).MakeByRefType() }, null);
                        _getBackpackInventoryMethod = _apiType.GetMethod("GetBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ItemDrop.ItemData) }, null);
                        _tryGetBackpackInventoryMethod = _apiType.GetMethod("TryGetBackpackInventory", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(ItemDrop.ItemData), typeof(Inventory).MakeByRefType() }, null);
                        _getAllBackpackInventoriesMethod = _apiType.GetMethod("GetAllBackpackInventories", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player) }, null);
                        _tryGetAllBackpackInventoriesMethod = _apiType.GetMethod("TryGetAllBackpackInventories", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(List<Inventory>).MakeByRefType() }, null);
                        _openBackpackMethod = _apiType.GetMethod("OpenBackpack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Player), typeof(InventoryGui) }, null);
                        _isCraftFromBackpackEnabledMethod = _apiType.GetMethod("IsCraftFromBackpackEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isCraftingContextActiveMethod = _apiType.GetMethod("IsCraftingContextActive", BindingFlags.Public | BindingFlags.Static);
                        _getConsumptionPriorityMethod = _apiType.GetMethod("GetConsumptionPriority", BindingFlags.Public | BindingFlags.Static);
                        _isLeaveOneItemInBackpackEnabledMethod = _apiType.GetMethod("IsLeaveOneItemInBackpackEnabled", BindingFlags.Public | BindingFlags.Static);
                        _isDisplayTotalIngredientCountEnabledMethod = _apiType.GetMethod("IsDisplayTotalIngredientCountEnabled", BindingFlags.Public | BindingFlags.Static);
                        _suppressNativeCraftingMethod = _apiType.GetMethod("SuppressNativeCrafting", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                        _unsuppressNativeCraftingMethod = _apiType.GetMethod("UnsuppressNativeCrafting", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
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
    }
}
