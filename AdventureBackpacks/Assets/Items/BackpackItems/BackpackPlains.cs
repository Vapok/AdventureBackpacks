using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackPlains : BackpackItem
{
    public BackpackPlains(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.Plains;
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