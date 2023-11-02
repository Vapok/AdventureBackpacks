namespace AdventureBackpacks.Assets.Effects;

public class FeatherFall : EffectsBase
{
    public FeatherFall(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override void LoadStatusEffect()
    {
        SetStatusEffect("SlowFall");
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        SetStatusEffect("SlowFall");
        return base.HasActiveStatusEffect(human, out statusEffect);
    }
}


