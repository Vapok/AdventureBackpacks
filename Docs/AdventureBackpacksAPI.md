<a name='assembly'></a>
# AdventureBackpacksAPI

## Contents

- [ABAPI](#T-AdventureBackpacks-API-ABAPI 'AdventureBackpacks.API.ABAPI')
  - [CanOpenBackpack(player)](#M-AdventureBackpacks-API-ABAPI-CanOpenBackpack-Player- 'AdventureBackpacks.API.ABAPI.CanOpenBackpack(Player)')
  - [GetActiveBackpackStatusEffects()](#M-AdventureBackpacks-API-ABAPI-GetActiveBackpackStatusEffects 'AdventureBackpacks.API.ABAPI.GetActiveBackpackStatusEffects')
  - [GetBackpack(itemData)](#M-AdventureBackpacks-API-ABAPI-GetBackpack-ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.GetBackpack(ItemDrop.ItemData)')
  - [GetEquippedBackpack(player)](#M-AdventureBackpacks-API-ABAPI-GetEquippedBackpack-Player- 'AdventureBackpacks.API.ABAPI.GetEquippedBackpack(Player)')
  - [GetRegisterdStatusEffects()](#M-AdventureBackpacks-API-ABAPI-GetRegisterdStatusEffects 'AdventureBackpacks.API.ABAPI.GetRegisterdStatusEffects')
  - [IsBackpack(itemData)](#M-AdventureBackpacks-API-ABAPI-IsBackpack-ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.IsBackpack(ItemDrop.ItemData)')
  - [IsBackpackEquipped(player)](#M-AdventureBackpacks-API-ABAPI-IsBackpackEquipped-Player- 'AdventureBackpacks.API.ABAPI.IsBackpackEquipped(Player)')
  - [IsLoaded()](#M-AdventureBackpacks-API-ABAPI-IsLoaded 'AdventureBackpacks.API.ABAPI.IsLoaded')
  - [IsThisBackpackEquipped(player,itemData)](#M-AdventureBackpacks-API-ABAPI-IsThisBackpackEquipped-Player,ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.IsThisBackpackEquipped(Player,ItemDrop.ItemData)')
  - [OpenBackpack(player,gui)](#M-AdventureBackpacks-API-ABAPI-OpenBackpack-Player,InventoryGui- 'AdventureBackpacks.API.ABAPI.OpenBackpack(Player,InventoryGui)')
  - [RegisterBackpack(definition)](#M-AdventureBackpacks-API-ABAPI-RegisterBackpack-AdventureBackpacks-API-ABAPI-BackpackDefinition- 'AdventureBackpacks.API.ABAPI.RegisterBackpack(AdventureBackpacks.API.ABAPI.BackpackDefinition)')
  - [RegisterEffect(effectDefinition)](#M-AdventureBackpacks-API-ABAPI-RegisterEffect-AdventureBackpacks-API-ABAPI-EffectDefinition- 'AdventureBackpacks.API.ABAPI.RegisterEffect(AdventureBackpacks.API.ABAPI.EffectDefinition)')
- [Backpack](#T-AdventureBackpacks-API-ABAPI-Backpack 'AdventureBackpacks.API.ABAPI.Backpack')
  - [Definition](#F-AdventureBackpacks-API-ABAPI-Backpack-Definition 'AdventureBackpacks.API.ABAPI.Backpack.Definition')
  - [Inventory](#F-AdventureBackpacks-API-ABAPI-Backpack-Inventory 'AdventureBackpacks.API.ABAPI.Backpack.Inventory')
  - [ItemData](#F-AdventureBackpacks-API-ABAPI-Backpack-ItemData 'AdventureBackpacks.API.ABAPI.Backpack.ItemData')
  - [Name](#F-AdventureBackpacks-API-ABAPI-Backpack-Name 'AdventureBackpacks.API.ABAPI.Backpack.Name')
- [BackpackBiomes](#T-AdventureBackpacks-API-BackpackBiomes 'AdventureBackpacks.API.BackpackBiomes')
  - [Ashlands](#F-AdventureBackpacks-API-BackpackBiomes-Ashlands 'AdventureBackpacks.API.BackpackBiomes.Ashlands')
  - [BlackForest](#F-AdventureBackpacks-API-BackpackBiomes-BlackForest 'AdventureBackpacks.API.BackpackBiomes.BlackForest')
  - [DeepNorth](#F-AdventureBackpacks-API-BackpackBiomes-DeepNorth 'AdventureBackpacks.API.BackpackBiomes.DeepNorth')
  - [EffectBiome1](#F-AdventureBackpacks-API-BackpackBiomes-EffectBiome1 'AdventureBackpacks.API.BackpackBiomes.EffectBiome1')
  - [EffectBiome2](#F-AdventureBackpacks-API-BackpackBiomes-EffectBiome2 'AdventureBackpacks.API.BackpackBiomes.EffectBiome2')
  - [EffectBiome3](#F-AdventureBackpacks-API-BackpackBiomes-EffectBiome3 'AdventureBackpacks.API.BackpackBiomes.EffectBiome3')
  - [EffectBiome4](#F-AdventureBackpacks-API-BackpackBiomes-EffectBiome4 'AdventureBackpacks.API.BackpackBiomes.EffectBiome4')
  - [EffectBiome5](#F-AdventureBackpacks-API-BackpackBiomes-EffectBiome5 'AdventureBackpacks.API.BackpackBiomes.EffectBiome5')
  - [Meadows](#F-AdventureBackpacks-API-BackpackBiomes-Meadows 'AdventureBackpacks.API.BackpackBiomes.Meadows')
  - [Mistlands](#F-AdventureBackpacks-API-BackpackBiomes-Mistlands 'AdventureBackpacks.API.BackpackBiomes.Mistlands')
  - [Mountains](#F-AdventureBackpacks-API-BackpackBiomes-Mountains 'AdventureBackpacks.API.BackpackBiomes.Mountains')
  - [None](#F-AdventureBackpacks-API-BackpackBiomes-None 'AdventureBackpacks.API.BackpackBiomes.None')
  - [Plains](#F-AdventureBackpacks-API-BackpackBiomes-Plains 'AdventureBackpacks.API.BackpackBiomes.Plains')
  - [Swamp](#F-AdventureBackpacks-API-BackpackBiomes-Swamp 'AdventureBackpacks.API.BackpackBiomes.Swamp')
- [BackpackDefinition](#T-AdventureBackpacks-API-ABAPI-BackpackDefinition 'AdventureBackpacks.API.ABAPI.BackpackDefinition')
  - [#ctor()](#M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor 'AdventureBackpacks.API.ABAPI.BackpackDefinition.#ctor')
  - [#ctor(backPackGo)](#M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor-UnityEngine-GameObject- 'AdventureBackpacks.API.ABAPI.BackpackDefinition.#ctor(UnityEngine.GameObject)')
  - [#ctor(assetBundle,prefabName)](#M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor-UnityEngine-AssetBundle,System-String- 'AdventureBackpacks.API.ABAPI.BackpackDefinition.#ctor(UnityEngine.AssetBundle,System.String)')
  - [AssetBundle](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetBundle 'AdventureBackpacks.API.ABAPI.BackpackDefinition.AssetBundle')
  - [BackPackGo](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackPackGo 'AdventureBackpacks.API.ABAPI.BackpackDefinition.BackPackGo')
  - [BackpackBiome](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackBiome 'AdventureBackpacks.API.ABAPI.BackpackDefinition.BackpackBiome')
  - [BackpackSizeByQuality](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackSizeByQuality 'AdventureBackpacks.API.ABAPI.BackpackDefinition.BackpackSizeByQuality')
  - [CarryBonus](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-CarryBonus 'AdventureBackpacks.API.ABAPI.BackpackDefinition.CarryBonus')
  - [ConfigSection](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ConfigSection 'AdventureBackpacks.API.ABAPI.BackpackDefinition.ConfigSection')
  - [CraftingTable](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-CraftingTable 'AdventureBackpacks.API.ABAPI.BackpackDefinition.CraftingTable')
  - [DropsFrom](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-DropsFrom 'AdventureBackpacks.API.ABAPI.BackpackDefinition.DropsFrom')
  - [EffectsToApply](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EffectsToApply 'AdventureBackpacks.API.ABAPI.BackpackDefinition.EffectsToApply')
  - [EnableFreezing](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EnableFreezing 'AdventureBackpacks.API.ABAPI.BackpackDefinition.EnableFreezing')
  - [ItemName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.ItemName')
  - [ItemSetStatusEffect](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemSetStatusEffect 'AdventureBackpacks.API.ABAPI.BackpackDefinition.ItemSetStatusEffect')
  - [MaxRequiredStationLevel](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-MaxRequiredStationLevel 'AdventureBackpacks.API.ABAPI.BackpackDefinition.MaxRequiredStationLevel')
  - [PrefabName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-PrefabName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.PrefabName')
  - [RecipeIngredients](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-RecipeIngredients 'AdventureBackpacks.API.ABAPI.BackpackDefinition.RecipeIngredients')
  - [SpeedMod](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-SpeedMod 'AdventureBackpacks.API.ABAPI.BackpackDefinition.SpeedMod')
  - [StationLevel](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-StationLevel 'AdventureBackpacks.API.ABAPI.BackpackDefinition.StationLevel')
  - [UpgradeIngredients](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-UpgradeIngredients 'AdventureBackpacks.API.ABAPI.BackpackDefinition.UpgradeIngredients')
  - [WeightMultiplier](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-WeightMultiplier 'AdventureBackpacks.API.ABAPI.BackpackDefinition.WeightMultiplier')
- [DropTarget](#T-AdventureBackpacks-API-ABAPI-DropTarget 'AdventureBackpacks.API.ABAPI.DropTarget')
  - [#ctor(creature,chance,min,max)](#M-AdventureBackpacks-API-ABAPI-DropTarget-#ctor-System-String,System-Single,System-Int32,System-Nullable{System-Int32}- 'AdventureBackpacks.API.ABAPI.DropTarget.#ctor(System.String,System.Single,System.Int32,System.Nullable{System.Int32})')
  - [Chance](#F-AdventureBackpacks-API-ABAPI-DropTarget-Chance 'AdventureBackpacks.API.ABAPI.DropTarget.Chance')
  - [Creature](#F-AdventureBackpacks-API-ABAPI-DropTarget-Creature 'AdventureBackpacks.API.ABAPI.DropTarget.Creature')
  - [Max](#F-AdventureBackpacks-API-ABAPI-DropTarget-Max 'AdventureBackpacks.API.ABAPI.DropTarget.Max')
  - [Min](#F-AdventureBackpacks-API-ABAPI-DropTarget-Min 'AdventureBackpacks.API.ABAPI.DropTarget.Min')
- [EffectDefinition](#T-AdventureBackpacks-API-ABAPI-EffectDefinition 'AdventureBackpacks.API.ABAPI.EffectDefinition')
  - [#ctor(name,localizedName,effectName,description,statusEffect)](#M-AdventureBackpacks-API-ABAPI-EffectDefinition-#ctor-System-String,System-String,System-String,System-String,StatusEffect- 'AdventureBackpacks.API.ABAPI.EffectDefinition.#ctor(System.String,System.String,System.String,System.String,StatusEffect)')
  - [Description](#F-AdventureBackpacks-API-ABAPI-EffectDefinition-Description 'AdventureBackpacks.API.ABAPI.EffectDefinition.Description')
  - [EffectName](#F-AdventureBackpacks-API-ABAPI-EffectDefinition-EffectName 'AdventureBackpacks.API.ABAPI.EffectDefinition.EffectName')
  - [LocalizedName](#F-AdventureBackpacks-API-ABAPI-EffectDefinition-LocalizedName 'AdventureBackpacks.API.ABAPI.EffectDefinition.LocalizedName')
  - [Name](#F-AdventureBackpacks-API-ABAPI-EffectDefinition-Name 'AdventureBackpacks.API.ABAPI.EffectDefinition.Name')
  - [StatusEffect](#F-AdventureBackpacks-API-ABAPI-EffectDefinition-StatusEffect 'AdventureBackpacks.API.ABAPI.EffectDefinition.StatusEffect')
- [RecipeIngredient](#T-AdventureBackpacks-API-ABAPI-RecipeIngredient 'AdventureBackpacks.API.ABAPI.RecipeIngredient')
  - [#ctor(itemPrefabName,quantity)](#M-AdventureBackpacks-API-ABAPI-RecipeIngredient-#ctor-System-String,System-Int32- 'AdventureBackpacks.API.ABAPI.RecipeIngredient.#ctor(System.String,System.Int32)')
  - [ItemPrefabName](#F-AdventureBackpacks-API-ABAPI-RecipeIngredient-ItemPrefabName 'AdventureBackpacks.API.ABAPI.RecipeIngredient.ItemPrefabName')
  - [Quantity](#F-AdventureBackpacks-API-ABAPI-RecipeIngredient-Quantity 'AdventureBackpacks.API.ABAPI.RecipeIngredient.Quantity')

<a name='T-AdventureBackpacks-API-ABAPI'></a>
## ABAPI `type`

##### Namespace

AdventureBackpacks.API

##### Summary

Adventure Backpacks API. Be sure to include the AdventureBackpacksAPI.dll as a dependency to your project.

<a name='M-AdventureBackpacks-API-ABAPI-CanOpenBackpack-Player-'></a>
### CanOpenBackpack(player) `method`

##### Summary

Determines if the player is capable of currently opening the equipped backpack.

##### Returns

true or false

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player, usually Player.m_localPlayer |

<a name='M-AdventureBackpacks-API-ABAPI-GetActiveBackpackStatusEffects'></a>
### GetActiveBackpackStatusEffects() `method`

##### Summary

Retrieves the current Active Backpack StatusEffects running in the local players game.

##### Returns

HashSet of Status Effects.

##### Parameters

This method has no parameters.

<a name='M-AdventureBackpacks-API-ABAPI-GetBackpack-ItemDrop-ItemData-'></a>
### GetBackpack(itemData) `method`

##### Summary

Returns Backpack object of the provided itemData. Operates similarly to a TryGet but with a nullable type.

##### Returns

Nullable Backpack Object. Check HasValue.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| itemData | [ItemDrop.ItemData](#T-ItemDrop-ItemData 'ItemDrop.ItemData') | ItemDrop.ItemData object |

<a name='M-AdventureBackpacks-API-ABAPI-GetEquippedBackpack-Player-'></a>
### GetEquippedBackpack(player) `method`

##### Summary

Returns a Backpack object if the provided Player is currently wearing a backpack.

##### Returns

Nullable Backpack Object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player, usually Player.m_localPlayer |

<a name='M-AdventureBackpacks-API-ABAPI-GetRegisterdStatusEffects'></a>
### GetRegisterdStatusEffects() `method`

##### Summary

Retrieves all Status Effects Registered with Adventure Backpacks

##### Returns

HashSet of Status Effects.

##### Parameters

This method has no parameters.

<a name='M-AdventureBackpacks-API-ABAPI-IsBackpack-ItemDrop-ItemData-'></a>
### IsBackpack(itemData) `method`

##### Summary

When provided with an ItemData object, will detect whether the Item is an Adventure Backpack or not.

##### Returns

true or false

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| itemData | [ItemDrop.ItemData](#T-ItemDrop-ItemData 'ItemDrop.ItemData') | This is the ItemDrop.ItemData object of the item. |

<a name='M-AdventureBackpacks-API-ABAPI-IsBackpackEquipped-Player-'></a>
### IsBackpackEquipped(player) `method`

##### Summary

Determines if the Player provided is currently wearing a backpack.

##### Returns

true or false

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player, usually Player.m_localPlayer |

<a name='M-AdventureBackpacks-API-ABAPI-IsLoaded'></a>
### IsLoaded() `method`

##### Summary

Notifies if the ABAPI is active or not.

##### Returns

true of false

##### Parameters

This method has no parameters.

<a name='M-AdventureBackpacks-API-ABAPI-IsThisBackpackEquipped-Player,ItemDrop-ItemData-'></a>
### IsThisBackpackEquipped(player,itemData) `method`

##### Summary

Determines if the player provided is wearing the item provided and that it's a backpack.

##### Returns

true or false. If item provided is not a backpack, will return false.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player, usually Player.m_localPlayer |
| itemData | [ItemDrop.ItemData](#T-ItemDrop-ItemData 'ItemDrop.ItemData') | Any ItemData |

<a name='M-AdventureBackpacks-API-ABAPI-OpenBackpack-Player,InventoryGui-'></a>
### OpenBackpack(player,gui) `method`

##### Summary

Method to activate the backpack on the local player's GUI and open it. Use in conjunction with CanOpenBackpack()

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| player | [Player](#T-Player 'Player') | Player, usually Player.m_localPlayer |
| gui | [InventoryGui](#T-InventoryGui 'InventoryGui') | The instance of InventoryGui |

<a name='M-AdventureBackpacks-API-ABAPI-RegisterBackpack-AdventureBackpacks-API-ABAPI-BackpackDefinition-'></a>
### RegisterBackpack(definition) `method`

##### Summary

Use this method in the Awake() of your mod to register a new Backpack that can be utilized on Adventure Backpacks.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| definition | [AdventureBackpacks.API.ABAPI.BackpackDefinition](#T-AdventureBackpacks-API-ABAPI-BackpackDefinition 'AdventureBackpacks.API.ABAPI.BackpackDefinition') | Create a new BackpackDefinition that contains the overall parameters that are needed to register the new backpack. |

<a name='M-AdventureBackpacks-API-ABAPI-RegisterEffect-AdventureBackpacks-API-ABAPI-EffectDefinition-'></a>
### RegisterEffect(effectDefinition) `method`

##### Summary

Use this method in the Awake() of your mod to register a Status Effect that can be utilized on Adventure Backpacks

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| effectDefinition | [AdventureBackpacks.API.ABAPI.EffectDefinition](#T-AdventureBackpacks-API-ABAPI-EffectDefinition 'AdventureBackpacks.API.ABAPI.EffectDefinition') | Create a new EffectDefinition that contains the overall parameters that are needed to register the new effect. |

<a name='T-AdventureBackpacks-API-ABAPI-Backpack'></a>
## Backpack `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Instanced Backpack Information

<a name='F-AdventureBackpacks-API-ABAPI-Backpack-Definition'></a>
### Definition `constants`

##### Summary

Definition of the Backpack item

<a name='F-AdventureBackpacks-API-ABAPI-Backpack-Inventory'></a>
### Inventory `constants`

##### Summary

Inventory object of the configured backpack

<a name='F-AdventureBackpacks-API-ABAPI-Backpack-ItemData'></a>
### ItemData `constants`

##### Summary

ItemData representative of the Backpack

<a name='F-AdventureBackpacks-API-ABAPI-Backpack-Name'></a>
### Name `constants`

##### Summary

Backpack Name

<a name='T-AdventureBackpacks-API-BackpackBiomes'></a>
## BackpackBiomes `type`

##### Namespace

AdventureBackpacks.API

##### Summary

This is a Flags enum for determining Backpack Biomes. This is not representative of Heightmap.Biomes.
This is instead a way of creating sets of Effects. Default Adventure Backpacks will utilize Biome names.
Custom External Effects can pick and choose between the Custom Effect flags as well.

<a name='F-AdventureBackpacks-API-BackpackBiomes-Ashlands'></a>
### Ashlands `constants`

##### Summary

Ashland Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-BlackForest'></a>
### BlackForest `constants`

##### Summary

Black Forest Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-DeepNorth'></a>
### DeepNorth `constants`

##### Summary

DeepNorth Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-EffectBiome1'></a>
### EffectBiome1 `constants`

##### Summary

Custom Biome for use with External Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-EffectBiome2'></a>
### EffectBiome2 `constants`

##### Summary

Custom Biome for use with External Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-EffectBiome3'></a>
### EffectBiome3 `constants`

##### Summary

Custom Biome for use with External Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-EffectBiome4'></a>
### EffectBiome4 `constants`

##### Summary

Custom Biome for use with External Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-EffectBiome5'></a>
### EffectBiome5 `constants`

##### Summary

Custom Biome for use with External Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-Meadows'></a>
### Meadows `constants`

##### Summary

Meadows Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-Mistlands'></a>
### Mistlands `constants`

##### Summary

Mistlands Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-Mountains'></a>
### Mountains `constants`

##### Summary

Mountains Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-None'></a>
### None `constants`

##### Summary

None, no backpack effects are applied.

<a name='F-AdventureBackpacks-API-BackpackBiomes-Plains'></a>
### Plains `constants`

##### Summary

Plains Backpack Effects

<a name='F-AdventureBackpacks-API-BackpackBiomes-Swamp'></a>
### Swamp `constants`

##### Summary

Swamp Backpack Effects

<a name='T-AdventureBackpacks-API-ABAPI-BackpackDefinition'></a>
## BackpackDefinition `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Backpack Definition Settings
Use this object to create backpack definitions for adding Backpacks to Adventure Backpacks.
You can use either the GameObject directly, or provide your AssetBundle object and the PrefabName.

<a name='M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor'></a>
### #ctor() `constructor`

##### Summary

Default Constructor

##### Parameters

This constructor has no parameters.

<a name='M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor-UnityEngine-GameObject-'></a>
### #ctor(backPackGo) `constructor`

##### Summary

Use this constructor when adding a backpack using the GameObject
The item should have ItemDrop.ItemData on the item, and it should be an item that is utilizing the Shoulder slot.
Equipped Detection won't detect if not in the shoulder slot.
TODO: Make this more flexible for additional slots through AzuEPI

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| backPackGo | [UnityEngine.GameObject](#T-UnityEngine-GameObject 'UnityEngine.GameObject') | GameObject of |

<a name='M-AdventureBackpacks-API-ABAPI-BackpackDefinition-#ctor-UnityEngine-AssetBundle,System-String-'></a>
### #ctor(assetBundle,prefabName) `constructor`

##### Summary

Use this constructor when adding a backpack using the AssetBundle and Prefab Name
The item should have ItemDrop.ItemData on the item, and it should be an item that is utilizing the Shoulder slot.
Equipped Detection won't detect if not in the shoulder slot.
TODO: Make this more flexible for additional slots through AzuEPI

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| assetBundle | [UnityEngine.AssetBundle](#T-UnityEngine-AssetBundle 'UnityEngine.AssetBundle') | Provide the Asset Bundle that contains the backpack prefab |
| prefabName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Prefab name of the backpack |

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetBundle'></a>
### AssetBundle `constants`

##### Summary

Asset Bundle containing the Backpack
(Not required if GameObject is provided)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackPackGo'></a>
### BackPackGo `constants`

##### Summary

Backpack GameObject
(Not required if AssetBundle and Prefab Name are provided)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackBiome'></a>
### BackpackBiome `constants`

##### Summary

Provides the configured biomes settings applied to the backpack.
This is flag enum.
(e.g. "BackpackBiomes.Meadows" or "BackpackBiomes.Meadows | BackpackBiomes.BlackForest" to select multiple biomes.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackSizeByQuality'></a>
### BackpackSizeByQuality `constants`

##### Summary

Dictionary of Vector2's that contain the x and y sizing of the backpack at each Quality level'
Dictionary key is the Item's Quality level.
Dictionary value is the Vector2 object.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-CarryBonus'></a>
### CarryBonus `constants`

##### Summary

Provides the additional carry weight bonus applied to backpacks.
For registering a new backpack, this is the default value.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ConfigSection'></a>
### ConfigSection `constants`

##### Summary

Custom Configuration Section Name in Adventure Backpacks.
If left empty, will use default section name.
(e.g. Backpack: {$backpack_itemname} )

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-CraftingTable'></a>
### CraftingTable `constants`

##### Summary

Name of Crafting Table that the Backpack can be crafted at.
If left empty, will disable crafting at any table.
Can also include custom table names.
(e.g. piece_workbench, forge, piece_stonecutter)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-DropsFrom'></a>
### DropsFrom `constants`

##### Summary

List of Ingredients for Upgrading.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EffectsToApply'></a>
### EffectsToApply `constants`

##### Summary

Dictionary of StatusEffect's to apply to the backpack when equipped.
Dictionary Key is the BackpackBiome that needs to be applied to the Backpack for Effect to be activated.
Dictionary Value is a Key Value Pair of a Status Effect to apply:
Key of the KVP is the actual Status Effect
Value of the KVP is the default int Quality level that of the backpack before the effect is applied.
(examples of the quality level: 1 - 4)
Use GetRegisteredStatusEffects() to determine which status effect Adventure Backpacks is aware of.
Use RegisterEffect() first to register new status effects.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EnableFreezing'></a>
### EnableFreezing `constants`

##### Summary

Provides whether the wearer of the backpack will freeze or not.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemName'></a>
### ItemName `constants`

##### Summary

Item Name of the Backpack. Use the $_name localize token.
(e.g. $vapok_mod_rugged_backpack)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemSetStatusEffect'></a>
### ItemSetStatusEffect `constants`

##### Summary

When backpack is equipped, set what the Set Effect Status Effect Should.
If left empty, none will be equipped.
Note: Be sure this is one of the effects included in EffectsToApply.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-MaxRequiredStationLevel'></a>
### MaxRequiredStationLevel `constants`

##### Summary

Max Crafting Station Level to upgrade, and repair.
(e.g. 1, 2, etc.)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-PrefabName'></a>
### PrefabName `constants`

##### Summary

Prefab Name of the Backpack Asset
(Not required if GameObject is provided)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-RecipeIngredients'></a>
### RecipeIngredients `constants`

##### Summary

List of Recipe Ingredients.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-SpeedMod'></a>
### SpeedMod `constants`

##### Summary

Provides the Speed Modification that is applied on the backpack.
For registering a new backpack, this is the default value.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-StationLevel'></a>
### StationLevel `constants`

##### Summary

Minimum Level of Crafting Table Station before Bag can be crafted
(e.g. 1, 2, etc.)

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-UpgradeIngredients'></a>
### UpgradeIngredients `constants`

##### Summary

List of Ingredients for Upgrading.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-WeightMultiplier'></a>
### WeightMultiplier `constants`

##### Summary

Provides the configured weight multiplier that reduces the weight of the items in the backpack.
For registering a new backpack, this is the default value.

<a name='T-AdventureBackpacks-API-ABAPI-DropTarget'></a>
## DropTarget `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Configuration of Drop Target

<a name='M-AdventureBackpacks-API-ABAPI-DropTarget-#ctor-System-String,System-Single,System-Int32,System-Nullable{System-Int32}-'></a>
### #ctor(creature,chance,min,max) `constructor`

##### Summary

Drop Target Constructor

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| creature | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Prefab name of Creature |
| chance | [System.Single](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Single 'System.Single') | Chance to Drop Float |
| min | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Minimum amount to drop. |
| max | [System.Nullable{System.Int32}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.Int32}') | Max amount to drop. |

<a name='F-AdventureBackpacks-API-ABAPI-DropTarget-Chance'></a>
### Chance `constants`

##### Summary

Configured Drop Chance

<a name='F-AdventureBackpacks-API-ABAPI-DropTarget-Creature'></a>
### Creature `constants`

##### Summary

Prefab name of creature

<a name='F-AdventureBackpacks-API-ABAPI-DropTarget-Max'></a>
### Max `constants`

##### Summary

Maximum number of items that can drop.

<a name='F-AdventureBackpacks-API-ABAPI-DropTarget-Min'></a>
### Min `constants`

##### Summary

Min number of items that can drop.

<a name='T-AdventureBackpacks-API-ABAPI-EffectDefinition'></a>
## EffectDefinition `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Create anew EffectDefinition in order to register status effects.

<a name='M-AdventureBackpacks-API-ABAPI-EffectDefinition-#ctor-System-String,System-String,System-String,System-String,StatusEffect-'></a>
### #ctor(name,localizedName,effectName,description,statusEffect) `constructor`

##### Summary

Create anew EffectDefinition in order to register status effects.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| localizedName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| effectName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| statusEffect | [StatusEffect](#T-StatusEffect 'StatusEffect') |  |

<a name='F-AdventureBackpacks-API-ABAPI-EffectDefinition-Description'></a>
### Description `constants`

##### Summary

Description of the Status Effect. Shows up in Configuration.

<a name='F-AdventureBackpacks-API-ABAPI-EffectDefinition-EffectName'></a>
### EffectName `constants`

##### Summary

This is the registered Effect name
(e.g. SetEffect_NecromancyArmor or SE_Demister)

<a name='F-AdventureBackpacks-API-ABAPI-EffectDefinition-LocalizedName'></a>
### LocalizedName `constants`

##### Summary

This is the Localized Translated Effect Name.
This is used in places like the Configuration and in the HUD/GUI
(e.g. "Water Resistance").

<a name='F-AdventureBackpacks-API-ABAPI-EffectDefinition-Name'></a>
### Name `constants`

##### Summary

This is the Effect Name.
(e.g. "$some_effect_name").

<a name='F-AdventureBackpacks-API-ABAPI-EffectDefinition-StatusEffect'></a>
### StatusEffect `constants`

##### Summary

This is your actual Status Effect from your own asset bundle or from another source.
As long as it's of the type SE_Stats, you can use it.

<a name='T-AdventureBackpacks-API-ABAPI-RecipeIngredient'></a>
## RecipeIngredient `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Defines a Recipe Ingredient

<a name='M-AdventureBackpacks-API-ABAPI-RecipeIngredient-#ctor-System-String,System-Int32-'></a>
### #ctor(itemPrefabName,quantity) `constructor`

##### Summary

Create Ingredient Object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| itemPrefabName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Item Prefab Name |
| quantity | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Amount of item to consume. |

<a name='F-AdventureBackpacks-API-ABAPI-RecipeIngredient-ItemPrefabName'></a>
### ItemPrefabName `constants`

##### Summary

Prefab Name of Item to include as a recipe ingredient

<a name='F-AdventureBackpacks-API-ABAPI-RecipeIngredient-Quantity'></a>
### Quantity `constants`

##### Summary

Amount of Item required.
