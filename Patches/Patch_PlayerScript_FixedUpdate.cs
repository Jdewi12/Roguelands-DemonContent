using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DemonContent.Patches
{
    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch(nameof(PlayerScript.FixedUpdate))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_PlayerScript_FixedUpdate
    {

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, Rigidbody ___r, bool ___downing, bool ___upping)
        {
            // If the player is speed boosted by Demon Ring, don't apply speed limits
            if(__instance == InstanceTracker.PlayerScript && Time.time < Patch_PlayerScript_TD.ClearBuffsTime)
            {
                // Some weapons limit fall speed I guess
                if ((GameScript.equippedIDs[0] == 477 || GameScript.equippedIDs[0] == 322) && ___r.velocity.y < -5f && GameScript.combatMode) 
                {
                    ___r.velocity = new Vector3(___r.velocity.x, -4f, 0f);
                }
                if (Menuu.curUniform == 14 && ___r.velocity.y < -10f) // Beehive uniform limits fall speed
                {
                    ___r.velocity = new Vector3(___r.velocity.x, -9f, 0f);
                }
                if (Menuu.curAugment == 20) // Halo augment lets players fly with wasd
                {
                    if (!___downing && ___upping)
                    {
                        ___r.velocity = new Vector3(___r.velocity.x, 0f, 0f);
                    }
                }
                return false; // don't run original
            }
            // else
            return true; // run original
        }

    }
}
