using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace AdventureBackpacks.Assets.Effects;

public static class BackpackEffects
{
    public static HitData.DamageModPair FrostResistance = new() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant};
}

public static class EquipmentEffectCache
{
  private static HashSet<StatusEffect> activeEffects = new();
  private static StatusEffect effectDemister;
  private static StatusEffect effectSlowfall;

  public static HashSet<StatusEffect> AddActiveBackpackEffects(HashSet<StatusEffect> other)
  {
    if (Player.m_localPlayer == null)
    {
      AdventureBackpacks.Log.Debug($"Add Active Effects Started... Player null");
    }
    
    var player = Player.m_localPlayer;
          
    if (other == null)
    {
      AdventureBackpacks.Log.Debug($"Add Active Effects Started... And I suck because other is null.");  
    }
    AdventureBackpacks.Log.Debug($"Add Active Effects Started... Count of Other: {other.Count}");
    activeEffects = new HashSet<StatusEffect>();
          
    effectDemister = effectDemister == null ? ObjectDB.instance.GetStatusEffect("Demister") : effectDemister;
    effectSlowfall = effectSlowfall == null ? ObjectDB.instance.GetStatusEffect("SlowFall") : effectSlowfall;
          
    void EnsureEffectsAdded(StatusEffect se, bool shouldHave)
    {
      if (shouldHave && !activeEffects.Any( x => x.name.Equals(se.name)))
        activeEffects.Add(se);
    }
          
    EnsureEffectsAdded(effectDemister,Demister.ShouldHaveDemister(player));
    EnsureEffectsAdded(effectSlowfall,FeatherFall.ShouldHaveFeatherFall(player));
          
    foreach (var backpackEffect in activeEffects)
    {
      if (!other.Any( x => x.name.Equals(backpackEffect.name)))
        other.Add(backpackEffect);
    }
    
    AdventureBackpacks.Log.Debug($"Add Active Effects Ending... Count of Other: {other.Count}");
    return other;
  }

  
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    public static class HumanoidUpdateEquipmentStatusEffectsPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
          var instrs = instructions.ToList();
          var positionToPatch = 0;
          var lastBrFalsePosition = 0;
          var foundPatchPosition = false;
          
          var equipmentStatusEffectsField = AccessTools.DeclaredField(typeof(Humanoid), nameof(Humanoid.m_eqipmentStatusEffects));
          
          CodeInstruction patchBeforeCodeInstruction = new CodeInstruction(OpCodes.Nop);
          CodeInstruction brFalseCodeInstruction = new CodeInstruction(OpCodes.Nop);
          var brFalseLastLabel = new Label();

          for (int j = 0; j < instrs.Count; ++j)
          {
            if (instrs[j].opcode == OpCodes.Brfalse && instrs[j].operand is Label label)
            {
              lastBrFalsePosition = j;
              brFalseLastLabel = label;
            }
            
            if (instrs[j].opcode == OpCodes.Br)
            {
              for (int k = lastBrFalsePosition; k < j; k++)
              {
                if (instrs[k].labels.Contains(brFalseLastLabel))
                {
                  positionToPatch = k;
                  patchBeforeCodeInstruction = instrs[k];
                  AdventureBackpacks.Log.Debug($"Position Found: {positionToPatch}  Current Position Opcode: {patchBeforeCodeInstruction.opcode} Patched Position Opcode: {instrs[positionToPatch].opcode}");
                  AdventureBackpacks.Log.Warning($"Number of Labels on This Element: {patchBeforeCodeInstruction.labels.Count}");
                  AdventureBackpacks.Log.Warning($"Number of Exception Blocks on This Element: {patchBeforeCodeInstruction.blocks.Count}");
                  foundPatchPosition = true;
                  break;
                }
              }

              if (foundPatchPosition)
                break;
            }
          }

          var counter = 0;

          CodeInstruction LogMessage(CodeInstruction instruction)
          {
            AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
            return instruction;
          }

          var ldlocInstruction = new CodeInstruction(OpCodes.Ldloc_0); 

          for (int i = 0; i < instrs.Count; ++i)
          {
            if (i == positionToPatch && instrs[i] == patchBeforeCodeInstruction)
            {
              //Patch here
              
              //Move Labels from the current This element to new instruction.
              instrs[i].MoveLabelsTo(ldlocInstruction);
              
              //Patch the ldloc_0 which is the argument of my method using local variable 0.
              yield return LogMessage(ldlocInstruction);
              counter++;
              
              //Patch Calling Method
              yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(EquipmentEffectCache), nameof(AddActiveBackpackEffects))));
              counter++;

              //Save output of calling method to local variable 0
              yield return LogMessage(new CodeInstruction(OpCodes.Stloc_0));
              counter++;
            }
            
            yield return LogMessage(instrs[i]);
            counter++;
          }
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), new[] { typeof(string), typeof(bool) })]
    public static class RemoveStatusEffects
    {

      [UsedImplicitly]
      [HarmonyPriority(Priority.First)]
      public static bool Prefix(string name, ref bool __result)
      {
        if (activeEffects == null)
          return true;
          
        if (activeEffects.Any(x => x.name.Equals(name)))
        {
          __result = false;
          return false;
        }

        return true;
      }
    }
}