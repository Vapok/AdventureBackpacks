using AdventureBackpacks.Assets;
using AdventureBackpacks.Components;
using HarmonyLib;
using Vapok.Common.Managers;

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
            
            if (Backpacks.BackpackContainer == null || Player.m_localPlayer == null)
                return;
            
            var player = Player.m_localPlayer;

            var item = __0;

            // Check if the item being unequipped is a backpack, and see if it is the same backpack the player is wearing
            if (Backpacks.BackpackTypes.Contains(item.m_shared.m_name)
                && player.m_shoulderItem == item)
            {
                var backpackInventory = Backpacks.BackpackContainer.m_inventory;
                if (backpackInventory is null) return;

                //Save Backpack
                var backpackComponent = item.Data().GetOrCreate<BackpackComponent>();
                backpackComponent.Save(backpackInventory);

                var inventoryGui = InventoryGui.instance;

                // Close the backpack inventory if it's currently open
                if (inventoryGui.IsContainerOpen())
                {
                    inventoryGui.CloseContainer();
                }

                Backpacks.ResetBackpackContainer();
            }
        }

    }
}