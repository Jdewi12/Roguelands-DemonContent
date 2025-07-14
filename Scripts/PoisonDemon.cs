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
using UnityEngine.Networking.Types;
using Random = UnityEngine.Random;

namespace DemonContent.Scripts
{
    public class PoisonDemon : CustomEntityScript<PoisonDemon>
    {
        public Transform e;
        public Transform eSub;
        public Transform head;
        public Transform body;
        public Transform tail;
        public Transform legs;
        public Transform ear1;
        public Transform ear2;

        public HazardScript hazard;
        public Animation anim;

        private bool attacking = false;

        public const int BaseHP = 230;

        public void Awake() 
        {
            if(e == null)
                FindTransforms();
            Initialize(hp: BaseHP, contactDamage: 8, exp: BaseHP * 3 / 5 /*int division*/, isFlying: true);
            hazard.damage = ContactDamage;
            hazard.isPoison = 3;
            rigidbody.useGravity = true;
            anim["i"].layer = 0; // Just copying the layers and speed from chamchamscript
            anim["a"].layer = 1;
            anim["r"].layer = 0;
            anim["r"].speed = 1f;
            anim["a"].speed = 1.5f;
            anim["i"].speed = 0.5f;
            anim.Play("i"); // idle
            if(Network.isServer)
                StartCoroutine(FollowAI());
            var networkView = GetComponent<NetworkView>();
            networkView.observed = rigidbody;
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


        public void FindTransforms()
        {
            var box = GetComponent<BoxCollider>();
            var hazardGO = new GameObject("hazard");
            hazardGO.transform.parent = transform;
            hazardGO.transform.localPosition = Vector3.zero;
            var hazBox = hazardGO.AddComponent<BoxCollider>();
            hazBox.center = box.center;
            hazBox.size = box.size;
            hazBox.isTrigger = true;
            hazard = hazardGO.AddComponent<HazardScript>();

            trig = new GameObject[2];
            for (int i = 0; i < 2; i++)
            {
                var aggroGO = new GameObject("aggroCube" + i);
                aggroGO.transform.parent = transform;
                var aggroBox = aggroGO.AddComponent<BoxCollider>();
                aggroBox.isTrigger = true;
                aggroBox.size = new Vector2(40f, 40f);
                trig[i] = aggroGO;
            }
            e = transform.Find("e");
            eSub = e.Find(nameof(PoisonDemon));
            if(eSub == null)
            {
                eSub = e.Find("chamcham");
                eSub.name = nameof(PoisonDemon);
            }
            head = eSub.Find("Plane");
            ear1 = head.Find("Plane_001");
            ear2 = head.Find("Plane_002");
            body = eSub.Find("Plane_003");
            tail = eSub.Find("Plane_004");
            legs = eSub.Find("Plane_005");
            anim = eSub.GetComponent<Animation>();
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

        IEnumerator FollowAI()
        {
            float timeSinceJump = 0f;
            while (true)
            {
                if (AttackTarget != null && !attacking)
                {
                    if(timeSinceJump == 0f)
                        GetComponent<NetworkView>().RPC(nameof(Aggro), RPCMode.All);
                    timeSinceJump += Time.deltaTime;

                    float xDiff = AttackTarget.transform.position.x - eSub.transform.position.x;
                    float xDir = Mathf.Sign(xDiff);
                    if (xDir > 0)
                    {
                        head.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        head.localScale = Vector3.one;
                    }

                    if(Mathf.Abs(xDiff) > 1f && timeSinceJump > 0.4f)
                        rigidbody.velocity = new Vector3(xDir * 15f, rigidbody.velocity.y);
                    if (timeSinceJump > 0.85f) // cooldown
                    { 
                        float yDiff = AttackTarget.transform.position.y - eSub.transform.position.y;
                        if(yDiff > 1f)
                        {
                            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 28f);
                            timeSinceJump = 0f;
                            //anim.Play("j"); // jumping?
                        }
                    }

                    const float maxAggroRange = 80;
                    // de-aggro
                    if ((AttackTarget.transform.position - transform.position).sqrMagnitude > maxAggroRange * maxAggroRange)
                    {
                        AttackTarget = null;
                    }
                }

                yield return null;
            }
        }

        [RPC]
        public void Aggro()
        {
            anim.Play("r");
            StartCoroutine(AggroSound());

        }

        IEnumerator AggroSound()
        {
            var source = GetComponent<AudioSource>();
            source.pitch = 3f;
            base.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/demonsl"), Menuu.soundLevel / 14f);
            yield return new WaitForSeconds(0.4f);
            source.pitch = 1.3f;
            base.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/chamcham"), Menuu.soundLevel / 50f);
        }
    }
}
