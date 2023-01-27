using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackBlackForest : BackpackItem
{
    public BackpackBlackForest(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.BlackForest;
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