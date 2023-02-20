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
            var wet = ObjectDB.instance.GetStatusEffect("Wet");
            var se = new CustomSE(Enums.StatusEffects.Stats, "SE_vapok_ab_wet_resistance");
            se.Effect.m_name = "$vapok_mod_se_wet_resistance";
            se.Effect.m_icon = wet.m_icon;
            _externalStatusEffect = se.Effect;
        }
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
