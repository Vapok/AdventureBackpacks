namespace AdventureBackpacks.Assets.Effects;

public class Demister: EffectsBase
{
    public Demister(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }


    public override void LoadStatusEffect()
    {
        SetStatusEffect("Demister");
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        SetStatusEffect("Demister");
        return base.HasActiveStatusEffect(human, out statusEffect);
    }
}