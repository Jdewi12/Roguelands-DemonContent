using GadgetCore.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DemonContent.Scripts
{
    public class FireDemon : CustomEntityScript<FireDemon>
    {
        public Transform e;
        public Transform eSub;
        public Transform head;
        public Transform chest;
        public Transform stomach;
        public Transform leftArm;
        public Transform rightArm;
        public Transform wings;
        public Transform bottom;

        public HazardScript leftArmHazard;
        public HazardScript rightArmHazard;

        public const int BaseHP = 1800;

        int burn;

        public void Awake() 
        {
            if (e == null)
                FindTransforms();
            int cL = GameScript.challengeLevel;
            int maxHP = BaseHP + 500 * cL;
            // actual drop amount is between quantity and quantity + variation (inclusive)
            AddCurrencyDrop(currencyID: 52, quantity: maxHP / 6, quantityVariation: maxHP / 12);
            Initialize(hp: maxHP, contactDamage: 15 + cL, exp: maxHP * 3 / 5 /*int division*/, isFlying: true);
            leftArmHazard.damage = rightArmHazard.damage = ContactDamage - cL * 6; // HazardScript adds cL * 6
            burn = 4;
            if (cL > 0)
                burn += 1;
            leftArmHazard.isBurn = rightArmHazard.isBurn = burn;
            StartCoroutine(FlapWings());
            if(Network.isServer)
            {
                StartCoroutine(AttackAI());
                StartCoroutine(FollowAI());
            }

            var networkView = GetComponent<NetworkView>();
            networkView.observed = rigidbody;
        }

        public override void OnCollisionEnter(Collision c)
        {
            base.OnCollisionEnter(c);
            // Apply status manually to prevent the base class applying challenge level scaling to our status effects
            if (burn > 0)
                c.gameObject.SendMessage("BUR", burn);
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

        public void FindTransforms()
        {
            e = transform.Find("e");
            eSub = e.Find("FireDemon");
            if(eSub == null)
            {
                eSub = e.Find("azazel");
                eSub.name = "FireDemon";
            }
            head = eSub.Find("Plane");
            chest = eSub.Find("Plane_001");
            stomach = eSub.Find("Plane_002");
            leftArm = eSub.Find("Plane_003");
            rightArm = eSub.Find("Plane_004");
            wings = eSub.Find("Plane_005");
            bottom = eSub.Find("Plane_006");

            leftArmHazard = leftArm.GetComponentInChildren<HazardScript>();
            rightArmHazard = rightArm.GetComponentInChildren<HazardScript>();

            var cols = new List<GameObject>();
            foreach(Transform t in transform)
            {
                if (t.name.StartsWith("Cube"))
                {
                    cols.Add(t.gameObject);
                    if (t.localScale.sqrMagnitude > 80 * 80)
                        t.localScale = new Vector3(80f, 80f, 1f);
                }
            }
            trig = cols.ToArray();
        }

        IEnumerator FlapWings()
        {
            Vector3 startingScale = wings.localScale;
            float lastVerticalMove = 0;
            float t = 0f;
            while (true)
            {
                float wingsXScale = Mathf.Sin(Mathf.PI * 2f * t * 2f) / 4f + 1.15f; // period 0.5s
                wings.localScale = new Vector3(startingScale.x * wingsXScale, startingScale.y, startingScale.z);
                float bodyYOffset = Mathf.Sin(Mathf.PI * 2f * t * 2f) / 8f; // period 0.5s
                // by reverting the last vertical move we can move relative to the default position, without affecting other animations/movements
                transform.localPosition -= new Vector3(0, lastVerticalMove, 0);
                transform.localPosition += new Vector3(0, bodyYOffset, 0);
                lastVerticalMove = bodyYOffset;

                t += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator FollowAI()
        {
            rigidbody.drag = 0.5f;
            while (true)
            {
                if(AttackTarget != null)
                {
                    if (AttackTarget.transform.position.x - eSub.transform.position.x > 0)
                    {
                        head.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        head.localScale = Vector3.one;
                    }
                    
                    const float forceDistanceScale = 10f;
                    Vector3 force = (AttackTarget.transform.position + Vector3.up * 2f - transform.position) * forceDistanceScale;
                    const float decelDist = 5f;
                    // decrease the force by flat amount if close by
                    if (force.sqrMagnitude < decelDist * decelDist)
                        force -= force.normalized * decelDist * forceDistanceScale;
                    //else
                    const float maxForce = 300;
                    if(force.sqrMagnitude > maxForce * maxForce)
                    {
                        force = force.normalized * maxForce;
                    }
                    rigidbody.AddForce(force * Time.deltaTime);

                    const float maxAggroRange = 100;
                    //de-aggro
                    if ((AttackTarget.transform.position - transform.position).sqrMagnitude > maxAggroRange * maxAggroRange)
                    {
                        AttackTarget = null;
                    }
                }
                yield return null;
            }
        }

        IEnumerator AttackAI()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1.5f, 3f));
                if (AttackTarget != null)
                {
                    float rng = Random.Range(0f, 1f);
                    if (rng <= 0.5f)
                    {
                        GetComponent<NetworkView>().RPC(nameof(StartFireBreath), RPCMode.All);
                    }
                    else
                    {
                        GetComponent<NetworkView>().RPC(nameof(StartFireBalls), RPCMode.All,
                            AttackTarget.transform.position.x, AttackTarget.transform.position.y);
                    }
                }
            }
        }

        [RPC]
        public void StartFireBalls(float targetX, float targetY)
        {
            StartCoroutine(SwipeFireballs(targetX, targetY));
        }

        FieldInfo projectileDirField = typeof(Projectile).GetField("dir", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo projectileDyingField = typeof(Projectile).GetField("dying", BindingFlags.Instance | BindingFlags.NonPublic);
        IEnumerator SwipeFireballs(float targetX, float targetY)
        {
            // todo: Swipe in direction of target
            // todo: Track amount rotated so we can reverse it perfectly
            const float duration = 0.7f;
            const float upDuration = duration * 2f / 3f; // 2/3rds
            const float downDuration = duration - upDuration; // 1/3rd
            const float swingAngle = 90f;
            const int numFireBalls = 4;
            int fireBallsFired = 0;
            float t = 0f;
            while (t < duration)
            {
                float dt = Time.deltaTime;
                if (t < upDuration)
                    leftArm.transform.RotateAround(chest.transform.position + new Vector3(-0.1f, 2f, 0f), new Vector3(0f, 0f, -1f), dt / upDuration * swingAngle);
                else
                    leftArm.transform.RotateAround(chest.transform.position + new Vector3(-0.1f, 2f, 0f), new Vector3(0f, 0f, -1f), dt / downDuration * -swingAngle);
                if ((t - upDuration + dt) / downDuration >= fireBallsFired / ((float)numFireBalls - 1)) // fire them evenly over the down swing
                {
                    fireBallsFired++;
                    if(fireBallsFired == 1) // sound is only played once for the whole swing
                        GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/fire"), Menuu.soundLevel / 10f);
                    Vector3 handPos = leftArm.transform.position + leftArm.transform.rotation * Vector2.left * 3f;
                    if (Network.isServer)
                    {
                        // offset target based on where the hand is along the swing
                        var targetPos = new Vector3(targetX, targetY) + (handPos - chest.transform.position) * 0.8f; 

                        var prefab = (GameObject)Resources.Load("proj/wyvern");
                        var prefabProj = prefab.GetComponent<Projectile>();
                        var oldDeathTimer = prefabProj.deathTimer;
                        prefabProj.deathTimer = 10f; // we need to set this before Instantiate so it's set before Awake is called.
                        GameObject p = (GameObject)Network.Instantiate(prefab, handPos, Quaternion.identity, 0);
                        var haz = p.GetComponent<HazardScript>();
                        haz.damage = 15 + GameScript.challengeLevel;
                        haz.isBurn = 4 + GameScript.challengeLevel; // 4, 5, 6, 7
                        p.SendMessage("EnemySet", targetPos, SendMessageOptions.DontRequireReceiver);
                        prefabProj.deathTimer = oldDeathTimer; // fix the prefab we modified
                    }
                }

                t += dt;
                yield return null;
            }
        }

        [RPC]
        public void StartFireBreath()
        {
            StartCoroutine(FireBreath());
        }

        IEnumerator FireBreath()
        {
            const float duration = 1f;
            const float timeBetween = 0.05f;
            float soundCooldown = 0f;
            float t = 0f;
            while (t < duration)
            {
                if (soundCooldown <= 0f)
                {
                    GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/fire"), Menuu.soundLevel / 10f);
                    soundCooldown += 0.35f;
                }
                if (Network.isServer && AttackTarget != null)
                {
                    Vector3 shotOrigin = head.transform.position + new Vector3(0f, 2.9f);
                    GameObject go = (GameObject)Network.Instantiate(Resources.Load("proj/spitter"), shotOrigin, Quaternion.identity, 0);
                    go.SendMessage("SpitterSet", SendMessageOptions.DontRequireReceiver);
                    var haz = go.GetComponent<HazardScript>();
                    int cL = GameScript.challengeLevel;
                    haz.damage = 14; // 14, 16, 16, 18
                    if (cL >= 1)
                        haz.damage += 2;
                    if (cL >= 3)
                        haz.damage += cL - 1;
                    haz.isBurn = 3 + cL / 2; // int division; 3, 3, 4, 4
                    var dir = AttackTarget.transform.position - shotOrigin;
                    go.transform.up = dir;
                }
                t += timeBetween;
                soundCooldown -= timeBetween;
                yield return new WaitForSeconds(timeBetween);
            }
        }
    }
}
