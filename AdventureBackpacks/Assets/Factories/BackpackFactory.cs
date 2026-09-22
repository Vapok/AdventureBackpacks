using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Assets.Items.BackpackItems;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace AdventureBackpacks.Assets.Factories;


internal class BackpackFactory : AssetFactory
{
    private static HashSet<BackpackItem> _backpackItems = new();
    private static bool _initialized;
    private static List<ABAPI.BackpackDefinition> _externalBackpacks = new();
    
    internal static IEnumerable<BackpackItem> BackpackItems => _backpackItems;
    

    internal BackpackFactory(ILogIt logger, ConfigSyncBase configSync) : base(logger, configSync)
    {
        if (!_initialized)
        {
            BackpackItem.SetConfig(configSync);
            BackpackItem.SetLogger(logger);
            _initialized = true;
        }
    }
    
    public static void RegisterExternalBackpack(ABAPI.BackpackDefinition backpackDefinition)
    {
        _externalBackpacks.Add(backpackDefinition);
    }


    internal override void CreateAssets()
    {
        //v2 bundles use new prefab names, keep the old ones registered for saves and AzuEPI
        _backpackItems.Add(new BackpackMeadows("backpack_meadows","BackpackSatchel","$vapok_mod_item_backpack_meadows", registerAs: "BackpackMeadows"));
        _backpackItems.Add(new BackpackBlackForest("backpack_black_forest","BackpackRugged","$vapok_mod_item_backpack_blackforest", registerAs: "BackpackBlackForest"));
        _backpackItems.Add(new BackpackSwamp("backpack_swamp","BackpackWetpack","$vapok_mod_item_backpack_swamp", registerAs: "BackpackSwamp"));
        _backpackItems.Add(new BackpackMountains("backpack_mountains","BackpackSherpa","$vapok_mod_item_backpack_mountains", registerAs: "BackpackMountains"));
        _backpackItems.Add(new BackpackPlains("backpack_plains","BackpackLox","$vapok_mod_item_backpack_plains", registerAs: "BackpackPlains"));
        _backpackItems.Add(new BackpackMistlands("backpack_mistlands","BackpackWisppack","$vapok_mod_item_backpack_mistlands", registerAs: "BackpackMistlands"));
        _backpackItems.Add(new BackpackAshlands("backpack_ashlands","BackpackCinderward","$vapok_mod_item_backpack_ashlands", registerAs: BackpackAshlands.Prefab));
        _backpackItems.Add(new BackpackDeepNorth("backpack_deepnorth","BackpackFrostbound","$vapok_mod_item_backpack_deepnorth", registerAs: BackpackDeepNorth.Prefab));

        _backpackItems.Add(new LegacyIronBackpack("vapokbackpacks","CapeIronBackpack","$vapok_mod_item_rugged_backpack"));
        _backpackItems.Add(new LegacySilverBackpack("vapokbackpacks","CapeSilverBackpack","$vapok_mod_item_arctic_backpack"));

        foreach (var backpackDefinition in _externalBackpacks)
        {
            if (_backpackItems.Any(x => x.ItemName.Equals(backpackDefinition.ItemName))) return;
            
            var newBackpack = backpackDefinition.BackPackGo != null ? 
                new ExternalBackpack(backpackDefinition, backpackDefinition.BackPackGo) : 
                new ExternalBackpack(backpackDefinition);
            
            _backpackItems.Add(newBackpack);

        }
    }

    internal static List<string> BackpackTypes()
    {
        return BackpackItems.Select(x => x.ItemName).ToList();
    }
}