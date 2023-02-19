using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class FrostResistance : EffectsBase
{
    public static HitData.DamageModPair EffectMod = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
    public FrostResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override bool IsEffectActive(Humanoid human)
    {
        if (human is Player player)
        {
            var equippedBackpack = player.GetEquippedBackpack();
            
            if (equippedBackpack == null || !EnabledEffect.Value)
                return false;
            
            var itemData = equippedBackpack.Item;
            return IsEffectActive(itemData);
        }

        return false;
    }
}