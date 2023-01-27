using AdventureBackpacks.Assets.Factories;

namespace AdventureBackpacks.Assets.Items.BackpackItems;

internal class BackpackMeadows : BackpackItem
{
    public BackpackMeadows(string prefabName, string itemName) : base(prefabName , itemName)
    {
        Biome = BackpackBiomes.Meadows;

        RegisterConfigSettings();
        
        AddRecipeIngredient("CapeDeerHide",1);
        AddRecipeIngredient("DeerHide",8);
        AddRecipeIngredient("BoneFragments",2);
        
        AddUpgradeIngredient("LeatherScraps", 5);
        AddUpgradeIngredient("DeerHide", 3);
        
        RegisterShaderSwap();
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