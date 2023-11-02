using AdventureBackpacks.Extensions;
using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public class FrostResistance : EffectsBase
{
    private StatusEffect _externalStatusEffect;
    public static HitData.DamageModPair EffectMod = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
    public FrostResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    private void LoadExternalStatusEffect()
    {
        if (_externalStatusEffect == null)
        {
            var freezing = ObjectDB.instance.GetStatusEffect("Freezing".GetHashCode());
            var se = new CustomSE(Enums.StatusEffects.Stats, "SE_vapok_ab_frost_resistance");
            se.Effect.m_name = "$vapok_mod_se_frost_resistance";
            se.Effect.m_icon = freezing.m_icon;
            _externalStatusEffect = se.Effect;
            SetStatusEffect(_externalStatusEffect);
        }
    }

    public override void LoadStatusEffect()
    {
        LoadExternalStatusEffect();
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        LoadExternalStatusEffect();
        SetStatusEffect(_externalStatusEffect);
        return base.HasActiveStatusEffect(human, out statusEffect);
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        LoadExternalStatusEffect();
        SetStatusEffect(_externalStatusEffect);
        return base.HasActiveStatusEffect(item, out statusEffect);
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