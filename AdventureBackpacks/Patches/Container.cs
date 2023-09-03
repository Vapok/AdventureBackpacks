using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace AdventureBackpacks.Patches;

public static class ContainerPatches
{
    [HarmonyPatch(typeof(Container), nameof(Container.TakeAll))]
    static class ContainerTakeAllPatch
    {
        static void Prefix(ArmorStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = true;
        }
        static void Postfix(ArmorStand __instance)
        {
            AdventureBackpacks.BypassMoveProtection = false;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
    static class ContainerInteractPatch
    {
        static bool Prefix(Container __instance, ref bool __result)
        {
            if (__instance.name.Equals("Player(Clone)"))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
    static class ContainerAwakePatch
    {
        static void UpdateZDO(Container instance, ZNetView nview)
        {
            if (instance.name.Equals("Player(Clone)"))
            {
                nview.GetZDO().Set("creator".GetStableHashCode(),1L);
            }
                
        }
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
        {
            var patchedSuccess = false;
            
            var instrs = instructions.ToList();

            var counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                AdventureBackpacks.Log.Debug($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            var invokeRepeatingMethod = AccessTools.DeclaredMethod(typeof(MonoBehaviour), nameof(MonoBehaviour.InvokeRepeating), new []{typeof(string), typeof(float), typeof(float)});
            var nviewField = AccessTools.DeclaredField(typeof(Container),nameof(Container.m_nview));

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 5 && instrs[i].opcode == OpCodes.Ret && instrs[i-1].opcode == OpCodes.Call && instrs[i-1].operand.Equals(invokeRepeatingMethod))
                {
                    //Container this (arg #1)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    counter++;
                    
                    //Container this (for arg #2)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    counter++;
                    
                    //Container Field nview (arg #2)
                    yield return new CodeInstruction(OpCodes.Ldfld,nviewField);
                    counter++;
          
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ContainerAwakePatch), nameof(UpdateZDO))));
                    counter++;

                    patchedSuccess = true;
                }

                yield return LogMessage(instrs[i]);
                counter++;
            }
            
            if (!patchedSuccess)
            {
                AdventureBackpacks.Log.Error($"Container.Awake Transpiler Failed To Patch");
                Thread.Sleep(5000);
            }
        }
    }
}