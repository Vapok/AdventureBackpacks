using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public class Waterproof: EffectsBase
{
    private StatusEffect _externalStatusEffect;
    public Waterproof(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    
    private void LoadExternalStatusEffect()
    {
        if (_externalStatusEffect == null)
        {
            StatusEffect wet = ObjectDB.instance == null ? null : ObjectDB.instance.GetStatusEffect("Wet".GetStableHashCode());
            CustomSE se = new(Enums.StatusEffects.Stats, "SE_vapok_ab_wet_resistance");
            se.Effect.m_name = "$vapok_mod_se_wet_resistance";
            se.Effect.m_icon = wet != null ? wet.m_icon : null;
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
}

