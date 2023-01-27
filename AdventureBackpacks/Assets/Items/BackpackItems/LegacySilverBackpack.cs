using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class LegacySilverBackpack : BackpackItem
{
    public LegacySilverBackpack(string prefabName, string itemName) : base(prefabName, itemName)
    {
        Biome = BackpackBiomes.None;
        
        RegisterConfigSettings();
    }

    internal sealed override void RegisterConfigSettings()
    {
        RegisterBackpackSize();
        RegisterWeightMultiplier();
        RegisterCarryBonus();
        RegisterSpeedMod();
        RegisterEnableFreezing();
    }
}