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
        _backpackItems.Add(new BackpackMeadows("BackpackMeadows","$vapok_mod_item_backpack_meadows"));
        _backpackItems.Add(new BackpackBlackForest("BackpackBlackForest","$vapok_mod_item_backpack_blackforest"));
        _backpackItems.Add(new BackpackSwamp("BackpackSwamp","$vapok_mod_item_backpack_swamp"));
        _backpackItems.Add(new BackpackMountains("BackpackMountains","$vapok_mod_item_backpack_mountains"));
        _backpackItems.Add(new BackpackPlains("BackpackPlains","$vapok_mod_item_backpack_plains"));
        _backpackItems.Add(new BackpackMistlands("BackpackMistlands","$vapok_mod_item_backpack_mistlands"));
        _backpackItems.Add(new LegacyIronBackpack("CapeIronBackpack","$vapok_mod_item_rugged_backpack"));
        _backpackItems.Add(new LegacySilverBackpack("CapeSilverBackpack","$vapok_mod_item_arctic_backpack"));

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