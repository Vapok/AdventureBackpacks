using System.Collections.Generic;
using System.Linq;
using AdventureBackpacks.API;
using AdventureBackpacks.Assets.Items;
using AdventureBackpacks.Assets.Items.BackpackItems;
using UnityEngine;
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
        _backpackItems.Add(new BackpackMeadows("backpack_meadows","BackpackMeadows","$vapok_mod_item_backpack_meadows"));
        _backpackItems.Add(new BackpackBlackForest("backpack_black_forest","BackpackBlackForest","$vapok_mod_item_backpack_blackforest"));
        _backpackItems.Add(new BackpackSwamp("backpack_swamp","BackpackSwamp","$vapok_mod_item_backpack_swamp"));
        _backpackItems.Add(new BackpackMountains("backpack_mountains","BackpackMountains","$vapok_mod_item_backpack_mountains"));
        _backpackItems.Add(new BackpackPlains("backpack_plains","BackpackPlains","$vapok_mod_item_backpack_plains"));
        _backpackItems.Add(new BackpackMistlands("backpack_mistlands","BackpackMistlands","$vapok_mod_item_backpack_mistlands"));
        _backpackItems.Add(new BackpackAshlands("backpack_ashlands","BackpackAshlands","$vapok_mod_item_backpack_ashlands"));
        _backpackItems.Add(new BackpackDeepNorth("backpack_deepnorth","BackpackDeepNorth","$vapok_mod_item_backpack_deepnorth"));
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

    internal static void ApplyUpgraderResources(ObjectDB objectDB)
    {
        if (objectDB == null || objectDB.m_recipes == null)
            return;

        foreach (BackpackItem backpack in _backpackItems)
        {
            if (backpack.UpgraderIngredients.Count == 0)
                continue;

            List<Recipe> recipes = objectDB.m_recipes
                .Where(r => r != null && r.m_item != null && r.m_item.gameObject.name.Equals(backpack.PrefabName))
                .ToList();

            foreach (Recipe recipe in recipes)
            {
                List<Piece.Requirement> resourceList = recipe.m_resources.ToList();
                bool modified = false;

                foreach (KeyValuePair<string, int> upgraderIng in backpack.UpgraderIngredients)
                {
                    if (resourceList.Any(x => x.m_resItem != null && x.m_resItem.gameObject.name.Equals(upgraderIng.Key) && x.m_upgraderResource))
                        continue;

                    GameObject prefab = objectDB.GetItemPrefab(upgraderIng.Key);
                    if (prefab == null)
                        continue;

                    ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
                    if (itemDrop == null)
                        continue;

                    resourceList.Add(new Piece.Requirement
                    {
                        m_resItem = itemDrop,
                        m_amount = upgraderIng.Value,
                        m_upgraderResource = true,
                        m_amountPerLevel = 0,
                        m_recover = false
                    });
                    modified = true;
                }

                if (modified)
                {
                    recipe.m_resources = resourceList.ToArray();
                }
            }
        }
    }
}