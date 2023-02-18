using System.Collections.Generic;
using System.Linq;
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
    if (other == null)
    {
      AdventureBackpacks.Log.Debug($"other HashSet argument is null. Expecting initialized object.");
      other = new();
    }

    if (Player.m_localPlayer == null)
    {
      AdventureBackpacks.Log.Debug($"Add Active Effects Started... Player null");
      return other;
    }
    
    var player = Player.m_localPlayer;
          
    activeEffects = new HashSet<StatusEffect>();
    effectDemister = effectDemister == null ? ObjectDB.instance.GetStatusEffect("Demister") : effectDemister;
    effectSlowfall = effectSlowfall == null ? ObjectDB.instance.GetStatusEffect("SlowFall") : effectSlowfall;

    if (Demister.ShouldHaveDemister(player))
      activeEffects.Add(effectDemister);
    
    if (FeatherFall.ShouldHaveFeatherFall(player))
      activeEffects.Add(effectSlowfall);
          
    other.UnionWith(activeEffects);
    
    AdventureBackpacks.Log.Debug($"Adding {other.Count} Active Backpack Effects.");
    return other;
  }

  
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
    public static class HumanoidUpdateEquipmentStatusEffectsPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
          var instrs = instructions.ToList();

          var counter = 0;

          CodeInstruction LogMessage(CodeInstruction instruction)
          {
            AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
            return instruction;
          }

          var ldlocInstruction = new CodeInstruction(OpCodes.Ldloc_0); 

          for (int i = 0; i < instrs.Count; ++i)
          {

            yield return LogMessage(instrs[i]);
            counter++;

            //In Humanoid.UpdateEquipmentStatusEffects, Local Variable 0, or loc_0 is the target HashSet variable 
            //listed as "other".  So, patching after Stloc_0 is called immediately after Newobj, which creates the object.
            if (instrs[i].opcode == OpCodes.Stloc_0 && instrs[i-1].opcode == OpCodes.Newobj)
            {
              //Move Any Labels from the instruction position being patched to new instruction.
              if (instrs[i].labels.Count > 0)
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