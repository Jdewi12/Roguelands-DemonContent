using GadgetCore;
using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DemonContent.Patches
{
    [HarmonyPatch()]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_Patch_GameScript_UseSkill_Prefix
    {
        [HarmonyTargetMethod]
        public static MethodInfo TargetMethod() 
            => typeof(GadgetCoreAPI).Assembly.GetType("GadgetCore.Patches.Patch_GameScript_UseSkill")
            .GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);

        [HarmonyPrefix]
        public static void Prefix() => Patch_GameScript_UseSkill.Prefix();

        [HarmonyPostfix]
        public static void PostFix() => Patch_GameScript_UseSkill.PostFix(InstanceTracker.GameScript);
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch(nameof(GameScript.UseSkill))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_GameScript_UseSkill
    {
        public static int CachedMana;
        [HarmonyPrefix]
        public static void Prefix()
        {
            CachedMana = GameScript.mana;
        }

        [HarmonyPostfix]
        public static void PostFix(GameScript __instance)
        {
            // only continue if we have the Shadow Armor equipped
            if (GameScript.equippedIDs[3] != DemonContent.ShadowArmor.GetID())
                return;

            int manaSpent = CachedMana - GameScript.mana;
            if(manaSpent > 0) // spent mana
            {
                GameScript.mana = CachedMana; // undo the cost
                __instance.UpdateMana();
                GameScript.hp -= manaSpent;
                InstanceTracker.PlayerScript.GetComponent<NetworkView>().RPC("TDTEXT", RPCMode.All, new object[] { manaSpent });
                if (GameScript.hp <= 0)
                {
                    GameScript.dead = true;
                    GameScript.hp = 0;
                    __instance.Die();
                }
                __instance.UpdateHP();
            }

        }
    }
}
