using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMistlands : BackpackItem
{
    public BackpackMistlands(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.Mistlands;
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