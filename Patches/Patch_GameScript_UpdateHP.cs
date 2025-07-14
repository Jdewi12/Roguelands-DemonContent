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
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch(nameof(GameScript.UpdateHP))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_GameScript_UpdateHP
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reducerMethod = typeof(Patch_GameScript_UpdateHP).GetMethod(nameof(ReduceValue), BindingFlags.Static | BindingFlags.Public);
            List<CodeInstruction> codes = instructions.ToList();
            //  if the player has the demon helm, look for anywhere maxHP is set and reduce it, and reduce any maxHP is greater than n checks
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Stsfld && codes[i].operand.ToString() == "System.Int32 maxhp")
                {
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, reducerMethod));
                    i++;
                }
                // if maxHP <= n (or > n)
                else if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand.ToString() == "System.Int32 maxhp" &&
                    codes[i + 1].opcode == OpCodes.Ldc_I4_S &&
                    codes[i + 2].opcode != null && codes[i + 2].opcode == OpCodes.Ble)
                {
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, reducerMethod));
                }
            }
            return codes;

        }

        public static int ReduceValue(int val)
        {
            const int reduction = 15;
            if (GameScript.equippedIDs[2] == DemonContent.DemonHelm.GetID())
            {
                if (val <= reduction)
                    return 1;
                // else
                return val - reduction;
            }
            // else
            return val;
        }
    }
}
