using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackSwamp : BackpackItem
{
    public BackpackSwamp(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.Swamp;
        RegisterConfigSettings();
    }

    internal sealed override void RegisterConfigSettings()
    {
        //RegisterBackpackSize();
        RegisterWeightMultiplier();
        //RegisterCarryBonus();
        //RegisterSpeedMod();
        //RegisterEnableFreezing();
    }
}