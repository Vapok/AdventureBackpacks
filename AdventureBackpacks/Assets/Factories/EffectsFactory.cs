using System;
using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Effects;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Factories;

public enum BackpackEffect
{
    FeatherFall,
    ColdResistance,
    Demister,
    WaterResistance,
    FrostResistance,
    TrollArmor,
    NecromancyArmor,
    ExternalEffect
}
public class EffectsFactory : FactoryBase
{

    public static HashSet<EffectsBase> AllEffects => _allEffects;
    public static Dictionary<BackpackEffect,EffectsBase> EffectList => _effectList;

    private static Dictionary<BackpackEffect,EffectsBase> _effectList = new();
    
    private static HashSet<EffectsBase> _externalEffects = new();
    private static HashSet<EffectsBase> _allEffects = new();
    
    public EffectsFactory(ILogIt logger, ConfigSyncBase configs) : base(logger, configs)
    {
    
    }

    public static HashSet<StatusEffect> GetRegisteredEffects()
    {
        var effects = new HashSet<StatusEffect>();

        foreach (var effectsBase in _effectList)
        {
            effects.Add(effectsBase.Value.GetStatusEffect());
        }
        
        foreach (var effectsBase in _externalEffects)
        {
            effects.Add(effectsBase.GetStatusEffect());
        }
        return effects;
    }
    public static void RegisterExternalEffect(ABAPI.EffectDefinition effectDefinition)
    {
        if (_externalEffects.Any(x => x.EffectName.Equals(effectDefinition.EffectName))) return;
        
        var externalEffect = new ExternalEffect(effectDefinition);
        _externalEffects.Add(externalEffect);
    }

    public void RegisterEffects()
    {
        _effectList.Add(BackpackEffect.FeatherFall, new FeatherFall("Feather Fall", "When activated allows you to slow fall gracefully and without damage from high elevations."));
        _effectList.Add(BackpackEffect.ColdResistance, new ColdResistance("Cold Immunity", "When activated keeps you from feeling cold. Does not prevent freezing."));
        _effectList.Add(BackpackEffect.Demister, new Effects.Demister("Demister", "When activated provides you with the Wisplight Effect, which clears mist from a small area around you while in the Mistlands."));
        _effectList.Add(BackpackEffect.WaterResistance, new Waterproof("Water Resistance", "When activated allows you to stay dry from the rain. Will still get wet if swimming."));
        _effectList.Add(BackpackEffect.FrostResistance, new FrostResistance("Frost Resistance", "When activated allows you to stay warm in freezing conditions, negating the freezing debuff."));
        _effectList.Add(BackpackEffect.TrollArmor, new TrollArmor("Troll Armor Set", "When activated the backpack acts as the Shoulder Set piece of the Troll Armor Set allowing the set to complete for the Sneak Effect"));

        foreach (BackpackEffect effect in Enum.GetValues(typeof(BackpackEffect)))
        {
            switch (effect)
            {
                case BackpackEffect.ExternalEffect:
                    foreach (var effectsBase in _externalEffects)
                    {
                        _allEffects.Add(effectsBase);
                        effectsBase.RegisterEffectConfiguration();
                    }
                    break;
                case BackpackEffect.FeatherFall:
                case BackpackEffect.ColdResistance:
                case BackpackEffect.Demister:
                case BackpackEffect.WaterResistance:
                case BackpackEffect.FrostResistance:
                case BackpackEffect.TrollArmor:
                case BackpackEffect.NecromancyArmor:
                    if (!EffectList.ContainsKey(effect)) continue;
                    _allEffects.Add(EffectList[effect]);
                    EffectList[effect].RegisterEffectConfiguration();
                    break;
            }
        }
    }
}