using System;
using System.Collections.Generic;
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
    FrostResistance
}
public class EffectsFactory : FactoryBase
{
    public static Dictionary<BackpackEffect,EffectsBase> EffectList => _effectList;

    private static Dictionary<BackpackEffect,EffectsBase> _effectList = new();
    
    public EffectsFactory(ILogIt logger, ConfigSyncBase configs) : base(logger, configs)
    {
    
    }

    public void RegisterEffects()
    {
        _effectList.Add(BackpackEffect.FeatherFall, new FeatherFall("Feather Fall", "When activated allows you to slow fall gracefully and without damage from high elevations."));
        _effectList.Add(BackpackEffect.ColdResistance, new ColdResistance("Cold Immunity", "When activated keeps you from feeling cold. Does not prevent freezing."));
        _effectList.Add(BackpackEffect.Demister, new Effects.Demister("Demister", "When activated provides you with the Wisplight Effect, which clears mist from a small area around you while in the Mistlands."));
        _effectList.Add(BackpackEffect.WaterResistance, new Waterproof("Water Resistance", "When activated allows you to stay dry from the rain. Will still get wet if swimming."));
        _effectList.Add(BackpackEffect.FrostResistance, new FrostResistance("Frost Resistance", "When activated allows you to stay warm in freezing conditions, negating the freezing debuff."));

        foreach (BackpackEffect effect in Enum.GetValues(typeof(BackpackEffect)))
        {   
            EffectList[effect].RegisterEffectConfiguration();
        }
    }
}