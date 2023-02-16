using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public static class BackpackEffects
{
    public static HitData.DamageModPair FrostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
}

public static class EquipmentEffectCache
{
  private static HashSet<StatusEffect> activeEffects = new();
  private static HashSet<StatusEffect> backpackEffects = new();
  
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    public static class UpdateStatusEffects
    {

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Humanoid __instance)
        {
          activeEffects = new HashSet<StatusEffect>();
          backpackEffects = new HashSet<StatusEffect>();

            var deMister = ObjectDB.instance.GetStatusEffect("Demister");
            var slowFall = ObjectDB.instance.GetStatusEffect("SlowFall");

            foreach (StatusEffect eqipmentStatusEffect in __instance.m_eqipmentStatusEffects)
            {
              
              if (eqipmentStatusEffect.Equals(deMister) && Demister.ShouldHaveDemister(__instance))
              {
                if (!backpackEffects.Contains(eqipmentStatusEffect)) 
                  backpackEffects.Add(eqipmentStatusEffect);
              }
              
              
              if (eqipmentStatusEffect.Equals(slowFall) && FeatherFall.ShouldHaveFeatherFall(__instance))
              {
                if (!backpackEffects.Contains(eqipmentStatusEffect)) 
                  backpackEffects.Add(eqipmentStatusEffect);
              }
            }

            foreach (var backpackEffect in backpackEffects)
            {
              if (!activeEffects.Contains(backpackEffect))
                activeEffects.Add(backpackEffect);
            }
        }
        
        [UsedImplicitly]
        public static void Finalizer(Exception __exception, Humanoid __instance)
        {
          if (__exception != null)
          {
            AdventureBackpacks.Log.Error($"Error: {__exception.Message}");
            AdventureBackpacks.Log.Error($"Stack Trace: {__exception.StackTrace}");
            AdventureBackpacks.Log.Error($"Source: {__exception.Source}");
            throw __exception;
          }
          
          try
          {
            backpackEffects = new HashSet<StatusEffect>();

            foreach (var activeEffect in activeEffects)
            {
              if (!__instance.m_eqipmentStatusEffects.Contains(activeEffect))
                backpackEffects.Add(activeEffect);
            }
          
            __instance.m_eqipmentStatusEffects.UnionWith(backpackEffects);

          }
          catch (Exception e)
          {
            AdventureBackpacks.Log.Error($"Error: {e.Message}");
            AdventureBackpacks.Log.Error($"Stack Trace: {e.StackTrace}");
            AdventureBackpacks.Log.Error($"Source: {e.Source}");
            throw e;
          }
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), new[] { typeof(string), typeof(bool) })]
    public static class RemoveStatusEffects
    {

      [UsedImplicitly]
      [HarmonyPriority(Priority.First)]
      public static bool Prefix(string name, ref bool __result)
      {
        if (activeEffects == null)
          return true;
          
        if (activeEffects.Any(x => x.name.Equals(name)))
        {
          __result = false;
          return false;
        }

        return true;
      }
    }
}


