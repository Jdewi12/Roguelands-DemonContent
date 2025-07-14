using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DemonContent.Patches
{
    [HarmonyPatch(typeof(ObjectScript))]
    [HarmonyPatch(nameof(ObjectScript.Die))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_ObjectScript_Die
    {
        [HarmonyPrefix]
        public static void Prefix(ObjectScript __instance)
        {
            // 0-50: ore
            // 51-100: tree
            // 101-150: plant
            // 151- 200: bugspot
            if (__instance.id == DemonContent.SoulWisp.GetID())
            {
                Menuu.characterStat[6]++; // bugspots harvested
            }
            else if (__instance.id == DemonContent.DemonStone.GetID())
            {
                Menuu.characterStat[5]++; // ore harvested
            }
            // Menuu.characterStat[4] is trees/plants harvested.
        }
    }
}
