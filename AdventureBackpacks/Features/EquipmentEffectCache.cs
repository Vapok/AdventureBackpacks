using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Features;

public static class EquipmentEffectCache
{
    public static HashSet<StatusEffect> ActiveEffects = new();

    public static HashSet<StatusEffect> AddActiveBackpackEffects(HashSet<StatusEffect> other, Humanoid instance)
    {
        if (other == null)
        {
            AdventureBackpacks.Log.Debug($"other HashSet argument is null. Expecting initialized object.");
            other = new();
        }

        if (instance is Player player && player == Player.m_localPlayer)
        {
            ActiveEffects = new HashSet<StatusEffect>();

            foreach (var effectKeyValuePair in EffectsFactory.EffectList)
            {
                if (effectKeyValuePair.Value.HasActiveStatusEffect(player, out var statusEffect))
                    ActiveEffects.Add(statusEffect);  
            }
          
            other.UnionWith(ActiveEffects);
    
            AdventureBackpacks.Log.Debug($"Adding {other.Count} Active Backpack Effects.");
        }
          
        return other;
    }
}