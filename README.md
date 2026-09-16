<div align="center">

# 🎒 Adventure Backpacks

### *A progression-based adventuring backpack mod for Valheim.*

[![GitHub Release](https://img.shields.io/github/v/release/Vapok/AdventureBackpacks?include_prereleases&logo=github&style=for-the-badge)](https://github.com/Vapok/AdventureBackpacks/releases)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Vapok/AdventureBackpacks?logo=thunderstore&style=for-the-badge)](https://thunderstore.io/c/valheim/p/Vapok/AdventureBackpacks/)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-Available-da8e35?logo=nexusmods&style=for-the-badge)](https://www.nexusmods.com/valheim/mods/2204)
<br>
[![Discord](https://img.shields.io/badge/Discord-Join%20Community-7289da?logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/5YAJkRFBXt)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

</div>

Starting as a wee Viking rummaging through the tranquil fields of the Meadows, you'll gather materials that lead to your destiny. From Deer Hide and Leather Scraps to mystical Wisp-infused fabrics, you'll learn how to craft, upgrade, and customize your very own **Adventure Backpacks**! Go forth and wander, ye wanderer of the wanders!

---

<div align="center">

<br>

[![Survival Servers](https://raw.githubusercontent.com/Vapok/AdventureBackpacks/main/images/survivalservers_banner.png)](https://www.survivalservers.com/services/game_servers/valheim/?ref=vapok)

</div>

## 🧭 How to Use Adventure Backpacks

* 🔨 **Discover & Craft**: As you explore biomes, defeat creatures, and collect new materials, you will unlock recipes for Adventuring Backpacks.
* 🎒 **Open Your Backpack**: The default hotkey is <kbd>I</kbd> to open your equipped backpack. Fully customizable for Keyboard, Mouse, and Gamepad / Controller inputs.
* 📈 **Progression & Upgrades**: Each backpack features unique inventory sizing, carry weight bonuses, movement speed modifiers, and environmental perks. Upgrading your backpack at a workbench or forge expands capacity and strengthens bonuses.
* ⚙️ **Full Customization**: Almost every aspect of these backpacks (sizes, recipes, drop chances, effects, and weight multipliers) can be tailored via configuration files or the in-game Configuration Manager.

---

## 📦 Backpack Catalogue

| Backpack | Biome / Tier | Description & Key Traits |
| :--- | :--- | :--- |
| **Satchel** | 🌸 Meadows | *A small, lightweight starter pack crafted from early Meadow materials.* |
| **Rugged Backpack** | 🌲 Black Forest | *A sturdy leather pack complete with brass buckles and fine strapping.* |
| **Bloodbag Wetpack** | 🐸 Swamp | *A durable Swamp pack sealed with bloodbags; grants the **Waterproof** perk.* |
| **Arctic Sherpa Pack** | ❄️ Mountain | *An insulated Mountain pack fit for freezing treks; grants **Frost & Cold Resistance**.* |
| **Lox Hide Knappsack** | 🌾 Plains | *A massive Plains pack made from thick, durable lox hide with high carry capacity.* |
| **Explorers Wisppack** | 🌫️ Mistlands | *A mystical backpack with a Box of Holding, built-in **Demister / Wisplight**, and **Slow Fall**.* |
| **Ashlands Pack** | 🔥 Ashlands | *🔥 Coming Soon — Forged for the volcanic fires and molten depths.* |
| **Deep North Pack** | 🧊 Deep North | *🧊 Coming Soon — Prepared for the sub-zero permafrost.* |
| **Legacy Packs** | ⚔️ Legacy | *Original Iron and Silver backpacks preserved for existing world saves.* |

<blockquote style="background-color: rgba(56, 139, 253, 0.08); border-left: 4px solid #388bfd; padding: 12px 16px; margin: 16px 0; border-radius: 4px;">
  <p style="margin: 0 0 6px 0;">💡 <strong>Coming Soon: Ashlands &amp; Deep North Backpacks</strong></p>
  <p style="margin: 0;">New biome-specific backpacks for the <strong>Ashlands</strong> and <strong>Deep North</strong> are actively in development, featuring custom models, materials, and unique biome status effects!</p>
</blockquote>

---

## 🕹️ Controls & Interaction Modes

| Control Mode | Input / Keybind | Description |
| :--- | :--- | :--- |
| **Toggle Backpack** | <kbd>I</kbd> *(Default)* | Opens or closes your equipped backpack inventory. |
| **Hover Interaction** | *Hover + Hotkey* | Hover over an equipped backpack in your main inventory and press the hotkey to open it directly. |
| **Open with Inventory** | *Configurable* | Automatically opens your backpack inventory whenever you open your player inventory. |
| **Outward Quick Drop** | <kbd>Y</kbd> *(Default)* | Press while moving to immediately detach and drop your backpack behind you to escape danger. |
| **Gamepad / Controller** | 🎮 Native | Full controller navigation support for opening, closing, and managing backpack inventories. |

---

## 🛡️ Advanced Mechanics & Safeguards

* ⚡ **Thor's Inventory Guard (Inception Prevention)**:
  * Backpack-in-backpack nesting is strictly prevented to safeguard against data corruption and infinite inventory loops.
* 🏷️ **Yard Sale Overflow Safeguard**:
  * If a backpack is resized or upgraded in a way that reduces slots below the stored item count, excess items are cleanly dropped at the player's feet rather than lost.
* 🔀 **Right-Click Quick Transfer (Fast Item Transfer)**:
  * Move single items or entire stacks between your inventory and any open backpack or container with a single right-click.
* 🌀 **Portal & Teleportation Compatibility**:
  * Fully respects Valheim's `Inventory.IsTeleportable()` checks. Non-teleportable ores/items inside backpacks will properly restrict portal usage across all portal mods.
* 🗝️ **Key & Quest Item Recognition**:
  * Keys (such as Swamp Crypt Keys) and quest items in your **equipped backpack** are recognized by locked doors and crypt entrances without moving them to player inventory.
* 🖼️ **Armor Stand & Item Stand Mounting**:
  * Safely mount backpacks on Item Stands and Armor Stands for base decoration without item loss or duplication.

---

## ✨ Status Effects & Passive Perks

* 💪 **Carry Weight Modification**: Increases base carrying capacity per upgrade level.
* ⚖️ **Weight Reduction Multiplier**: Configures how much weight items inside the backpack contribute *(0% = weightless contents; 100% = normal weight)*.
* 🏃 **Speed Modifiers**: Configurable movement speed adjustments per tier that improve as packs are upgraded.
* ❄️ **Frost & Cold Resistance**: Keeps you warm in freezing Mountain biomes.
* 🌧️ **Waterproof**: Prevents the "Wet" debuff in rain and swamp water.
* 🦹 **Troll Armor Set Synergy**: Integrates into the Sneak set bonus when wearing Troll Leather gear.
* 🪶 **Slow Fall / Feather Fall**: Eliminates fatal fall damage when leaping from mountain peaks.
* 💡 **Demister (Wisplight)**:
  * Clears mist in the Mistlands.
  * Configurable toggle keybind (default <kbd>L</kbd>).
  * Includes smart biome logic to automatically stow the light outside the Mistlands.

---

## 🧩 Adventure Backpacks API (ABAPI)

Mod developers can easily build custom backpacks and register effects using our dedicated API assembly:

* 📖 [ABAPI Documentation](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)
* 💾 [Download ABAPI.DLL Releases](https://github.com/Vapok/AdventureBackpacks/releases)

### API Capabilities:
* 🎒 Register custom 3D backpack prefabs, textures, and recipes.
* 🪄 Register and attach custom status effects.
* 🔍 Query backpack state, equipped equipment, and inspect backpack contents.

---

## 🤝 Verified Mod Compatibility

<div align="center">

| Mod | Compatibility Status |
| :--- | :--- |
| **AzuCraftyBoxes & AzuAutoStore** | 🟢 Fully Supported (Crafting & auto-storing from equipped backpack) |
| **Valheim Plus (CraftFromChest)** | 🟢 Fully Supported (Crafting calculation stack-interception) |
| **Epic Loot** | 🟢 Fully Supported (Loot tables, enchanting, and weightless modifiers) |
| **Cheb's Necromancy** | 🟢 Fully Supported (Includes *Spectral Shroud of Holding*) |
| **ContentsWithin** | 🟢 Fully Supported (Backpack contents obfuscation) |
| **ZenUI / ZenDragon ModLib** | 🟢 Fully Supported (Defensive UI transpiler hooks) |
| **Quick Stack Store / Auto Split Stack** | 🟢 Fully Supported |
| **Multi-User-Chests (MUC)** | 🟢 Fully Supported |
| **Extra Slots / Equipment & Quick Slots** | 🟢 Fully Supported |
| **Jewelcrafting** | 🟢 Fully Supported |
| **Smoothbrain's Mods (Blacksmithing, etc.)** | 🟢 Fully Supported |

</div>

<blockquote style="background-color: rgba(235, 87, 87, 0.08); border-left: 4px solid #eb5757; padding: 12px 16px; margin: 16px 0; border-radius: 4px;">
  <p style="margin: 0;"><strong>⚠️ NOTE — JotunnBackpacks Incompatibility:</strong> Adventure Backpacks is incompatible with JotunnBackpacks. However, Adventure Backpacks will automatically convert existing JotunnBackpacks saves into new Adventure Backpacks upon loading your character.</p>
</blockquote>

---

## 🌐 Available Translations

Adventure Backpacks includes community translations for:

<div align="center">

🇨🇳 **Chinese (Simplified & Traditional)** • 🇨🇿 **Czech** • 🇺🇸 **English** • 🇫🇷 **French** • 🇩🇪 **German**  
🇯🇵 **Japanese** • 🇰🇷 **Korean** • 🇳🇴 **Norwegian** • 🇵🇱 **Polish** • 🇧🇷 **Portuguese (BR)**  
🇷🇺 **Russian** • 🇪🇸 **Spanish** • 🇸🇪 **Swedish** • 🇹🇷 **Turkish** • 🇺🇦 **Ukrainian**

</div>

*Don't see your language? We welcome community translations! Please submit a PR on [GitHub](https://github.com/Vapok/AdventureBackpacks) or stop by our [Discord](https://discord.gg/5YAJkRFBXt).*

---

## 📥 Installation & Server Setup

### Mod Manager (Recommended)
1. Install via **R2ModMan** or **Thunderstore Mod Manager**.
2. Dependencies are automatically installed: `BepInExPack`, `Jotunn (JVL)`, and `YamlDotNet`.

### Dedicated Servers
* **Required on Both Client & Server**: Adventure Backpacks must be present on both the server and all connecting clients.
* **Network Compatibility**: Built-in version checking ensures game-state and inventory consistency across clients.
* **ServerSync**: Server configuration files automatically lock and sync settings down to non-admin players in real-time.

---

<div align="center">

### 👨‍💻 Created by Vapok Gaming

[![Vapok Gaming](https://avatars.githubusercontent.com/u/1264136?s=120&v=4)](https://github.com/Vapok)

**Author**: [Vapok](https://github.com/Vapok)  
**Source Code**: [GitHub Repository](https://github.com/Vapok/AdventureBackpacks)  
**Community & Support**: [Discord Server](https://discord.gg/5YAJkRFBXt)  
**Changelog**: [Release Notes](https://github.com/Vapok/AdventureBackpacks/blob/main/CHANGELOG.md)

</div>
