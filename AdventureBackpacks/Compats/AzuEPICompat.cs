using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.Assets.Factories;
using AdventureBackpacks.Assets.Items.BackpackItems;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace AdventureBackpacks.Compats;

//AzuEPI hardcodes the backpack slot prefabs. Its AddSlot merges into an existing slot, so just add ours.
internal static class AzuEPICompat
{
    private const string PluginGuid = "Azumatt.AzuExtendedPlayerInventory";
    private const string SlotName = "$bp_backpack_slot_name";

    internal static bool IsLoaded => Chainloader.PluginInfos.ContainsKey(PluginGuid);

    internal static void RegisterBackpackSlot()
    {
        if (!IsLoaded)
            return;

        try
        {
            var api = AccessTools.TypeByName("AzuEPI.API");
            var addSlot = api == null ? null : AccessTools.Method(api, "AddSlot", new[] { typeof(string), typeof(IEnumerable<string>), typeof(int) });
            if (addSlot == null)
            {
                AdventureBackpacks.Log?.Warning("AzuEPI is loaded but AzuEPI.API.AddSlot(string, IEnumerable<string>, int) was not found. New packs will use the shoulder slot.");
                return;
            }

            var prefabs = BackpackFactory.BackpackItems
                .Where(item => item is not ExternalBackpack && !string.IsNullOrEmpty(item.PrefabName))
                .Select(item => item.PrefabName)
                .Distinct()
                .ToList();

            if (prefabs.Count == 0)
                return;

            var added = (bool)addSlot.Invoke(null, new object[] { SlotName, prefabs, -1 });
            AdventureBackpacks.Log?.Info(added
                ? $"Registered {prefabs.Count} packs with the AzuEPI backpack slot."
                : "AzuEPI declined the backpack slot registration; it may be marked for removal in its config.");
        }
        catch (Exception ex)
        {
            AdventureBackpacks.Log?.Warning($"AzuEPI backpack slot registration failed: {ex.Message}");
        }
    }
}
