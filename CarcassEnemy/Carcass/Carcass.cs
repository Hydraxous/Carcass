using CarcassEnemy.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.UIR;

namespace CarcassEnemy
{
    public class Carcass : MonoBehaviour, ICustomEnemy
    {
        //References
        [SerializeField] private CarcassComponents components;
        public CarcassComponents Components => components;

        [SerializeField] private Transform target;
        public Transform Target => target;

        [SerializeField] private CarcassParametersAsset serializedParameters;

        private CarcassParameters parameters;
        public CarcassParameters Parameters
        {
            get
            {
                if (parameters == null)
                {
                    if (serializedParameters != null)
                        parameters = serializedParameters.Parameters;
                    else
                        parameters = new CarcassParameters(); //L
                }
                return parameters;
            }
        }

        //Public
        public float Health => health;
        public bool IsAlive => health > 0;
        public bool Enraged { get; private set; } = false;
        public bool IsAttacking => isActioning;


        //internal state
        private float health;
        private bool dead;
        private bool isStunned;
        private bool isHealing;
        private float randomStrafeDirection;
        private float timeUntilDirectionChange = 2f;
        private float actionTimer = 3f;
        private float healCooldownTimer;
        private bool isActioning;
        public Vector3 ExternalForce { get; private set; }
        private float eyeRespawnTimer;
        private Delegate lastAttack;

        private List<Drone> spawnedEyes = new List<Drone>();

        //debug
        public static bool DisableActionTimer;

        #region UnityMessages

        private void Awake()
        {
            if (components != null)
            {
                health = Parameters.maxHealth;
                components.Machine.health = Parameters.maxHealth;
                foreach (GameObject go in components.Hitboxes)
                {
                    Rigidbody rb = go.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }

        private void Start()
        {
            SetTarget(PlayerTracker.Instance.GetTarget());
            StartCoroutine(ActionFailsafe());
            SpawnEyes();
        }


        private void Update()
        {
            TimerUpdate();
        }

        private void FixedUpdate()
        {
            MovementUpdate();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            TurnTowards(target.position, 20000f);
        }

        #endregion

        private void TimerUpdate()
        {
            timeUntilDirectionChange -= Time.deltaTime;
            if (timeUntilDirectionChange <= 0f)
            {
                ChangeStrafeDirection();
                timeUntilDirectionChange = Parameters.directionChangeDelay;
            }

            if (!isActioning && !DisableActionTimer)
                actionTimer -= Time.deltaTime;

            if (actionTimer <= 0f && !isStunned)
            {
                PerformAction();
                actionTimer = Parameters.attackDelay;
            }

            eyeRespawnTimer -= Time.deltaTime;
        }
        private Vector3 GetRingSpawnPosition()
        {
            Vector3 position = Components.CenterMass.position;
            Vector2 disc = UnityEngine.Random.onUnitSphere;
            disc.Normalize();

            Vector3 targetPos = position + Vector3.up;
            if (target != null)
            {
                targetPos = target.position;
            }

            Quaternion targetAlignedRotation = Quaternion.LookRotation(targetPos - position);
            Vector3 offset = targetAlignedRotation * new Vector3(disc.x, disc.y, 0f);

            position += offset * Parameters.shake_ProjectileOriginRadius;
            return position;
        }

        #region Movement

        private void TurnTowards(Vector3 point, float speed)
        {
            Vector3 pos = transform.position;
            Vector3 direction = point - pos;
            Quaternion rot = Quaternion.LookRotation(direction.XZ());
            Quaternion currentRot = transform.rotation;
            Quaternion newRotation = Quaternion.RotateTowards(currentRot, rot, Time.deltaTime * speed);
            transform.rotation = newRotation;
        }

        private float CalculateVerticalMoveDirection(Vector3 position, float desiredHeight)
        {
            if (!Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore))
                return -1f;

            float targetHeight = hit.point.y + desiredHeight;

            if (target != null)
            {
                targetHeight = Mathf.Max(targetHeight, target.position.y);
            }

            float distanceToDesiredHeight = targetHeight - position.y;
            return Mathf.Sign(distanceToDesiredHeight);
        }


        private void MovementUpdate()
        {
            Vector3 velocity = GetVelocity();

            if (isStunned || dead || isHealing)
                velocity = ApplyBrake(velocity);
            else
                velocity = ApplyMovement(velocity);

            Components.Rigidbody.velocity = velocity;
        }

        private Vector3 GetVelocity()
        {
            return Components.Rigidbody.velocity;
        }

        private Vector3 ApplyMovement(Vector3 velocity)
        {
            Vector3 travelVector = transform.right * randomStrafeDirection;

            Vector3 targetPos = target.position;
            Vector3 pos = transform.position;

            Vector3 toTarget = targetPos - pos;
            Vector3 toTargetXZ = toTarget.XZ();

            float lateralDistance = toTargetXZ.magnitude;

            float speedMultiplier = GetMoveSpeedMultiplier();

            //GUH fix this.

            float latSpeed = Parameters.strafeSpeed;

            if (lateralDistance < Parameters.minTargetDistance)
            {
                travelVector = -toTargetXZ.normalized;
                latSpeed = Parameters.lateralFlySpeed;
            }

            if (lateralDistance > Parameters.maxTargetDistance)
            {
                travelVector = toTargetXZ.normalized;
                latSpeed = Parameters.lateralFlySpeed;
            }

            latSpeed *= speedMultiplier;

            Vector3 moveDelta = travelVector * latSpeed;

            float verticalMovement = CalculateVerticalMoveDirection(transform.position, Parameters.desiredFlyHeight);
            verticalMovement *= (verticalMovement > 0) ? Parameters.verticalFlySpeed : Parameters.fallSpeed;

            moveDelta.y += verticalMovement * speedMultiplier;
            velocity = Vector3.MoveTowards(velocity, moveDelta, Time.deltaTime * Parameters.movementSmoothing);
            return velocity;
        }

        private Vector3 ApplyBrake(Vector3 velocity)
        {
            return Vector3.MoveTowards(velocity, Vector3.zero, Time.deltaTime * Parameters.movementSmoothing);
        }

        private float GetMoveSpeedMultiplier()
        {
            if (isActioning)
                return Parameters.speedWhileAttackingMultiplier;

            if (Enraged)
                return Parameters.enragedSpeedMultiplier;

            return 1f;
        }
        private void ChangeStrafeDirection()
        {
            randomStrafeDirection = Mathf.Sign((UnityEngine.Random.value - 0.5f) * 2f);
        }

        #endregion

        #region Actions
        private void PerformAction()
        {
            //If can spawn eyes. Spawn eyes.
            if (spawnedEyes.Count < Parameters.eye_SpawnCount && eyeRespawnTimer <= 0f)
            {
                SpawnEyes();
                return;
            }


            //Peform damage attack
            Vector3 position = transform.position;
            Vector3 targetPos = target.position;

            Vector3 toTarget = targetPos - position;

            float verticalDistance = toTarget.y;
            float lateralDistance = toTarget.XZ().magnitude;

            List<Delegate> attackPool = new List<Delegate>();

            if (healCooldownTimer <= 0f)
                if (spawnedEyes.Count > 0)
                    if (health < Parameters.lowHealthThreshold)
                        attackPool.Add(HealAction);

            //In range and higher up than the player
            if (lateralDistance < Parameters.spin_MaxRange && verticalDistance < 0)
                attackPool.Add(SpinAttack);

            attackPool.Add(ShakeAttack);

            if (lastAttack != null)
                attackPool.Remove(lastAttack);

            lastAttack = attackPool[UnityEngine.Random.Range(0, attackPool.Count)];
            lastAttack.DynamicInvoke();
        
        }

        public void SpinAttack()
        {
            lastActionName = "LobYellow";
            isActioning = true;
            Components.Animation.Spin();
            StartCoroutine(InvokeAfterAnimation(AttackDone));
        }

        public void Stun()
        {
            isStunned = true;
            isActioning = false;
            isHealing = false;
            StopAllCoroutines();
            Components.Animation.Stunned();
            StartCoroutine(InvokeAfterAnimation(StopStun));
        }

        private void StopStun()
        {
            isStunned = false;
        }

        public void ShakeAttack()
        {
            lastActionName = "BlueAttack";
            isActioning = true;
            Components.Animation.Shake();
            StartCoroutine(ShakeAttackCoroutine());
        }

        public void SpawnEyes()
        {
            lastActionName = "SpawnEyes";
            eyeRespawnTimer = Parameters.eye_SpawnCooldown;
            isActioning = true;
            Components.Animation.Shake();

            int eyesToSpawn = Parameters.eye_SpawnCount - spawnedEyes.Count;

            for (int i = 0; i < eyesToSpawn; i++)
            {
                float delay = Parameters.eye_SpawnDelay * (i + 1);
                InvokeAfterTime(SpawnSingleEye, delay);
            }

            StartCoroutine(InvokeAfterAnimation(AttackDone));
        }

        public Projectile FireExplosiveProjectile()
        {
            Transform tf = Components.ProjectileLobPoint;

            Vector3 position = tf.position;
            Vector3 direction = tf.forward;

            Projectile projectile = GameObject.Instantiate<GameObject>(UKPrefabs.LobbedProjectileExplosiveHH.Asset, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
            projectile.target = this.target;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(transform.up * 50f, ForceMode.VelocityChange);
            projectile.safeEnemyType = EnemyType.Mindflayer; //lol
            //TODO: Figure out gorezone parenting
            //projectile.transform.SetParent(base.GetComponentInParent<GoreZone>().transform, true);
            projectile.speed *= Components.EnemyIdentifier.totalSpeedModifier;
            projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;
            return projectile;
        }

        public Projectile FireTrackingProjectile()
        {
            Transform tf = Components.CenterMass;

            Vector3 position = tf.position;
            Vector3 offset = UnityEngine.Random.onUnitSphere;

            position += offset * Parameters.shake_ProjectileOriginRadius;

            GameObject projectileGameObject = GameObject.Instantiate(UKPrefabs.HomingProjectile.Asset, position, Quaternion.LookRotation(offset));
            Projectile projectile = projectileGameObject.GetComponent<Projectile>();
            projectile.target = this.target;
            projectile.speed = 10f * Components.EnemyIdentifier.totalSpeedModifier;
            projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;

            return projectile;
        }

        public Projectile FireTrackingProjectileHalo()
        {
            Vector3 position = GetRingSpawnPosition();
            GameObject projectileGameObject = GameObject.Instantiate(UKPrefabs.HomingProjectile.Asset, position, Quaternion.LookRotation(position - Components.CenterMass.position));
            Projectile projectile = projectileGameObject.GetComponent<Projectile>();
            projectile.target = this.target;
            projectile.speed = 10f * Components.EnemyIdentifier.totalSpeedModifier;
            projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;

            return projectile;
        }

        public void Enrage()
        {
            Enraged = true;
            Debug.Log("Carcass enraged");
        }

        public void Instakill()
        {
            if (dead)
                return;

            dead = true;
            StopAllCoroutines();
            StartCoroutine(DeathCoroutine());
        }

        public void AttackDone()
        {
            isActioning = false;
        }

        private void SpawnSingleEye()
        {
            Vector3 position = GetRingSpawnPosition();
            GameObject eyeObject = GameObject.Instantiate(UKPrefabs.DroneFlesh.Asset, position, Quaternion.identity);

            if (eyeObject.TryGetComponent<Drone>(out Drone drone))
            {
                spawnedEyes.Add(drone);
                drone.health = Parameters.eye_Health;
            }

            if (eyeObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyIdentifier))
            {
                enemyIdentifier.dontCountAsKills = true;
                enemyIdentifier.health = Parameters.eye_Health;
                enemyIdentifier.damageBuff = Components.EnemyIdentifier.damageBuff;
                enemyIdentifier.healthBuff = Components.EnemyIdentifier.healthBuff;
                enemyIdentifier.speedBuff = Components.EnemyIdentifier.speedBuff;

                if (drone != null)
                    enemyIdentifier.onDeath.AddListener(() => { OnEyeDeath(drone); });
            }
        }

        public void HealAction()
        {
            lastActionName = "Heal";
            isActioning = true;
            isHealing = true;
            healCooldownTimer = Parameters.healCooldown;
            Components.Animation.Shake();
            InvokeAfterTime(() =>
            {
                for (int i = 0; i < spawnedEyes.Count; i++)
                {
                    InvokeAfterTime(SacrificeEyeForHealth, Parameters.eye_HealDelay * (i));
                }

            }, Parameters.eye_InitialHealDelay);

            StartCoroutine(InvokeAfterAnimation(() =>
            {
                isHealing = false;
                AttackDone();
            }));

        }

        private void SacrificeEyeForHealth()
        {
            if (isStunned || dead || !isHealing)
                return;

            Drone spawnedEye = null;
            for (int i = 0; i < spawnedEyes.Count; i++)
            {
                if (spawnedEyes[i] == null)
                    continue;

                spawnedEye = spawnedEyes[i];
                break;
            }

            if (spawnedEye == null)
                return;

            spawnedEyes.Remove(spawnedEye);
            spawnedEye.Explode();
            health = Mathf.Clamp(health + Parameters.eye_HealPerEye, 0f, Parameters.maxHealth);
        }

        #endregion

        #region Listeners

        public void Knockback(Vector3 force)
        {
            ExternalForce += force;
        }

        public void GetHurt(HurtEventData hurtEventData)
        {
            string hurtDbg = $"Carcass hurt for {hurtEventData.multiplier} * {hurtEventData.critMultiplier}";
            if (hurtEventData.sourceWeapon != null)
            {
                hurtDbg += $" from {hurtEventData.sourceWeapon.name}";
            }

            if (hurtEventData.target != null)
            {
                hurtDbg += $" at location {hurtEventData.target.name}";
            }

            if(!string.IsNullOrEmpty(hurtEventData.hitter))
            {
                hurtDbg += $" from hitter {hurtEventData.hitter}";

                if (hurtEventData.hitter == "cannonball")
                    Stun();
            }

            Debug.Log(hurtDbg);

            //Impl: Crit damage
            health = Mathf.Max(0f, health - hurtEventData.multiplier);

            if (health <= 0f && !dead)
            {
                dead = true;
                StartCoroutine(DeathCoroutine());
            }
        }


        private void OnEyeDeath(Drone eye)
        {
            Debug.Log($"Eye killed! {spawnedEyes.Count}");
            if (!spawnedEyes.Contains(eye)) //If its not in the list, we probably healed from it.
                return;

            spawnedEyes.Remove(eye);

            if (isHealing)
                if (spawnedEyes.Count == 0)
                    Enrage();
        }

        #endregion

        #region Coroutines

        private void InvokeAfterTime(Action action, float time)
        {
            StartCoroutine(InvokeAfterTimeCoroutine(action, time));
        }

        private IEnumerator ShakeAttackCoroutine()
        {
            float timer = Parameters.shake_ProjectileBurstLengthInSeconds;
            int projectilesRemaining = Parameters.shake_ProjectileCount;
            float timerPerProjectile = timer / projectilesRemaining;

            while(projectilesRemaining > 0)
            {
                yield return new WaitForSeconds(timerPerProjectile);
                GameObject spread = new GameObject();
                ProjectileSpread spr = spread.AddComponent<ProjectileSpread>();
                spr.dontSpawn = true;
                spr.timeUntilDestroy = Parameters.shake_ProjectileBurstLengthInSeconds*2f;

                for(int i=0;i<Parameters.shake_ProjectileGroup;i++)
                {
                    FireTrackingProjectileHalo().transform.parent = spr.transform;
                }
                --projectilesRemaining;
            }

            isActioning = false;
        }

        private IEnumerator InvokeAfterTimeCoroutine(Action action, float time)
        {
            float timer = time;

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }

            action?.Invoke();
        }

        private IEnumerator InvokeAfterAnimation(Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            Func<bool> func = () =>
            {
                return Components.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
            };
            yield return new WaitWhile(func);

            onComplete?.Invoke();
        }
        private IEnumerator ActionFailsafe()
        {
            float timer = 0f;
            while (true)
            {
                if (isActioning)
                    timer += Time.deltaTime;
                else
                    timer = 0f;

                if (timer > Parameters.maxAttackTimeFailsafe && isActioning)
                    AttackDone();

                yield return new WaitForEndOfFrame();
            }
        }

        //Placeholder stuff
        private IEnumerator DeathCoroutine()
        {
            for (int i = 0; i < spawnedEyes.Count; i++)
            {
                if (spawnedEyes[i] == null)
                    continue;

                spawnedEyes[i].Explode();
            }

            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            float time = 1f;
            float timer = time;

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;

                transform.position = pos + UnityEngine.Random.onUnitSphere*0.15f;
            }

            timer = time;

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;

                float interval = 1 - (timer / time);
                transform.localScale = Vector3.Lerp(scale, new Vector3(0, scale.y, 0f), interval);
                transform.position = pos + UnityEngine.Random.onUnitSphere * 0.15f;
            }


            transform.localScale = Vector3.zero;
            Debug.Log("Carcass died.");

            GameObject.Destroy(gameObject);
        }

        #endregion

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        string lastActionName;

        //Shows state on screen
        private void OnGUI()
        {
            GUI.skin.box.fontSize = 35;
            GUI.skin.box.normal.textColor = Color.white;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;

            string stateName = $"AT: ({actionTimer}) ACT:{isActioning}";
            stateName += $"\nLA:{lastActionName}";
            stateName += $"\nHP:{health} HEAL:{isHealing}";
            stateName += $"\nEyeCD: {eyeRespawnTimer}";
            stateName += $"\nEyeCT: {spawnedEyes.Count}";
            stateName += $"\nRAGE: {Enraged}";
            GUILayout.Box(stateName.TrimEnd('\n', '\r'));
        }
    }
}
