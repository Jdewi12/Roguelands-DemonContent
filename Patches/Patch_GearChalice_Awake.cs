using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace DemonContent.Patches
{
    [HarmonyPatch(typeof(GearChalice))]
    [HarmonyPatch(nameof(GearChalice.Awake))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_GearChalice_Awake
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == OpCodes.Ldc_I4_S && code.operand.ToString() == "9") // check for biome 9 (Demon's rift)
                    code.operand = -86; // replace with arbitrary negative id that shouldn't be used
                yield return code;
            }
        }
    }
}
