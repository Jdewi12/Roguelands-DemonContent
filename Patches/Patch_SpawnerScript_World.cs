using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DemonContent.Patches
{
    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch(nameof(SpawnerScript.World))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_SpawnerScript_World
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            Patch_Chunk_SpawnBiomeSlot.HasSpawnedFireDemon = false;
        }
    }
}
