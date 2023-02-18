using System.Linq;
using AdventureBackpacks.Features;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Patches;

public static class SEManPatches
{
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), new[] { typeof(string), typeof(bool) })]
    public static class RemoveStatusEffects
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(string name, ref bool __result)
        {
            if (EquipmentEffectCache.ActiveEffects == null)
                return true;
        
            if (EquipmentEffectCache.ActiveEffects.Any(x => x.name.Equals(name)))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

}