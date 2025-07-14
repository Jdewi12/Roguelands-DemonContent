using DemonContent.Scripts;
using GadgetCore.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DemonContent.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch(nameof(Chunk.SpawnBiomeSlot))]
    [HarmonyGadget(nameof(DemonContent))]
    public static class Patch_Chunk_SpawnBiomeSlot
    {
        public static bool HasSpawnedFireDemon = false;
        [HarmonyPrefix]
        public static bool Prefix(int a, int i, int mid, Chunk __instance, GameObject[] ___networkStuff, ref int ___temp)
        {
            if (a != 9) // if not Demon's Rift, run original
                return true;

            Transform transform = __instance.spawnSpot[i].transform;
            if (mid == 1)
            {
                transform = __instance.spawnSpotMid[i].transform;
            }

            int rng = UnityEngine.Random.Range(0, 100);
            if(rng < 9) // 9% Ice Demon
            {

                ___networkStuff[___temp] = DemonContent.IceDemon.Spawn(transform.position + new Vector3(0f, 1f, 0f));
                ___temp++;
            }
            else if(!HasSpawnedFireDemon && rng < 19) // 10% Fire Demon but limit 1
            {
                HasSpawnedFireDemon = true;
                ___networkStuff[___temp] = DemonContent.FireDemon.Spawn(transform.position + new Vector3(0f, 1f, 0f));
                ___temp++;
            }
            else if(rng < 24) // 5% chance of chest
            {
                int rng2 = UnityEngine.Random.Range(0, 4);
                if (rng2 < 3) // 3/4 regular
                {
                    ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)Resources.Load("obj/chest"), transform.position, Quaternion.identity, 0);
                }
                else // 1/4 gold chest
                {
                    ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)Resources.Load("obj/chestGold"), transform.position, Quaternion.identity, 0);
                }
                ___temp++;
            }
            else if(rng < 34) // 10% bugspot
            {
                ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)GadgetCoreAPI.GetCustomResource("obj/DemonContent/bugspot9"), transform.position, Quaternion.identity, 0);
                ___temp++;
            }
            else if (rng < 44) // 10% ore
            {
                ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)GadgetCoreAPI.GetCustomResource("obj/DemonContent/ore9"), transform.position, Quaternion.identity, 0);
                ___temp++;
            }
            else if(rng < 53) // 9% poison demon
            {
                ___networkStuff[___temp] = DemonContent.PoisonDemon.Spawn(transform.position);
                ___temp++;
            }
            else if(rng < 65) // 12% hazard
            {
                float yOffset = (rng - 52) / 3f; // 0.33-4
                ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)GadgetCoreAPI.GetCustomResource("haz/DemonContent/haz9"), transform.position + new Vector3(0f, yOffset), Quaternion.identity, 0);
                ___temp++;
            }
            else if(rng < 66) // 1% relic
            {
                if (UnityEngine.Random.Range(0, 3) < 2) // 2/3rds, plus around 1/3rd chance of calling the original which also has a 1% chance
                {
                    ___networkStuff[___temp] = (GameObject)Network.Instantiate((GameObject)Resources.Load("obj/relic"), transform.position, Quaternion.identity, 0);
                    ___temp++;
                }
            }
            else // spawned nothing
            {
                return true; // the original just spawns relics at 1%, but maybe some other mod wants to do something.
            }
            return false;
        }
    }
}
