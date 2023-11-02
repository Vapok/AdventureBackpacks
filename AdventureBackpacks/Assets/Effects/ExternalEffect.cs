using System;
using AdventureBackpacks.API;
using AdventureBackpacks.Extensions;

namespace AdventureBackpacks.Assets.Effects;

public class ExternalEffect : EffectsBase
{
    private string _effectName = string.Empty;
    private StatusEffect _externalStatusEffect;
    private ABAPI.EffectDefinition _effectDefinition;

    public ExternalEffect(ABAPI.EffectDefinition effectDefinition) : base(effectDefinition.Name, effectDefinition.Description)
    {
        _effectDefinition = effectDefinition;
        
        if (_effectDefinition.StatusEffect == null)
        {
            AdventureBackpacks.Log.Error($"Status Effect is null {_effectName} - Disabling Status Effect");
            EnabledEffect.Value = false;
        }

        if (!string.IsNullOrEmpty(_effectDefinition.EffectName))
            _effectName = _effectDefinition.EffectName;
    }

    public override void AdditionalConfiguration(string configSection)
    {
    }

    private void LoadExternalStatusEffect()
    {
        try
        {
            if (_externalStatusEffect != null) return;
            
            _externalStatusEffect = _effectDefinition.StatusEffect;
            SetStatusEffect(_externalStatusEffect);
        }
        catch (Exception e)
        {
            AdventureBackpacks.Log.Error($"Can't Load External Status Effect: {_effectName} - Message: {e.Message}");
        }
    }

    public override void LoadStatusEffect()
    {
        LoadExternalStatusEffect();
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = null;
        if (!EnabledEffect.Value)
            return false;
        
        LoadExternalStatusEffect();
        SetStatusEffect(_externalStatusEffect);
        return base.HasActiveStatusEffect(human, out statusEffect);
    }

    public override bool HasActiveStatusEffect(ItemDrop.ItemData item, out StatusEffect statusEffect)
    {
        statusEffect = null;
        if (!EnabledEffect.Value)
            return false;
        
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