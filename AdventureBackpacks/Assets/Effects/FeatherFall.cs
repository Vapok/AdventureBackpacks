namespace AdventureBackpacks.Assets.Effects;

public class FeatherFall : EffectsBase
{
    public FeatherFall(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = GetStatusEffect("SlowFall");
        return statusEffect != null && IsEffectActive(human);
    }
}


