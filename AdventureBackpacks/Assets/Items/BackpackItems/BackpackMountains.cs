using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMountains : BackpackItem
{
    public BackpackMountains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.Mountains;
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