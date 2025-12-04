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

        public static float ClearBuffsTime = -1;
        public static IEnumerator ClearTempBuffs(float delay)
        {
            ClearBuffsTime = Time.time + delay;
            // Wait until we hit ClearBuffsTime. If ClearTempBuffs is called again before this is done, both coroutines should finish at the same time
            while (Time.time < ClearBuffsTime)
                yield return null;
            RefreshModsMethod.Invoke(InstanceTracker.GameScript, new object[0]);
        }

        [HarmonyPrefix]
        public static void Prefix(ref int dmg)
        {
            dmg = (int)ApplyDamageReductionsDouble(dmg);
        }

        public static Coroutine RefreshingCoroutine = null;
        public static FieldInfo InventoryField = typeof(GameScript).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance);
        // stack neutral and using doubles so we can easily use it for BigNumberCore via transpiler.
        public static double ApplyDamageReductionsDouble(double dmg)
        {
            if (GameScript.equippedIDs[1] == DemonContent.SoulShield.GetID())
            {
                int maxReduction = GameScript.mana;
                int reduction;
                if (maxReduction * 2 > dmg)
                {
                    reduction = (int)(dmg / 2);
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
            int soulRings = 0;
            if (inventory[36 + 4].id == DemonContent.SoulRing.GetID())
                soulRings++;
            if(inventory[36 + 5].id == DemonContent.SoulRing.GetID())
                soulRings++;
            if(soulRings > 0)
            {
                if (!GameScript.combatMode)
                {
                    dmg -= 6 * soulRings;
                    if (dmg < 0)
                        dmg = 0;
                }
            }
            int demonRings = 0;
            if (inventory[36 + 4].id == DemonContent.DemonRing.GetID())
                demonRings++;
            if (inventory[36 + 5].id == DemonContent.DemonRing.GetID())
                demonRings++;
            if(demonRings > 0)
            {
                if (Time.time > ClearBuffsTime) // prevent the effect stacking if hit while still affected by previous effect (timer will reset though)
                {
                    GameScript.MODS[17] += 40 * demonRings; // dash speed+
                    GameScript.MODS[18] += 15 + 20 * demonRings; // jump height+
                    GameScript.MODS[16] += 40 * demonRings; // move speed+
                }

                if (RefreshingCoroutine != null)
                    InstanceTracker.GameScript.StopCoroutine(RefreshingCoroutine);
                var rb = InstanceTracker.PlayerScript.GetComponent<Rigidbody>();
                RefreshingCoroutine = InstanceTracker.GameScript.StartCoroutine(ClearTempBuffs(delay: 1.6f));

            }

            return dmg;
        }
    }
}
