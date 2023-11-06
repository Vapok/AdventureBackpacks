using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace AdventureBackpacks.Compats;

public static class ContentsWithin
{
    private static PluginInfo _plugin;
    private static Assembly _assembly;
    public static Type Main;
    public static Type InventoryGuiPatch;
    public static void Awake(Harmony harmony, string guidID)
    {
        var pluginLoaded = Chainloader.PluginInfos.TryGetValue(guidID, out _plugin);
        
        if (!pluginLoaded) return;
        
        _assembly = Assembly.LoadFile(_plugin.Location);
        Main = _assembly.GetType("ContentsWithin.ContentsWithin");
        InventoryGuiPatch = Main.GetNestedType("InventoryGuiPatch");

        var hasContainerAccessMethod = AccessTools.Method(InventoryGuiPatch, "HasContainerAccess");

        if (InventoryGuiPatch != null)
        {
            harmony.Patch(hasContainerAccessMethod, new HarmonyMethod(typeof(ContentsWithin), nameof(ContainerAccessPrefix)));
        }
    }

    private static bool ContainerAccessPrefix(Container container, ref bool __result)
    {
        if (container == null) return true;
        if (!container.name.Equals("Player(Clone)")) return true;
        
        __result = false;
        return false;
    }
}