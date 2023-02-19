namespace AdventureBackpacks.Assets.Effects;

public class Demister: EffectsBase
{
    public Demister(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        statusEffect = GetStatusEffect("Demister");
        return statusEffect != null && IsEffectActive(human);
    }
}