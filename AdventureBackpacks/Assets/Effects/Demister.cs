namespace AdventureBackpacks.Assets.Effects;

public class Demister: EffectsBase
{
    public static bool DemisterActive = true;
    public static Heightmap.Biome PreviouseBiome = Heightmap.Biome.None;
    
    public Demister(string effectName, string effectDesc) : base(effectName, effectDesc)
    {
    }


    public override void LoadStatusEffect()
    {
        if (!DemisterActive)
            return;
        
        SetStatusEffect("Demister");
    }

    public override bool HasActiveStatusEffect(Humanoid human, out StatusEffect statusEffect)
    {
        if (!DemisterActive)
        {
            statusEffect = null;
            return false;   
        }
            
        SetStatusEffect("Demister");
        return base.HasActiveStatusEffect(human, out statusEffect);
    }
}