using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AdventureBackpacks.Extensions;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace AdventureBackpacks.Features;

//Console commands that dump game data to BepInEx/AdventureBackpacksDump. Run from a loaded world.
[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
internal static class DevDump
{
    private static string OutputDir => Path.Combine(Paths.BepInExRootPath, "AdventureBackpacksDump");

    [HarmonyPrepare]
    private static bool Prepare() => !PlayerExtensions.IsDedicatedOrHeadless();

    private static void Postfix()
    {
        Register("ab_dump_all", "Write every AdventureBackpacks dump file", _ =>
        {
            DumpStatusEffects(); DumpCapes(); DumpItems(); DumpRecipes(); DumpStations(); DumpCreatures();
        });
        Register("ab_dump_se", "Status effects with SE_Stats fields", _ => DumpStatusEffects());
        Register("ab_dump_capes", "Shoulder items with stats and recipes", _ => DumpCapes());
        Register("ab_dump_items", "Every item prefab", _ => DumpItems());
        Register("ab_dump_recipes", "Every recipe", _ => DumpRecipes());
        Register("ab_dump_stations", "Crafting stations", _ => DumpStations());
        Register("ab_dump_creatures", "Creatures with drop tables", _ => DumpCreatures());
    }

    private static void Register(string name, string description, Action<Terminal.ConsoleEventArgs> action)
    {
        new Terminal.ConsoleCommand(name, description, args =>
        {
            if (ObjectDB.instance == null)
            {
                args.Context.AddString("Load a world first.");
                return;
            }

            try
            {
                Directory.CreateDirectory(OutputDir);
                action(args);
                args.Context.AddString($"Written to {OutputDir}");
            }
            catch (Exception ex)
            {
                args.Context.AddString($"{name} failed: {ex.Message}");
                AdventureBackpacks.Log?.Error($"{name}: {ex}");
            }
        });
    }

    private static void WriteFile(string fileName, StringBuilder content)
    {
        File.WriteAllText(Path.Combine(OutputDir, fileName), content.ToString());
        AdventureBackpacks.Log?.Info($"Dumped {fileName}");
    }

    private static string Loc(string text) => string.IsNullOrEmpty(text) ? "" : Localization.instance?.Localize(text) ?? text;

    private static string Name(UnityEngine.Object obj) => obj != null ? obj.name : "";

    private static string Mods(IEnumerable<HitData.DamageModPair> mods) =>
        mods == null ? "" : string.Join(",", mods.Select(m => $"{m.m_type}:{m.m_modifier}"));

    //Skip defaults so new SE_Stats fields show up on their own.
    private static string NonDefaultFields(object target)
    {
        var parts = new List<string>();
        foreach (var field in target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = field.GetValue(target);
            switch (value)
            {
                case float f when Mathf.Abs(f) > 0.0001f && !(field.Name.EndsWith("Multiplier") && Mathf.Approximately(f, 1f)):
                    parts.Add($"{field.Name}={f}");
                    break;
                case int i when i != 0:
                    parts.Add($"{field.Name}={i}");
                    break;
                case bool b when b:
                    parts.Add($"{field.Name}=true");
                    break;
                case List<HitData.DamageModPair> mods when mods.Count > 0:
                    parts.Add($"{field.Name}=[{Mods(mods)}]");
                    break;
                case UnityEngine.Object o when o != null && field.FieldType != typeof(Sprite) && !typeof(EffectList).IsAssignableFrom(field.FieldType):
                    parts.Add($"{field.Name}={o.name}");
                    break;
            }
        }
        return string.Join(" ", parts);
    }

    private static void DumpStatusEffects()
    {
        var sb = new StringBuilder("prefab\tclass\tname\tttl\tfields\ttooltip\n");
        foreach (var se in ObjectDB.instance.m_StatusEffects.Where(x => x != null).OrderBy(x => x.name))
        {
            sb.Append(se.name).Append('\t').Append(se.GetType().Name).Append('\t').Append(Loc(se.m_name)).Append('\t')
              .Append(se.m_ttl).Append('\t').Append(NonDefaultFields(se)).Append('\t')
              .Append(Loc(se.m_tooltip).Replace("\n", " | ")).Append('\n');
        }
        WriteFile("StatusEffects.txt", sb);
    }

    private static IEnumerable<Recipe> RecipesFor(GameObject prefab) =>
        ObjectDB.instance.m_recipes.Where(r => r != null && r.m_item != null && r.m_item.gameObject == prefab);

    private static string Requirements(Piece.Requirement[] resources) =>
        resources == null ? "" : string.Join(",", resources.Where(r => r.m_resItem != null)
            .Select(r => $"{r.m_resItem.name}x{r.m_amount}(+{r.m_amountPerLevel}/lvl)"));

    private static void DumpCapes()
    {
        var sb = new StringBuilder();
        foreach (var go in ObjectDB.instance.m_items.Where(x => x != null).OrderBy(x => x.name))
        {
            var drop = go.GetComponent<ItemDrop>();
            var shared = drop?.m_itemData?.m_shared;
            if (shared == null || shared.m_itemType != ItemDrop.ItemData.ItemType.Shoulder) continue;

            sb.Append("== ").Append(go.name).Append(" (").Append(Loc(shared.m_name)).Append(")\n");
            sb.Append("  set=").Append(shared.m_setName).Append('/').Append(shared.m_setSize)
              .Append(" setSE=").Append(Name(shared.m_setStatusEffect))
              .Append(" equipSE=").Append(Name(shared.m_equipStatusEffect))
              .Append(" fullAdrenalineSE=").Append(Name(shared.m_fullAdrenalineSE))
              .Append(" maxAdrenaline=").Append(shared.m_maxAdrenaline).Append('\n');
            sb.Append("  armor=").Append(shared.m_armor).Append("+").Append(shared.m_armorPerLevel).Append("/lvl")
              .Append(" maxQuality=").Append(shared.m_maxQuality).Append(" weight=").Append(shared.m_weight).Append('\n');
            sb.Append("  movement=").Append(shared.m_movementModifier)
              .Append(" heat=").Append(shared.m_heatResistanceModifier)
              .Append(" eitrRegen=").Append(shared.m_eitrRegenModifier)
              .Append(" stamina[attack=").Append(shared.m_attackStaminaModifier)
              .Append(" block=").Append(shared.m_blockStaminaModifier)
              .Append(" dodge=").Append(shared.m_dodgeStaminaModifier)
              .Append(" run=").Append(shared.m_runStaminaModifier)
              .Append(" jump=").Append(shared.m_jumpStaminaModifier)
              .Append(" swim=").Append(shared.m_swimStaminaModifier)
              .Append(" sneak=").Append(shared.m_sneakStaminaModifier)
              .Append(" home=").Append(shared.m_homeItemsStaminaModifier).Append("]\n");
            sb.Append("  damageMods=").Append(Mods(shared.m_damageModifiers)).Append('\n');
            foreach (var recipe in RecipesFor(go))
            {
                sb.Append("  recipe: station=").Append(Name(recipe.m_craftingStation)).Append(" lvl=").Append(recipe.m_minStationLevel)
                  .Append(" ").Append(Requirements(recipe.m_resources)).Append('\n');
            }
        }
        WriteFile("Capes.txt", sb);
    }

    private static void DumpItems()
    {
        var sb = new StringBuilder("prefab\ttype\tname\tweight\tmaxStack\tteleportable\n");
        foreach (var go in ObjectDB.instance.m_items.Where(x => x != null).OrderBy(x => x.name))
        {
            var shared = go.GetComponent<ItemDrop>()?.m_itemData?.m_shared;
            if (shared == null) continue;
            sb.Append(go.name).Append('\t').Append(shared.m_itemType).Append('\t').Append(Loc(shared.m_name)).Append('\t')
              .Append(shared.m_weight).Append('\t').Append(shared.m_maxStackSize).Append('\t').Append(shared.m_teleportable).Append('\n');
        }
        WriteFile("Items.txt", sb);
    }

    private static void DumpRecipes()
    {
        var sb = new StringBuilder("item\tamount\tstation\tminLevel\trequirements\n");
        foreach (var r in ObjectDB.instance.m_recipes.Where(x => x != null && x.m_item != null).OrderBy(x => x.m_item.name))
        {
            sb.Append(r.m_item.name).Append('\t').Append(r.m_amount).Append('\t').Append(Name(r.m_craftingStation)).Append('\t')
              .Append(r.m_minStationLevel).Append('\t').Append(Requirements(r.m_resources)).Append('\n');
        }
        WriteFile("Recipes.txt", sb);
    }

    private static void DumpStations()
    {
        var sb = new StringBuilder("prefab\tname\n");
        var seen = new HashSet<string>();
        if (ZNetScene.instance != null)
        {
            foreach (var go in ZNetScene.instance.m_prefabs.Where(x => x != null))
            {
                var station = go.GetComponent<CraftingStation>();
                if (station != null && seen.Add(go.name))
                    sb.Append(go.name).Append('\t').Append(Loc(station.m_name)).Append('\n');
            }
        }
        foreach (var r in ObjectDB.instance.m_recipes.Where(x => x != null && x.m_craftingStation != null))
        {
            if (seen.Add(r.m_craftingStation.name))
                sb.Append(r.m_craftingStation.name).Append('\t').Append(Loc(r.m_craftingStation.m_name)).Append("\t(from recipe)\n");
        }
        WriteFile("Stations.txt", sb);
    }

    private static void DumpCreatures()
    {
        var sb = new StringBuilder("prefab\tname\tfaction\tboss\thealth\tdrops\n");
        if (ZNetScene.instance == null)
        {
            sb.Append("ZNetScene not available; load a world.\n");
        }
        else
        {
            foreach (var go in ZNetScene.instance.m_prefabs.Where(x => x != null).OrderBy(x => x.name))
            {
                var character = go.GetComponent<Character>();
                if (character == null) continue;
                var drops = go.GetComponent<CharacterDrop>()?.m_drops;
                var dropText = drops == null ? "" : string.Join(",", drops.Where(d => d.m_prefab != null)
                    .Select(d => $"{d.m_prefab.name}x{d.m_amountMin}-{d.m_amountMax}@{d.m_chance}"));
                sb.Append(go.name).Append('\t').Append(Loc(character.m_name)).Append('\t').Append(character.m_faction).Append('\t')
                  .Append(character.m_boss).Append('\t').Append(character.m_health).Append('\t').Append(dropText).Append('\n');
            }
        }
        WriteFile("Creatures.txt", sb);
    }
}
