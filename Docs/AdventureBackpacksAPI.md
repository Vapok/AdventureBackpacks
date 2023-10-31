<a name='assembly'></a>
# AdventureBackpacksAPI

## Contents

- [ABAPI](#T-AdventureBackpacks-API-ABAPI 'AdventureBackpacks.API.ABAPI')
  - [CanOpenBackpack(player)](#M-AdventureBackpacks-API-ABAPI-CanOpenBackpack-Player- 'AdventureBackpacks.API.ABAPI.CanOpenBackpack(Player)')
  - [GetActiveBackpackStatusEffects()](#M-AdventureBackpacks-API-ABAPI-GetActiveBackpackStatusEffects 'AdventureBackpacks.API.ABAPI.GetActiveBackpackStatusEffects')
  - [GetBackpack(itemData)](#M-AdventureBackpacks-API-ABAPI-GetBackpack-ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.GetBackpack(ItemDrop.ItemData)')
  - [GetEquippedBackpack(player)](#M-AdventureBackpacks-API-ABAPI-GetEquippedBackpack-Player- 'AdventureBackpacks.API.ABAPI.GetEquippedBackpack(Player)')
  - [IsBackpack(itemData)](#M-AdventureBackpacks-API-ABAPI-IsBackpack-ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.IsBackpack(ItemDrop.ItemData)')
  - [IsBackpackEquipped(player)](#M-AdventureBackpacks-API-ABAPI-IsBackpackEquipped-Player- 'AdventureBackpacks.API.ABAPI.IsBackpackEquipped(Player)')
  - [IsLoaded()](#M-AdventureBackpacks-API-ABAPI-IsLoaded 'AdventureBackpacks.API.ABAPI.IsLoaded')
  - [IsThisBackpackEquipped(player,itemData)](#M-AdventureBackpacks-API-ABAPI-IsThisBackpackEquipped-Player,ItemDrop-ItemData- 'AdventureBackpacks.API.ABAPI.IsThisBackpackEquipped(Player,ItemDrop.ItemData)')
  - [OpenBackpack(player,gui)](#M-AdventureBackpacks-API-ABAPI-OpenBackpack-Player,InventoryGui- 'AdventureBackpacks.API.ABAPI.OpenBackpack(Player,InventoryGui)')
- [Backpack](#T-AdventureBackpacks-API-ABAPI-Backpack 'AdventureBackpacks.API.ABAPI.Backpack')
  - [Definition](#F-AdventureBackpacks-API-ABAPI-Backpack-Definition 'AdventureBackpacks.API.ABAPI.Backpack.Definition')
  - [Inventory](#F-AdventureBackpacks-API-ABAPI-Backpack-Inventory 'AdventureBackpacks.API.ABAPI.Backpack.Inventory')
  - [ItemData](#F-AdventureBackpacks-API-ABAPI-Backpack-ItemData 'AdventureBackpacks.API.ABAPI.Backpack.ItemData')
  - [Name](#F-AdventureBackpacks-API-ABAPI-Backpack-Name 'AdventureBackpacks.API.ABAPI.Backpack.Name')
- [BackpackBiomes](#T-AdventureBackpacks-API-BackpackBiomes 'AdventureBackpacks.API.BackpackBiomes')
  - [BlackForest](#F-AdventureBackpacks-API-BackpackBiomes-BlackForest 'AdventureBackpacks.API.BackpackBiomes.BlackForest')
  - [Meadows](#F-AdventureBackpacks-API-BackpackBiomes-Meadows 'AdventureBackpacks.API.BackpackBiomes.Meadows')
  - [Mistlands](#F-AdventureBackpacks-API-BackpackBiomes-Mistlands 'AdventureBackpacks.API.BackpackBiomes.Mistlands')
  - [Mountains](#F-AdventureBackpacks-API-BackpackBiomes-Mountains 'AdventureBackpacks.API.BackpackBiomes.Mountains')
  - [Necromancy](#F-AdventureBackpacks-API-BackpackBiomes-Necromancy 'AdventureBackpacks.API.BackpackBiomes.Necromancy')
  - [None](#F-AdventureBackpacks-API-BackpackBiomes-None 'AdventureBackpacks.API.BackpackBiomes.None')
  - [Plains](#F-AdventureBackpacks-API-BackpackBiomes-Plains 'AdventureBackpacks.API.BackpackBiomes.Plains')
  - [Swamp](#F-AdventureBackpacks-API-BackpackBiomes-Swamp 'AdventureBackpacks.API.BackpackBiomes.Swamp')
- [BackpackDefinition](#T-AdventureBackpacks-API-ABAPI-BackpackDefinition 'AdventureBackpacks.API.ABAPI.BackpackDefinition')
  - [AssetFolderName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetFolderName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.AssetFolderName')
  - [AssetName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.AssetName')
  - [BackpackBiome](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackBiome 'AdventureBackpacks.API.ABAPI.BackpackDefinition.BackpackBiome')
  - [BackpackSizeByQuality](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackSizeByQuality 'AdventureBackpacks.API.ABAPI.BackpackDefinition.BackpackSizeByQuality')
  - [CarryBonus](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-CarryBonus 'AdventureBackpacks.API.ABAPI.BackpackDefinition.CarryBonus')
  - [EnableFreezing](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EnableFreezing 'AdventureBackpacks.API.ABAPI.BackpackDefinition.EnableFreezing')
  - [ItemName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.ItemName')
  - [PrefabName](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-PrefabName 'AdventureBackpacks.API.ABAPI.BackpackDefinition.PrefabName')
  - [SpeedMod](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-SpeedMod 'AdventureBackpacks.API.ABAPI.BackpackDefinition.SpeedMod')
  - [WeightMultiplier](#F-AdventureBackpacks-API-ABAPI-BackpackDefinition-WeightMultiplier 'AdventureBackpacks.API.ABAPI.BackpackDefinition.WeightMultiplier')
- [DropTarget](#T-AdventureBackpacks-API-ABAPI-DropTarget 'AdventureBackpacks.API.ABAPI.DropTarget')
  - [Chance](#F-AdventureBackpacks-API-ABAPI-DropTarget-Chance 'AdventureBackpacks.API.ABAPI.DropTarget.Chance')
  - [Creature](#F-AdventureBackpacks-API-ABAPI-DropTarget-Creature 'AdventureBackpacks.API.ABAPI.DropTarget.Creature')
  - [Max](#F-AdventureBackpacks-API-ABAPI-DropTarget-Max 'AdventureBackpacks.API.ABAPI.DropTarget.Max')
  - [Min](#F-AdventureBackpacks-API-ABAPI-DropTarget-Min 'AdventureBackpacks.API.ABAPI.DropTarget.Min')

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

<a name='F-AdventureBackpacks-API-BackpackBiomes-BlackForest'></a>
### BlackForest `constants`

##### Summary

Black Forest Backpack Effects

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

<a name='F-AdventureBackpacks-API-BackpackBiomes-Necromancy'></a>
### Necromancy `constants`

##### Summary

Special Biome configured for Cheb's Necromancy

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

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetFolderName'></a>
### AssetFolderName `constants`

##### Summary

Folder containing the Asset Bundle

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-AssetName'></a>
### AssetName `constants`

##### Summary

Asset Bundle Name

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-BackpackBiome'></a>
### BackpackBiome `constants`

##### Summary

Provides the configured biomes settings applied to the backpack.

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

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-EnableFreezing'></a>
### EnableFreezing `constants`

##### Summary

Provides whether the wearer of the backpack will freeze or not.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-ItemName'></a>
### ItemName `constants`

##### Summary

Item Name of the Backpack. Use the $_name localize token.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-PrefabName'></a>
### PrefabName `constants`

##### Summary

Prefab Name of the Backpack Asset

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-SpeedMod'></a>
### SpeedMod `constants`

##### Summary

Provides the Speed Modification that is applied on the backpack.

<a name='F-AdventureBackpacks-API-ABAPI-BackpackDefinition-WeightMultiplier'></a>
### WeightMultiplier `constants`

##### Summary

Provides the configured weight multiplier that reduces the weight of the items in the backpack.

<a name='T-AdventureBackpacks-API-ABAPI-DropTarget'></a>
## DropTarget `type`

##### Namespace

AdventureBackpacks.API.ABAPI

##### Summary

Configuration of Drop Target

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
