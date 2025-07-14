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
    [HarmonyPatch(nameof(PlayerScript.TD))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_PlayerScript_TD
    {
        public static MethodInfo RefreshModsMethod = typeof(GameScript).GetMethod("RefreshMODS", BindingFlags.NonPublic | BindingFlags.Instance);
        public static IEnumerator RefreshMods(float delay)
        {
            yield return new WaitForSeconds(delay);
            RefreshModsMethod.Invoke(InstanceTracker.GameScript, new object[0]);
        }

        public static Coroutine RefreshingCoroutine = null;

        public static FieldInfo InventoryField = typeof(GameScript).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance); 
        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int dmg)
        {
            if (!__instance.GetComponent<NetworkView>().isMine)
                return;
            if (GameScript.equippedIDs[1] == DemonContent.SoulShield.GetID()) 
            {
                int maxReduction = GameScript.mana;
                int reduction;
                if (maxReduction * 2 > dmg)
                {
                    reduction = dmg / 2;
                }
                else
                {
                    reduction = maxReduction;
                }
                dmg -= reduction;
                GameScript.mana -= reduction;
                InstanceTracker.GameScript.UpdateMana();
            }
            var inventory = (Item[])InventoryField.GetValue(InstanceTracker.GameScript);
            if (inventory[36 + 4].id == DemonContent.SoulRing.GetID() ||
                inventory[36 + 5].id == DemonContent.SoulRing.GetID())
            {
                if (!GameScript.combatMode)
                {
                    dmg -= 8;
                    if (dmg < 0)
                        dmg = 0;
                }
            }
            if (inventory[36 + 4].id == DemonContent.DemonRing.GetID() ||
                inventory[36 + 5].id == DemonContent.DemonRing.GetID())
            {
                GameScript.MODS[17] = 40; // dash speed+
                GameScript.MODS[18] = 35; // jump height+
                GameScript.MODS[16] = 35; // move speed+

                if (RefreshingCoroutine != null)
                    InstanceTracker.GameScript.StopCoroutine(RefreshingCoroutine);
                RefreshingCoroutine = InstanceTracker.GameScript.StartCoroutine(RefreshMods(delay: 1.6f));
                
            }

        }
    }
}
