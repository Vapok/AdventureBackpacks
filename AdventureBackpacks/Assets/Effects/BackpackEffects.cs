using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public static class BackpackEffects
{
    public static HitData.DamageModPair FrostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
}

public enum EquipmentEffects
{
    SlowFall,
}

public static class EquipmentEffectCache
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    public static class BackpackEffectsHumanoidUpdateEquipmentStatusEffectsPatch
    {
        public static HashSet<StatusEffect> activeEffects = new();

        [UsedImplicitly]
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Humanoid __instance)
        {
          activeEffects = new HashSet<StatusEffect>();

            var deMister = ObjectDB.instance.GetStatusEffect("Demister");
            var slowFall = ObjectDB.instance.GetStatusEffect("SlowFall");
            
            if (__instance.m_leftItem != null && (bool) (UnityEngine.Object) __instance.m_leftItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_leftItem.m_shared.m_equipStatusEffect);
            if (__instance.m_rightItem != null && (bool) (UnityEngine.Object) __instance.m_rightItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_rightItem.m_shared.m_equipStatusEffect);
            if (__instance.m_chestItem != null && (bool) (UnityEngine.Object) __instance.m_chestItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_chestItem.m_shared.m_equipStatusEffect);
            if (__instance.m_legItem != null && (bool) (UnityEngine.Object) __instance.m_legItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_legItem.m_shared.m_equipStatusEffect);
            if (__instance.m_helmetItem != null && (bool) (UnityEngine.Object) __instance.m_helmetItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_helmetItem.m_shared.m_equipStatusEffect);
            if (__instance.m_shoulderItem != null && (bool) (UnityEngine.Object) __instance.m_shoulderItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_shoulderItem.m_shared.m_equipStatusEffect);
            if (__instance.m_utilityItem != null && (bool) (UnityEngine.Object) __instance.m_utilityItem.m_shared.m_equipStatusEffect)
              activeEffects.Add(__instance.m_utilityItem.m_shared.m_equipStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_leftItem))
              activeEffects.Add(__instance.m_leftItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_rightItem))
              activeEffects.Add(__instance.m_rightItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_chestItem))
              activeEffects.Add(__instance.m_chestItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_legItem))
              activeEffects.Add(__instance.m_legItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_helmetItem))
              activeEffects.Add(__instance.m_helmetItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_shoulderItem))
              activeEffects.Add(__instance.m_shoulderItem.m_shared.m_setStatusEffect);
            if (__instance.HaveSetEffect(__instance.m_utilityItem))
              activeEffects.Add(__instance.m_utilityItem.m_shared.m_setStatusEffect);

            foreach (StatusEffect eqipmentStatusEffect in __instance.m_eqipmentStatusEffects)
            {
              
              if (eqipmentStatusEffect.Equals(deMister) && Demister.ShouldHaveDemister(__instance))
              {
                if (!activeEffects.Contains(eqipmentStatusEffect)) 
                  activeEffects.Add(eqipmentStatusEffect);
              }
              
              
              if (eqipmentStatusEffect.Equals(slowFall) && FeatherFall.ShouldHaveFeatherFall(__instance))
              {
                if (!activeEffects.Contains(eqipmentStatusEffect)) 
                  activeEffects.Add(eqipmentStatusEffect);
              }
            }

            foreach (StatusEffect eqipmentStatusEffect in __instance.m_eqipmentStatusEffects)
            {
              if (!activeEffects.Contains(eqipmentStatusEffect))
                __instance.m_seman.RemoveStatusEffect(eqipmentStatusEffect.name);
            }
            
            foreach (StatusEffect statusEffect in activeEffects)
            {
              if (!__instance.m_eqipmentStatusEffects.Contains(statusEffect))
                __instance.m_seman.AddStatusEffect(statusEffect);
            }
            
            __instance.m_eqipmentStatusEffects.Clear();
            __instance.m_eqipmentStatusEffects.UnionWith((IEnumerable<StatusEffect>) activeEffects);

            return false;
        }
    }
}
