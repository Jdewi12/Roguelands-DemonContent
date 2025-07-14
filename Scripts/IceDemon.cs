using GadgetCore.API;
using GadgetCore.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DemonContent.Scripts
{
    public class IceDemon : CustomEntityScript<IceDemon>
    {
        public Transform e;
        public Transform eSub;
        public Transform head;
        public Transform arms;
        public Transform body;

        public HazardScript hazard;

        private bool attacking = false;

        public const int BaseHP = 1000;

        public void Awake() 
        {
            if(e == null)
                FindTransforms();
            Initialize(hp: BaseHP, contactDamage: 20, exp: BaseHP * 3 / 5 /*int division*/, isFlying: true);
            hazard.damage = ContactDamage;
            FrostEffect = 4;
            if (Network.isServer)
            {
                StartCoroutine(AttackAI());
                StartCoroutine(FollowAI());
            }
            var networkView = GetComponent<NetworkView>();
            networkView.observed = rigidbody;
        }

        bool triedHealthBar = false;
        [RPC]
        public override void TD(float[] msg)
        {
            if (!triedHealthBar)
            {
                DemonContent.TryAddHealthBar(gameObject.GetComponent<NetworkView>().viewID, MaxHP, false);
                triedHealthBar = true;
            }
            base.TD(msg);
            gameObject.SendMessage("UpdateHealth", HP, SendMessageOptions.DontRequireReceiver);
        }


        protected override void InternalInit()
        {
            NetworkEnemyBasic networkEnemyBasic = GetComponent<NetworkEnemyBasic>();
            if (networkEnemyBasic != null) Destroy(networkEnemyBasic);
            NetworkR2 networkR2 = GetComponent<NetworkR2>();
            if (networkR2 != null) Destroy(networkR2);
            NetworkR4 networkR4 = GetComponent<NetworkR4>();
            if (networkR4 != null) Destroy(networkR4);
            NetworkRotation networkRotation = GetComponent<NetworkRotation>();
            if (networkRotation != null) Destroy(networkRotation);

            if (!Initialized)
                StartCoroutine(Ready());
            Initialized = true;

            if (Network.isServer && trig != null && trig.Length >= 2 && trig[0] != null && trig[1] != null)
            {
                StartCoroutine(TriggerAlternate());
            }
        }

        private IEnumerator TriggerAlternate()
        {
            while (true)
            {
                if (!IsDead)
                {
                    trig[0].SetActive(false);
                    trig[1].SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    trig[1].SetActive(false);
                    trig[0].SetActive(true);
                }
                yield return new WaitForSeconds(1f);
            }
        }

        /*
        IEnumerator ToggleAggroCubes()
        {
            int i = 0;
            while(true)
            {
                aggroCubes[i].gameObject.SetActive(false);
                i++;
                if (i >= aggroCubes.Count)
                    i = 0;
                aggroCubes[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
            }

        }*/


        public void FindTransforms()
        {
            hazard = transform.Find("hazard").GetComponent<HazardScript>();
            var aggroCubes = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if(child.name.StartsWith("Cube"))
                    aggroCubes.Add(child.gameObject);
            }
            trig = aggroCubes.ToArray();
            e = transform.Find("e");
            eSub = e.Find(nameof(IceDemon));
            if(eSub == null)
            {
                eSub = e.Find("cthu");
                eSub.name = nameof(IceDemon);
            }
            head = eSub.Find("Plane");
            arms = eSub.Find("Plane_001");
            eSub.Find("Plane_002").gameObject.SetActive(false);
            body = eSub.Find("Plane_003");
        }

        IEnumerator FollowAI()
        {
            while (true)
            {
                if (AttackTarget != null && !attacking)
                {
                    if (AttackTarget.transform.position.x - eSub.transform.position.x > 0)
                    {
                        head.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        head.localScale = Vector3.one;
                    }

                    float speed = Mathf.Clamp(Vector3.Distance(transform.position, AttackTarget.transform.position), 8f, 14f);
                    var vec = (AttackTarget.transform.position + Vector3.up * 2f - transform.position).normalized;
                    rigidbody.velocity = vec * speed;

                    const float maxAggroRange = 80;
                    // de-aggro
                    if ((AttackTarget.transform.position - transform.position).sqrMagnitude > maxAggroRange * maxAggroRange)
                    {
                        AttackTarget = null;
                    }
                }
                else
                {
                    rigidbody.velocity = Vector3.zero;
                }
                yield return null;
            }
        }

        IEnumerator AttackAI()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2.3f, 4.5f));
                if (AttackTarget != null)
                {
                    float rng = Random.Range(0f, 1f);
                    if (rng <= 1f) // 100% chance
                    {
                        GetComponent<NetworkView>().RPC(nameof(StartIceSpears), RPCMode.All, AttackTarget.transform.position.x, AttackTarget.transform.position.y);
                    }
                    else
                    {
                        GetComponent<NetworkView>().RPC(nameof(StartFrostAura), RPCMode.All);
                    }
                }
            }
        }

        [RPC]
        void StartIceSpears(float xTarget, float yTarget)
        {
            StartCoroutine(IceSpears(xTarget, yTarget));
        }

        IEnumerator IceSpears(float xTarget, float yTarget)
        {
            int xDirection = xTarget > transform.position.x ? 1 : -1;
            attacking = true;
            yield return new WaitForSeconds(0.3f);
            List<GameObject> spears = new List<GameObject>();
            float spacing = 6f;
            int numSpears = Mathf.Clamp(Mathf.RoundToInt((transform.position.x - xTarget)/ spacing) + 4,
                4,
                8);
            for (int i = 0; i < numSpears; i++)
            {
                if (HP == 0) // we died mid-attack
                    break;
                GameObject spear = (GameObject)GameObject.Instantiate(GadgetCoreAPI.GetCustomResource("haz/DemonContent/IceSpear"), new Vector3(transform.position.x + xDirection * (i - 1) * spacing, Mathf.Max(transform.position.y, yTarget) + 8f + (i % 2) * 2f, 0f), Quaternion.identity);
                spears.Add(spear); 
                spear.SendMessage("Sett", xDirection);
                var script = spear.GetComponent<DemonSword>();
                script.enabled = false;
                GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/demonsword"), Menuu.soundLevel / 7f); // note: louder than normal
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(0.3f);
            GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/fire"), Menuu.soundLevel / 10f);
            foreach (var spear in spears)
            {
                var script = spear.GetComponent<DemonSword>();
                script.enabled = true;
            }
            yield return new WaitForSeconds(0.3f);
            attacking = false;
        }

        [RPC]
        void StartFrostAura()
        {
            StartCoroutine(FrostAura());
        }

        IEnumerator FrostAura()
        {
            yield break;
        }
    }
}
