using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public class InventoryGridPatches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class UpdateGuiPatch
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(InventoryGrid __instance)
        {
            if (!__instance.name.Equals("ContainerGrid")) return true;

            if (__instance.m_elements.Count >= __instance.m_inventory.m_inventory.Count) return true;
            
            if ((__instance.m_width != __instance.m_inventory.m_width) ||
                (__instance.m_height != __instance.m_inventory.m_height)) return true;
            
            __instance.m_width = __instance.m_inventory.m_width + 1;
            __instance.m_height = __instance.m_inventory.m_height + 1;

            return true;
        }
    }
}