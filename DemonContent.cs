using UnityEngine;
using GadgetCore.API;
using System.Collections.Generic;
using GadgetCore;
using GadgetCore.Util;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using DemonContent.Scripts;
using RecipeMenuCore.API;

namespace DemonContent
{
    [Gadget("DemonContent", RequiredOnClients: true, Dependencies: new string[] { "RecipeMenuCore" })]
    public class DemonContent : Gadget
    {
        public const string MOD_VERSION = "1.5";
        public const string CONFIG_VERSION = "1.0";

        public static GadgetLogger logger;

        public static ItemInfo ShadowFabric;
        public static ItemInfo ShadowEmblem;
        public static ItemInfo DemonStone;
        public static ItemInfo DemonEmblem;
        public static ItemInfo SoulWisp;
        public static ItemInfo SoulEmblem;
        public static ItemInfo DemonHelm;
        public static ItemInfo ShadowArmor;
        public static ItemInfo SoulShield;
        public static ItemInfo DemonLance;
        public static ItemInfo DemonRing;
        public static ItemInfo SoulRing;


        public static EntityInfo IceDemon;
        public static EntityInfo FireDemon;
        public static EntityInfo PoisonDemon;

        public static void Log(string text)
        {
            logger.Log(text);
        }

        protected override void LoadConfig()
        {
            logger = base.Logger;

            Config.Load();

            string fileVersion = Config.ReadString("ConfigVersion", CONFIG_VERSION, comments: "The Config Version (not to be confused with mod version)");

            if (fileVersion != CONFIG_VERSION)
            {
                Config.Reset();
                Config.WriteString("ConfigVersion", CONFIG_VERSION, comments: "The Config Version (not to be confused with mod version)");
            }

            Config.Save();
        }

        protected override void Initialize()
        {
            Logger.Log(Info.Mod.Name + " v" + Info.Mod.Version);

            // materials
            ShadowFabric = new ItemInfo(
                Type: ItemType.LOOT | ItemType.MONSTER,
                Name: "Shadow Fabric",
                Desc: "LOOT - craft into an \nemblem at the Emblem \nForge.",
                Tex: GadgetCoreAPI.LoadTexture2D("ShadowFabric.png"),
                Value: 2);
            ShadowFabric.Register();

            ShadowEmblem = new ItemInfo(
                Type: ItemType.EMBLEM | ItemType.MONSTER,
                Name: "Shadow Emblem",
                Desc: "Used as a crafting \nmaterial in the \nUniversal Crafter",
                Tex: GadgetCoreAPI.LoadTexture2D("ShadowEmblem.png"),
                Value: 20);
            ShadowEmblem.Register();
            GadgetCoreAPI.AddEmblemRecipe(ShadowFabric.GetID(), ShadowEmblem.GetID());

            DemonStone = new ItemInfo(
                Type: ItemType.LOOT | ItemType.ORE,
                Name: "Demon Stone",
                Desc: "LOOT - craft into an \nemblem at the Emblem \nForge.",
                Tex: GadgetCoreAPI.LoadTexture2D("DemonStone.png"),
                Value: 2);
            DemonStone.Register();

            DemonEmblem = new ItemInfo(
                Type: ItemType.EMBLEM | ItemType.MONSTER,
                Name: "Demon Emblem",
                Desc: "Used as a crafting \nmaterial in the \nUniversal Crafter",
                Tex: GadgetCoreAPI.LoadTexture2D("DemonEmblem.png"),
                Value: 20);
            DemonEmblem.Register();
            GadgetCoreAPI.AddEmblemRecipe(DemonStone.GetID(), DemonEmblem.GetID());

            SoulWisp = new ItemInfo(
                Type: ItemType.LOOT | ItemType.BUG,
                Name: "Soul Wisp",
                Desc: "LOOT - craft into an \nemblem at the Emblem \nForge.",
                Tex: GadgetCoreAPI.LoadTexture2D("SoulWisp.png"),
                Value: 2);
            SoulWisp.Register();

            SoulEmblem = new ItemInfo(
                Type: ItemType.EMBLEM | ItemType.MONSTER,
                Name: "Soul Emblem",
                Desc: "Used as a crafting \nmaterial in the \nUniversal Crafter",
                Tex: GadgetCoreAPI.LoadTexture2D("SoulEmblem.png"),
                Value: 20);
            SoulEmblem.Register();
            GadgetCoreAPI.AddEmblemRecipe(SoulWisp.GetID(), SoulEmblem.GetID());

            // gear
            DemonHelm = new ItemInfo(
                Type: ItemType.HELMET,
                Name: "Demon Helm",
                Desc: "Reduces your maximum \nhealth by 15.",
                Tex: GadgetCoreAPI.LoadTexture2D("DemonHelm.png"),
                HeadTex: GadgetCoreAPI.LoadTexture2D("DemonHelmEquip.png"),
                Value: 60,
                Stats: new EquipStats(VIT: 8, STR: 6, DEX: 5, TEC: 0, MAG: 4, FTH: 4));
            DemonHelm.Register();

            var shadowArmorTex = GadgetCoreAPI.LoadTexture2D("ShadowArmor.png");
            ShadowArmor = new ItemInfo(
                Type: ItemType.ARMOR,
                Name: "Shadow Armor",
                //    "Combat chips directly \nconsume HP rather than \nmana. Can kill you.", // wrong; it refunds mana
                //    "Refunds mana used for \nchips at the cost of HP. \nIgnores 1-HP-protection.",
                //    "Refunds mana at the \ncost of HP. Bypasses DR \nand 1-HP-protection.",
                //    "Refunds mana at the \ncost of HP. Bypasses \nDR and 1-HP-protection.",
                //    "Refunds combat chip \nmana using HP. Bypasses \nDR and 1-HP-protection.",
                //    "Drains HP to refund \ncombat chip mana. Bypasses \nDR and 1-HP-protection.", // too long
                Desc: "Drains HP to refund \nchip mana, bypassing \nDR and 1-HP-protection.",
                Tex: shadowArmorTex,
                BodyTex: shadowArmorTex,
                ArmTex: new Texture2D(0, 0),
                Value: 60,
                Stats: new EquipStats(VIT: 8, STR: 4, DEX: 4, TEC: 0, MAG: 5, FTH: 5));
            ShadowArmor.Register();

            var soulShieldTex = GadgetCoreAPI.LoadTexture2D("SoulShield.png");
            SoulShield = new ItemInfo(
                Type: ItemType.OFFHAND,
                Name: "Soul Shield",
                // Reduces damage by up to half at a cost of 1 mana per hp reduced.
                Desc: "Damage taken is halved \nbefore other effects \nat the cost of mana.",
                Tex: soulShieldTex,
                HeldTex: soulShieldTex,
                Value: 60,
                Stats: new EquipStats(VIT: 6, STR: 3, DEX: 2, TEC: 0, MAG: 2, FTH: 2));
            SoulShield.Register();

            const int lanceSelfDmg = 2;
            DemonLance = new ItemInfo(
                Type: ItemType.WEAPON,
                Name: "Demon Lance",
                //Desc: "Costs 3hp to use, \nbypassing any effects. \nDamage: VIT+2×STR",
                Desc: $"Costs {lanceSelfDmg} HP, bypassing \nDR and 1-HP-protection. \nDamage: VIT+STR.",
                Tex: GadgetCoreAPI.LoadTexture2D("DemonLance.png"),
                HeldTex: GadgetCoreAPI.LoadTexture2D("DemonLanceUse.png"),
                Value: 60,
                Stats: new EquipStats(VIT: 4, STR: 5, DEX: 0, TEC: 0, MAG: 0, FTH: 0));
            DemonLance.SetWeaponInfo(new float[6] { 1f, 1f, 0f, 0f, 0f, 0f }, new AudioClip());
            IEnumerator DemonLanceOnAttack(PlayerScript player)
            {
                player.GetComponent<NetworkView>().RPC("TDTEXT", RPCMode.All, new object[] { lanceSelfDmg });
                GameScript.hp -= lanceSelfDmg;
                Menuu.characterStat[7] += lanceSelfDmg;
                if (GameScript.hp <= 0)
                {
                    GameScript.dead = true;
                    GameScript.hp = 0;
                    InstanceTracker.GameScript.Die();
                }
                InstanceTracker.GameScript.UpdateHP();
                yield return player.StartCoroutine(DemonLance.ThrustLance(player));
            }
            DemonLance.OnAttack += player => DemonLanceOnAttack(player);
            DemonLance.Register();

            DemonRing = new ItemInfo(
                Type: ItemType.RING,
                Name: "Demon Ring",
                Desc: "Taking damage gives \na crazy burst of \nmovement speed.",
                Tex: GadgetCoreAPI.LoadTexture2D("DemonRing.png"),
                Value: 60,
                Stats: new EquipStats(VIT: 6, STR: 4, DEX: 3, TEC: 0, MAG: 0, FTH: 0));
            DemonRing.Register();

            SoulRing = new ItemInfo(
                Type: ItemType.RING,
                Name: "Soul Ring",
                Desc: "Reduces damage taken \nout of combat mode by \n6, before most effects.",
                Tex: GadgetCoreAPI.LoadTexture2D("SoulRing.png"),
                Value: 60,
                Stats: new EquipStats(VIT: 3, STR: 0, DEX: 0, TEC: 0, MAG: 2, FTH: 2));
            SoulRing.Register();

            //recipes
            int shadow = ShadowEmblem.GetID();
            int demon = DemonEmblem.GetID();
            int soul = SoulEmblem.GetID();
            var recipes = new Tuple<int[], Item, int>[]
            {
                new Tuple<int[], Item, int>(
                    new int[3] { soul, demon, shadow },
                    SoulShield.Instantiate(),
                    0),
                new Tuple<int[], Item, int>(
                    new int[3] { soul, shadow, demon },
                    DemonLance.Instantiate(),
                    0),
                new Tuple<int[], Item, int>(
                    new int[3] { demon, soul, shadow },
                    DemonRing.Instantiate(),
                    0),
                new Tuple<int[], Item, int>(
                    new int[3] { shadow, soul, demon },
                    SoulRing.Instantiate(),
                    0),
                new Tuple<int[], Item, int>(
                    new int[3] { demon, shadow, soul },
                    DemonHelm.Instantiate(),
                    0),
                new Tuple<int[], Item, int>(
                    new int[3] { shadow, demon, soul },
                    ShadowArmor.Instantiate(),
                    0)
            };

            ((CraftMenuInfo)MenuRegistry.Singleton["Gadget Core:Crafter Menu"]).AddCraftPerformer(CraftMenuInfo.CreateSimpleCraftPerformer(recipes));
            RecipePage recipePage = new RecipePage(RecipePageType.UniversalCrafter, "Demon Content", GadgetCoreAPI.LoadTexture2D("Recipes.png")).Register();
            foreach (var recipe in recipes)
            {
                int[] inputIDs = recipe.Item1;
                int outputID = recipe.Item2.id;
                recipePage.AddRecipePageEntry(new RecipePageEntry(inputIDs[0], inputIDs[1], inputIDs[2], outputID, 1, 0));
            }

            // objects
            ObjectInfo bugspotObjectInfo = new ObjectInfo(ObjectType.BUGSPOT, SoulWisp.Instantiate(), 1, GadgetCoreAPI.LoadTexture2D("bugspot9.png"), GadgetCoreAPI.LoadTexture2D("flyHead.png"), GadgetCoreAPI.LoadTexture2D("flyWing.png"));
            bugspotObjectInfo.Register("Soul Wisp Bugspot");
            GadgetCoreAPI.AddCustomResource("obj/DemonContent/bugspot9", bugspotObjectInfo.Object);

            ObjectInfo oreObjectInfo = new ObjectInfo(ObjectType.ORE, DemonStone.Instantiate(), 1, GadgetCoreAPI.LoadTexture2D("ore9.png"));
            oreObjectInfo.Register("Demon Stone Ore");
            GadgetCoreAPI.AddCustomResource("obj/DemonContent/ore9", oreObjectInfo.Object);

            GameObject hazard = CreateHazard();
            GadgetCoreAPI.AddCustomResource("haz/DemonContent/haz9", hazard);


            // enemies
            GameObject fireDemonGO = CreateFireDemon();
            FireDemon = new EntityInfo(EntityType.RARE, fireDemonGO);
            FireDemon.Register("FireDemon");
            GadgetConsole.RegisterCommand("SummonFireDemon", true, (sender, args) =>
            {
                FireDemon.Spawn(InstanceTracker.PlayerScript.transform.position + new Vector3(10, 0, 0));
                return new GadgetConsole.GadgetConsoleMessage("Summoned Fire demon");
            }, "");

            GameObject poisonDemonGO = CreatePoisonDemon();
            PoisonDemon = new EntityInfo(EntityType.COMMON, poisonDemonGO);
            PoisonDemon.Register(nameof(PoisonDemon));
            GadgetConsole.RegisterCommand("SummonPoisonDemon", true, (sender, args) =>
            {
                PoisonDemon.Spawn(InstanceTracker.PlayerScript.transform.position + new Vector3(10, 0, 0));
                return new GadgetConsole.GadgetConsoleMessage("Summoned Poison demon");
            }, "");

            GameObject iceDemonGO = CreateIceDemon();
            IceDemon = new EntityInfo(EntityType.COMMON, iceDemonGO);
            IceDemon.Register("IceDemon");
            GadgetConsole.RegisterCommand("SummonIceDemon", true, (sender, args) =>
            {
                IceDemon.Spawn(InstanceTracker.PlayerScript.transform.position + new Vector3(10, 0, 0));
                return new GadgetConsole.GadgetConsoleMessage("Summoned Ice demon");
            }, "");
            GameObject icicle = CreateIceSpear();
            GadgetCoreAPI.AddCustomResource("haz/DemonContent/IceSpear", icicle);
        }

        private static GameObject CreateHazard()
        {
            GameObject hazard = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("haz/haz0"));
            Renderer[] rends = hazard.GetComponentsInChildren<Renderer>();
            foreach (var rend in rends)
            {
                rend.material = new Material(rend.material)
                {
                    mainTexture = GadgetCoreAPI.LoadTexture2D("Hazard.png")
                };
            }
            var hazScript = hazard.GetComponentInChildren<HazardScript>();
            hazScript.damage = 16;
            return hazard;
        }

        GameObject CreateFireDemon()
        {
            GameObject azazel = GameObject.Instantiate(Resources.Load<GameObject>("e/azazel"));
            azazel.name = "FireDemon";
            GameObject.DestroyImmediate(azazel.GetComponent<Azazel>());
            FireDemon fireDemon = azazel.AddComponent<FireDemon>();//ReplaceComponent<Azazel, FireDemon>();
            fireDemon.FindTransforms();
            fireDemon.AddLootTableDrop(item: ShadowFabric.Instantiate(), dropChance: 1f, minDropQuantity: 4, maxDropQuantity: 6);
            /* Modded drops?
            if (Gadgets.GetGadget("Crystal Crevasse")?.Enabled == true)
            {
                AddLootTableDrop(item: ItemRegistry.GetItemIDByName("").Instantiate(), dropChance: 0.5f, minDropQuantity: 1, maxDropQuantity: 1);
            }
            */
            GameObject.DestroyImmediate(fireDemon.eSub.GetComponent<Animation>());
            ChangeMaterial(fireDemon.head, "FireDemonHead.png");
            ChangeMaterial(fireDemon.chest, "FireDemonChest.png");
            ChangeMaterial(fireDemon.stomach, "FireDemonStomach.png");
            ChangeMaterial(fireDemon.leftArm, "FireDemonArm.png");
            ChangeMaterial(fireDemon.rightArm, "FireDemonArm.png");
            ChangeMaterial(fireDemon.wings, "FireDemonWings.png");
            ChangeMaterial(fireDemon.bottom, "FireDemonBottom.png");
            float armZ = (fireDemon.stomach.position.z + fireDemon.bottom.position.z) / 2f;
            fireDemon.leftArm.position = new Vector3(fireDemon.leftArm.position.x, fireDemon.leftArm.position.y, armZ);
            fireDemon.rightArm.position = new Vector3(fireDemon.rightArm.position.x, fireDemon.rightArm.position.y, armZ);
            return azazel;
        }

        GameObject CreateIceDemon()
        {
            GameObject cthu = GameObject.Instantiate(Resources.Load<GameObject>("e/cthu"));
            cthu.name = "IceDemon";
            GameObject.DestroyImmediate(cthu.GetComponent<Cthu>());
            IceDemon iceDemon = cthu.AddComponent<IceDemon>();//ReplaceComponent<Azazel, FireDemon>();
            iceDemon.FindTransforms();
            iceDemon.AddLootTableDrop(item: ShadowFabric.Instantiate(), dropChance: 1f, minDropQuantity: 1, maxDropQuantity: 4);
            ChangeMaterial(iceDemon.head, "IceDemonHead.png");
            ChangeMaterial(iceDemon.arms, "IceDemonBody.png");
            ChangeMaterial(iceDemon.body, "IceDemonBody2.png");
            return cthu;
        }

        GameObject CreatePoisonDemon()
        {
            GameObject cham = GameObject.Instantiate(Resources.Load<GameObject>("e/chamcham"));
            cham.name = nameof(PoisonDemon);
            GameObject.DestroyImmediate(cham.GetComponent<ChamchamScript>());
            PoisonDemon poisonDemon = cham.AddComponent<PoisonDemon>();
            poisonDemon.FindTransforms();
            poisonDemon.AddLootTableDrop(item: ShadowFabric.Instantiate(), dropChance: 1f, minDropQuantity: 1, maxDropQuantity: 4);
            ChangeMaterial(poisonDemon.head, "PoisonDemonHead.png");
            ChangeMaterial(poisonDemon.tail, "PoisonDemonTail.png");
            ChangeMaterial(poisonDemon.body, "PoisonDemonBody.png");
            ChangeMaterial(poisonDemon.legs, "PoisonDemonLegs.png");
            poisonDemon.ear1.gameObject.SetActive(false);
            poisonDemon.ear2.gameObject.SetActive(false);
            return cham;
        }

        static GameObject CreateIceSpear()
        {
            GameObject spear = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("haz/gruublade"));
            Renderer proj = spear.transform.GetChild(0).GetComponentInChildren<Renderer>();

            proj.material = new Material(proj.material)
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("IceSpear.png")
            };
            Renderer trail = spear.transform.GetChild(1).GetComponentInChildren<Renderer>();
            trail.material = new Material(trail.material)
            {
                mainTexture = GadgetCoreAPI.LoadTexture2D("IceSpearTrail.png")
            };
            return spear;
        }

        static void ChangeMaterial(Transform gameObject, string textureName)
        {
            var rend = gameObject.GetComponent<Renderer>();
            rend.material = new Material(rend.material);
            rend.material.mainTexture = GadgetCoreAPI.LoadTexture2D(textureName);
        }

        public static void TryAddHealthBar(NetworkViewID viewID, int maxHP, bool isBoss)
        {
            var healthBarGadget = Gadgets.GetGadget("Health Bar Mod");
            if (healthBarGadget?.Enabled == true)
            {
                healthBarGadget.GetType().Assembly.GetType("HealthBarMod.HealthBarRPCs")
                    .GetMethod("AddHealthBar", BindingFlags.Static | BindingFlags.Public)
                    .Invoke(null, new object[3] { viewID, maxHP, isBoss });
            }
        }
    }
}