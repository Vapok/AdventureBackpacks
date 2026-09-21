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
            StatusEffect cold = ObjectDB.instance.GetStatusEffect("Cold".GetStableHashCode());
            CustomSE se = new(Enums.StatusEffects.Stats, "SE_vapok_ab_cold_immunity");
            se.Effect.m_name = "$vapok_mod_se_cold_immunity";
            se.Effect.m_icon = cold.m_icon;
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

    public override void OnUpdateEnvStatusEffects(Player player)
    {
        if (player == null || !IsEffectActive(player))
            return;

        SEMan seMan = player.GetSEMan();
        if (seMan == null)
            return;

        seMan.RemoveStatusEffect(SEMan.s_statusEffectCold);
    }
}