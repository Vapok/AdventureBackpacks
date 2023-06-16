using Vapok.Common.Managers.StatusEffects;
using Vapok.Common.Shared;

namespace AdventureBackpacks.Assets.Effects;

public class ColdResistance : EffectsBase
{
    private StatusEffect _externalStatusEffect;
    public ColdResistance(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }
    
    private void LoadExternalStatusEffect()
    {
        if (_externalStatusEffect == null)
        {
            var cold = ObjectDB.instance.GetStatusEffect("Cold".GetHashCode());
            var se = new CustomSE(Enums.StatusEffects.Stats, "SE_vapok_ab_cold_immunity");
            se.Effect.m_name = "$vapok_mod_se_cold_immunity";
            se.Effect.m_icon = cold.m_icon;
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