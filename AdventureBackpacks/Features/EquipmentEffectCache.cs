using System.Collections.Generic;
using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Features;

public static class EquipmentEffectCache
{
    public static HashSet<StatusEffect> ActiveEffects = new();

    public static HashSet<StatusEffect> AddActiveBackpackEffects(HashSet<StatusEffect> other)
    {
        if (other == null)
        {
            AdventureBackpacks.Log.Debug($"other HashSet argument is null. Expecting initialized object.");
            other = new();
        }

        if (Player.m_localPlayer == null)
        {
            AdventureBackpacks.Log.Debug($"Add Active Effects Started... Player null");
            return other;
        }
    
        var player = Player.m_localPlayer;
          
        ActiveEffects = new HashSet<StatusEffect>();

        foreach (var effectKeyValuePair in EffectsFactory.EffectList)
        {
            if (effectKeyValuePair.Value.HasActiveStatusEffect(player, out var statusEffect))
                ActiveEffects.Add(statusEffect);  
        }
          
        other.UnionWith(ActiveEffects);
    
        AdventureBackpacks.Log.Debug($"Adding {other.Count} Active Backpack Effects.");
        return other;
    }
}