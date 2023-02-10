using AdventureBackpacks.Extensions;
using HarmonyLib;

namespace AdventureBackpacks.Patches;

public class HumanoidPatches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    static class HumanoidUnequipItemPatch
    {
        // The "__instance" here is a Humanoid type, but we want the ItemData argument, so we use "__0" instead.
        // "__0" fetches the argument passed into the first parameter of the original method, which in this case is an ItemData object.
        static void Prefix(ItemDrop.ItemData __0)
        {
            if (__0 is null) return;
            
            if ( Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;

            var item = __0;

            // Check if the item being unequipped is a backpack, and see if it is the same backpack the player is wearing
            if (item.IsBackpack() && player.m_shoulderItem == item)
            {
                var backpackInventory = player.GetEquippedBackpack();
                if (backpackInventory is null) return;

                //Save Backpack
                backpackInventory.Save();

                var inventoryGui = InventoryGui.instance;

                // Close the backpack inventory if it's currently open
                if (inventoryGui.IsContainerOpen())
                {
                    inventoryGui.CloseContainer();
                    InventoryGuiPatches.BackpackIsOpen = false;
                }

                InventoryGuiPatches.BackpackEquipped = false;
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    static class HumanoidEquipItemPatch
    {
        static void Postfix(ItemDrop.ItemData __0, bool __result)
        {
            if (__0 is null) return;
            
            if ( Player.m_localPlayer == null && !__result)
                return;

            var item = __0;

            if (item.IsBackpack())
            {
                InventoryGuiPatches.BackpackEquipped = true;
            }
        }
    }
}