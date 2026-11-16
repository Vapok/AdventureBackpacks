using AdventureBackpacks.Extensions;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public class InventoryGridPatches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class UpdateGuiPatch
    {
        [HarmonyPrepare]
        private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(InventoryGrid __instance)
        {
            if (__instance == null || __instance.m_elements == null || __instance.m_inventory == null || __instance.m_inventory.m_inventory == null)
                return true;

            bool isContainerGrid = (InventoryGui.instance != null && __instance == InventoryGui.instance.m_containerGrid) ||
                                   (InventoryGui.instance == null && __instance.name.Equals("ContainerGrid"));
            if (!isContainerGrid)
                return true;

            if (__instance.m_elements.Count >= __instance.m_inventory.m_inventory.Count) return true;
            
            if ((__instance.m_width != __instance.m_inventory.m_width) ||
                (__instance.m_height != __instance.m_inventory.m_height)) return true;
            
            __instance.m_width = __instance.m_inventory.m_width + 1;
            __instance.m_height = __instance.m_inventory.m_height + 1;

            return true;
        }
    }
}