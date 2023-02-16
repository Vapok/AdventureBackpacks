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
      private static StatusEffect effectDemister;
      private static StatusEffect effectSlowfall;

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static void Prefix(Humanoid __instance)
        { 
          activeEffects = new HashSet<StatusEffect>();
          backpackEffects = new HashSet<StatusEffect>();

          effectDemister = effectDemister == null ? ObjectDB.instance.GetStatusEffect("Demister") : effectDemister;
          effectSlowfall = effectSlowfall == null ? ObjectDB.instance.GetStatusEffect("SlowFall") : effectSlowfall;

          void EnsureEffectsAdded(StatusEffect se, bool shouldHave)
          {
            if (shouldHave && !backpackEffects.Any( x => x.name.Equals(se.name)))
              backpackEffects.Add(se);
          }
          
          EnsureEffectsAdded(effectDemister,Demister.ShouldHaveDemister(__instance));
          EnsureEffectsAdded(effectSlowfall,FeatherFall.ShouldHaveFeatherFall(__instance));

          foreach (var backpackEffect in backpackEffects)
          {
            if (!activeEffects.Any( x => x.name.Equals(backpackEffect.name)))
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
              if (!__instance.m_eqipmentStatusEffects.Any( x => x.name.Equals(activeEffect.name)))
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