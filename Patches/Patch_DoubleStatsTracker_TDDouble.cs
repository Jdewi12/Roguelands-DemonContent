using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DemonContent.Patches
{
    [HarmonyPatch()]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_DoubleStatsTracker_TDDouble
    {
        public static bool Prepare()
        {
            return Gadgets.GetGadget("BigNumberCore") != null;
        }

        public static MethodBase TargetMethod()
        {
            return Gadgets.GetGadget("BigNumberCore").GetType().Assembly
                .GetType("BigNumberCore.DoubleStatsTracker")
                .GetMethod("TDDouble", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static MethodInfo TDDamageReductionsMethod = typeof(Patch_PlayerScript_TD).GetMethod(nameof(Patch_PlayerScript_TD.ApplyDamageReductionsDouble), BindingFlags.Public | BindingFlags.Static);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Call && instruction.operand.ToString() == "Double BytesToDouble(System.Byte[], Int32)")
                {
                    yield return new CodeInstruction(OpCodes.Call, TDDamageReductionsMethod);
                }
            }
        }
    }
}
