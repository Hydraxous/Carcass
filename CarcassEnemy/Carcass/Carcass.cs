using CarcassEnemy.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarcassEnemy
{
    public class Carcass : MonoBehaviour, ICustomEnemy
    {
        //References
        [SerializeField] private CarcassComponents components;
        public CarcassComponents Components
        {
            get
            {
                if(components == null)
                    components = GetComponent<CarcassComponents>();
                return components;
            }
        }

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
                        parameters = CarcassCFG.GetParameters(); //L
                }
                return parameters;
            }
        }

        public void SetParams(CarcassParameters parameters)
        {
            this.parameters = parameters;
        }

        //Public
        public float Health => health;
        public bool IsAlive => health > 0;
        public bool IsEnraged { get; private set; } = false;
        public bool IsAttacking => isActioning;

        public bool IsDashing => dashTimeLeft > 0f;

        //internal state
        private float health;
        private bool dead;
        private bool isStunned;
        private bool isHealing;
        private bool isHooked;
        private float randomStrafeDirection;
        private float timeUntilDirectionChange = 2f;
        private float actionTimer = 3f;
        private float enrageTimer;
        private float dodgeCooldown;
        private float dashTimeLeft;
        private float currentDashBrakeForce;


        private bool isDodging;
        private float healCooldownTimer;
        private bool isActioning;
        private bool inModalAction;



        public Vector3 ExternalForce { get; private set; }
        private Vector3 dashDirection;
        private float eyeRespawnTimer;
        private Delegate lastAttack;

        private List<Drone> spawnedEyes = new List<Drone>();

        private GameObject summonCircle;
        private GameObject spawnedEnrageEffect;

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
                components.ProjectileDetector.OnProjectileDetected += OnProjectileDetected;
                components.SpinHitbox.OnTriggerEntered += OnHurtboxEnter;
                components.HookDetector.OnHookstateChanged += OnHookStateChanged;
                SetHitboxVisibility(CarcassCFG.HitboxesVisible);
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
            if (dead)
                return;

            if(target != null)
            {
                Vector3 pos = transform.position;
                Vector3 tgtPos = target.position;
                Vector3 toTarget = tgtPos- pos;
                targetDistance = toTarget.magnitude;
            }
            
            TimerUpdate();
        }

        private void FixedUpdate()
        {
            MovementUpdate();
        }

        private void LateUpdate()
        {
            if (target == null || isDodging || dead)
                return;

            TurnTowards(target.position, 20000f);
        }

        #endregion

        private void TimerUpdate()
        {
            float dt = Time.deltaTime;

            eyeRespawnTimer -= dt;
            timeUntilDirectionChange -= dt;
            dashTimeLeft -= dt;
            dodgeCooldown -= dt;

            if (timeUntilDirectionChange <= 0f)
            {
                ChangeStrafeDirection();
                timeUntilDirectionChange = Parameters.directionChangeDelay;
            }

            if (!DisableActionTimer)
                actionTimer = Mathf.Max(0, actionTimer - (dt * ((IsEnraged) ? Parameters.enrageAttackTimerMultiplier : 1f)));

            if (actionTimer <= 0f && !isStunned)
            {
                PerformAction();
                actionTimer = Parameters.attackDelay;
            }

            if(enrageTimer > 0f)
            {
                enrageTimer -= dt;
                if(enrageTimer <= 0f)
                    SetEnraged(false);
            }

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

            position += offset * Parameters.shakeProjectileOriginRadius;
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
                targetHeight = Mathf.Max(targetHeight, target.position.y);

            float distanceToDesiredHeight = targetHeight - position.y;
            return Mathf.Sign(distanceToDesiredHeight);
        }

        private void MovementUpdate()
        {
            SolveMovementDash();
            
            Vector3 velocity = GetVelocity();

            if (IsDashing && !dead)
                velocity = ApplyDashMovement(velocity);
            else if (isStunned || dead || isHealing)
                velocity = ApplyBrake(velocity);
            else
                velocity = ApplyMovement(velocity);

            Components.Rigidbody.velocity = velocity;
        }

        private void SolveMovementDash()
        {
            //When enraged, dash towards V1 if out of range.
            if (!IsEnraged || dead || IsDashing || target == null || isActioning)
                return;

            Vector3 pos = transform.position;
            Vector3 targetPos = target.position;
            Vector3 toTarget = targetPos - pos;
            float distance = toTarget.magnitude;

            if (distance < Parameters.enrageDashMaxRange)
                return;

            if (!TargetLineOfSightCheck())
                return;

            actionTimer += Parameters.enrageDashActionDelay;
            Components.Animation.Dash();
            Dash(toTarget, Parameters.enrageDashForce, Parameters.enrageDashBrakeForce, Parameters.enrageDashLength);
        }

        private bool TargetLineOfSightCheck()
        {
            if (target == null)
                return false;

            Vector3 pos = Components.CenterMass.position;
            Vector3 targetPos = target.position;
            Vector3 toTarget = targetPos - pos;

            return !Physics.Raycast(pos, toTarget, toTarget.magnitude, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore);
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
            velocity = Vector3.MoveTowards(velocity, moveDelta, Time.fixedDeltaTime * Parameters.movementSmoothing);
            return velocity;
        }


        private Vector3 ApplyBrake(Vector3 velocity)
        {
            return Vector3.MoveTowards(velocity, Vector3.zero, Time.fixedDeltaTime * Parameters.movementSmoothing);
        }

        private Vector3 ApplyDashMovement(Vector3 velocity)
        {
            return velocity + CalcDrag(velocity, currentDashBrakeForce) * Time.fixedDeltaTime;
        }

        private Vector3 CalcDrag(Vector3 velocity, float drag)
        {
            float speed = velocity.magnitude;
            float dragForceMagnitude = (speed * speed) * drag;
            return dragForceMagnitude * -velocity.normalized;
        }

        private float GetMoveSpeedMultiplier()
        {
            if (isActioning)
                return Parameters.speedWhileAttackingMultiplier;

            if (IsEnraged)
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
            if (spawnedEyes.Count < Parameters.eyeSpawnCount && eyeRespawnTimer <= 0f && !IsEnraged)
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

            bool hasLineOfSight = TargetLineOfSightCheck();

            if (healCooldownTimer <= 0f)
                if (spawnedEyes.Count > 0)
                    if (health < Parameters.lowHealthThreshold*Parameters.maxHealth)
                        attackPool.Add(HealAction);

            //In range and higher up than the player
            if (lateralDistance < Parameters.spinMaxRange && verticalDistance < 0 && hasLineOfSight)
                if(IsEnraged && Parameters.enableEnrageWildAttacks)
                    attackPool.Add(CarpetBomb);
                else
                    attackPool.Add(SpinAttack);


            if((verticalDistance < 0f || !hasLineOfSight || IsEnraged) && summonCircle == null || IsEnraged)
                attackPool.Add(SummonSigil);

            if (IsEnraged && Parameters.enableEnrageWildAttacks)
                attackPool.Add(BarrageAttack);
            else
                attackPool.Add(ShakeAttack);

            if (lastAttack != null)
                attackPool.Remove(lastAttack);

            if(attackPool.Count == 0)//do nothing
                return;

            try
            {
                int index = UnityEngine.Random.Range(0, attackPool.Count);

                string msg = "--ATK--";
                for( int i = 0; i < attackPool.Count; i++)
                {
                    if (attackPool[i] != null)
                        msg += $"\n[{i}] {attackPool[i].ToString()}";
                    else
                        msg += $"\n[{i}] NULL";
                }

                Debug.LogWarning(msg);
                lastAttack = attackPool[index];
                lastAttack.DynamicInvoke();
            }
            catch (Exception e) 
            {
                Debug.LogWarning("???");
                Debug.LogException(e);
            }
        }

        public void SpinAttack()
        {
            lastActionName = "LobYellow";
            isActioning = true;
            inModalAction = true;
            Components.Animation.Spin();
            Components.SpinHitbox.gameObject.SetActive(true);

            StartCoroutine(InvokeAfterAnimation(() =>
            {
                AttackDone();
                Components.SpinHitbox.gameObject.SetActive(false);
            }));
        }

        public void Stun()
        {
            if (dead)
                return;

            isStunned = true;
            isActioning = false;
            isHealing = false;
            StopAllCoroutines();
            Components.Animation.Stunned();

            if (IsEnraged)
                enrageTimer += Parameters.enrageAddEnrageTimeOnStun;

            StartCoroutine(InvokeAfterAnimation(StopStun));
        }

        private void StopStun()
        {
            isStunned = false;
        }

        public void OnProjectileDetected(Collider col)
        {
            //Dodge directionally

            bool canDodge = CanDodge();
            Debug.Log($"Projectile Detect ({col.name}) CAN_DODGE: {canDodge}");

            if (!canDodge)
                return;

            Vector3 projPosition = col.transform.position;
            Vector3 pos = transform.position;
            Vector3 toProjectile = projPosition - pos;
            
            toProjectile = toProjectile.XZ().normalized;
            
            //Extremely delightful
            float signedAngle = Vector3.SignedAngle(transform.forward.XZ().normalized, toProjectile, Vector3.up);
            Debug.Log($"Signed Ange:{signedAngle}");

            Vector3 dodgeVec = transform.right * -Mathf.Sign(signedAngle);
            Dodge(dodgeVec);
        }

        private bool CanDodge()
        {
            bool canDodge = !isDodging && !inModalAction && dodgeCooldown <= 0f;

            if (!canDodge)
                return false;

            if (target == null)
                return true;

            Vector3 targetPosition = target.position;
            Vector3 pos = transform.position;
            Vector3 toTarget = targetPosition - pos;

            return toTarget.magnitude > Parameters.dodgeMinRange;
        }

        public void Dodge(Vector3 direction)
        {
            dodgeCooldown = Parameters.dodgeCooldownTime;
            actionTimer += Parameters.dodgeActionTimerAddition;
            lastActionName = "Dodge";
            isActioning = true;
            isDodging = true;
            SetHitboxesActive(false);
            Components.Animation.Dodge();

            Dash(direction, Parameters.dodgeForce, Parameters.dodgeBrakeSpeed, Parameters.dodgeLength);

            InvokeAfterTime(() =>
            {
                SetHitboxesActive(true);
            }, dashTimeLeft/2f);

            InvokeAfterTime(()=>
            {
                AttackDone();
                isDodging = false;
            }, dashTimeLeft);
        }


        public void Dash(Vector3 direction, float force, float brakeForce, float length)
        {
            dashDirection = direction.normalized;
            dashTimeLeft = length;
            currentDashBrakeForce = brakeForce;
            Components.Rigidbody.velocity = direction.normalized * force;
        }

        private void SetHitboxesActive(bool enabled)
        {
            foreach (GameObject gameObject in Components.Hitboxes)
            {
                gameObject.SetActive(enabled);
            }
        }

        public void ShakeAttack()
        {
            lastActionName = "BlueAttack";
            isActioning = true;
            Components.Animation.Shake();
            StartCoroutine(ShakeAttackCoroutine());
        }

        public void BarrageAttack()
        {
            lastActionName = "Barrage";
            isActioning = true;
            Components.Animation.Writhe();
            StartCoroutine(EnragedBarrageCoroutine());
        }

        public void CarpetBomb()
        {
            lastActionName = "CarpetBomb";
            isActioning = true;
            Components.Animation.Spin();
            StartCoroutine(EnragedCarpetBomb());
        }

        public void SummonSigil()
        {
            lastActionName = "SummonSigil";
            isActioning = true;
            Components.Animation.Summon();
            StartCoroutine(InvokeAfterAnimation(AttackDone));
        }

        public void Heal(float amount)
        {
            if (dead)
                return;

            health = Mathf.Clamp(health + amount, 0f, Parameters.maxHealth);
        }

        public void SpawnSigil()
        {
            GameObject newSigil = GameObject.Instantiate(Components.SummonCirclePrefab);
            SummonCircle summonCircle = newSigil.GetComponent<SummonCircle>();
            summonCircle.SetTarget(target);
            summonCircle.SetOwner(this);
            this.summonCircle = newSigil;
        }

        public void SpawnEyes()
        {
            lastActionName = "SpawnEyes";
            eyeRespawnTimer = Parameters.eyeSpawnCooldown;
            isActioning = true;
            Components.Animation.Writhe();

            int eyesToSpawn = Parameters.eyeSpawnCount - spawnedEyes.Count;

            for (int i = 0; i < eyesToSpawn; i++)
            {
                float delay = Parameters.eyeSpawnDelay * (i + 1);
                InvokeAfterTime(SpawnSingleEye, delay);
            }

            StartCoroutine(InvokeAfterAnimation(AttackDone));
        }

        public void FireExplosiveProjectile()
        {
            Transform tf = Components.ProjectileLobPoint;

            Vector3 position = tf.position;
            Vector3 direction = tf.forward;

            Projectile projectile = GameObject.Instantiate<GameObject>(UKPrefabs.LobbedProjectileExplosiveHH.Asset, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
            projectile.target = this.target;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb?.AddForce(transform.up * 50f, ForceMode.VelocityChange);
            projectile.safeEnemyType = EnemyType.Mindflayer; //Hack
                                                             //projectile.transform.SetParent(base.GetComponentInParent<GoreZone>().transform, true);
            projectile.speed *= Components.EnemyIdentifier.totalSpeedModifier;
            projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;
        }

        public Projectile FireTrackingProjectileSpherical()
        {
            Transform tf = Components.CenterMass;

            Vector3 position = tf.position;
            Vector3 offset = UnityEngine.Random.onUnitSphere;

            position += offset * Parameters.shakeProjectileOriginRadius;

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
            SetEnraged(true);
        }

        public void Instakill()
        {
            Die();
        }

        public void AttackDone()
        {
            isActioning = false;
            inModalAction= false;
        }

        private void SpawnSingleEye()
        {
            Vector3 position = GetRingSpawnPosition();
            GameObject eyeObject = GameObject.Instantiate(UKPrefabs.DroneFlesh.Asset, position, Quaternion.identity);

            if (eyeObject.TryGetComponent<Drone>(out Drone drone))
            {
                spawnedEyes.Add(drone);
                drone.health = Parameters.eyeHealth;
            }

            if (eyeObject.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier enemyIdentifier))
            {
                enemyIdentifier.dontCountAsKills = true;
                enemyIdentifier.health = Parameters.eyeHealth;
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
            inModalAction = true;

            healCooldownTimer = Parameters.healCooldown;
            Components.Animation.KillEyes();

            int eyeCount = spawnedEyes.Count;

            InvokeAfterTime(() =>
            {
                for (int i = 0; i < eyeCount; i++)
                {
                    Debug.LogWarning("Started Sacrificing EYE");
                    InvokeAfterTime(SacrificeEyeForHealth, (0.01f+Parameters.eyeHealDelay) * (i));
                }

            }, Parameters.eyeInitialHealDelay);

            StartCoroutine(InvokeAfterAnimation(() =>
            {
                isHealing = false;
                AttackDone();
            }));

        }

        private void SacrificeEyeForHealth()
        {
            Debug.LogWarning("EYE SACRIFICE TRY!");
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
            Heal(Parameters.eyeHealPerEye);
        }

        private void SetEnraged(bool enraged)
        {
            bool wasEnraged = this.IsEnraged;
            this.IsEnraged = enraged;

            if(!IsEnraged)
            {
                enrageTimer = 0f;

                if (spawnedEnrageEffect != null)
                    GameObject.Destroy(spawnedEnrageEffect.gameObject);
            }
            else if(!wasEnraged)
            {
                enrageTimer = Parameters.enrageLength;
                spawnedEnrageEffect = GameObject.Instantiate(UKPrefabs.RageEffect.Asset, Components.CenterMass);
            }

            Debug.Log($"Carcass enraged {IsEnraged}");
        }

        #endregion

        #region Listeners

        public void SetHitboxVisibility(bool visible)
        {
            foreach (GameObject hitboxObject in Components.Hitboxes)
            {
                MeshRenderer mr = hitboxObject.GetComponent<MeshRenderer>();
                mr.enabled = visible;
            }
        }

        public void Knockback(Vector3 force)
        {
            ExternalForce += force;
        }

        public void GetHurt(HurtEventData hurtEventData)
        {
            float damage = CalcDamage(hurtEventData);
            string hurtDbg = $"Carcass hurt for m:{hurtEventData.multiplier} * c:{hurtEventData.critMultiplier}";
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

            hurtDbg += $" for a total dmg of {damage}";

            Debug.Log(hurtDbg);

            float lastHealth = health;

            health = Mathf.Max(0f, health - damage);

            float lowHealth = Parameters.maxHealth * Parameters.lowHealthThreshold;
            if (health < lowHealth && lastHealth >= lowHealth)
            {
                SetEnraged(true);
            }

            if (health <= 0f && !dead)
            {
                Die();
                return;
            }

            OnHurtGore(hurtEventData);
            OnHurtStyle(hurtEventData);
        }


        private void OnEyeDeath(Drone eye)
        {
            Debug.Log($"Eye killed! {spawnedEyes.Count}");
            if (!spawnedEyes.Contains(eye)) //If its not in the list, we probably healed from it.
                return;

            spawnedEyes.Remove(eye);

            if(CarcassHealOrb.EnableOrbs)
            {
                Vector3 eyePosition = eye.transform.position;

                GameObject healOrbObject = GameObject.Instantiate(Components.HealOrbPrefab, eyePosition, Quaternion.identity);
                if (healOrbObject.TryGetComponent<CarcassHealOrb>(out CarcassHealOrb healOrb))
                    healOrb.SetOwner(this);
            }
            

            if (spawnedEyes.Count == 0)
                Enrage();
        }

        private void OnHurtboxEnter(Collider col)
        {
            if (!col.CompareTag("Player"))
                return;

            Vector3 pos = Components.CenterMass.position;
            Vector3 playerPos = NewMovement.Instance.transform.position;

            Vector3 toPlayer = playerPos - pos;
            Vector3 knockForce = toPlayer.XZ().normalized * Parameters.spinMeleeKnockback;

            NewMovement.Instance.GetHurt(Parameters.spinMeleeDamage, false);
            NewMovement.Instance.Launch(knockForce);
        }

        #endregion

        #region Coroutines

        private Coroutine InvokeAfterTime(Action action, float time)
        {
            return StartCoroutine(InvokeAfterTimeCoroutine(action, time));
        }

        private IEnumerator ShakeAttackCoroutine()
        {
            float timer = Parameters.shakeProjectileBurstLengthInSeconds;
            int projectilesRemaining = Parameters.shakeProjectileCount;
            float timerPerProjectile = timer / projectilesRemaining;

            while(projectilesRemaining > 0)
            {
                yield return new WaitForSeconds(timerPerProjectile);
                GameObject spread = new GameObject();
                ProjectileSpread spr = spread.AddComponent<ProjectileSpread>();
                spr.dontSpawn = true;
                spr.timeUntilDestroy = Parameters.shakeProjectileBurstLengthInSeconds*2f;

                int projectileCount = (IsEnraged) ? Parameters.enrageBlueProjectileCount : Parameters.shakeProjectileGroup;

                for(int i=0;i<projectileCount;i++)
                {
                    FireTrackingProjectileHalo().transform.parent = spr.transform;
                }
                --projectilesRemaining;
            }

            isActioning = false;
        }

        private IEnumerator EnragedBarrageCoroutine()
        {
            float timer = Parameters.barrageAttackLength;
            int projectilesRemaining = Parameters.barrageProjectileCount;
            GameObject spread = new GameObject();
            ProjectileSpread spr = spread.AddComponent<ProjectileSpread>();
            spr.dontSpawn = true;
            spr.timeUntilDestroy = (Parameters.barrageAttackProjectileDelay * Parameters.barrageProjectileCount) + Parameters.barrageAttackLength;

            while (projectilesRemaining > 0)
            {
                yield return new WaitForSeconds(Parameters.barrageAttackProjectileDelay);
                FireTrackingProjectileSpherical().transform.parent = spr.transform;
                --projectilesRemaining;
            }

            isActioning = false;
        }

        private IEnumerator EnragedCarpetBomb()
        {
            float timer = Parameters.carpetBombLength;
            float projectileCountf = timer / (Parameters.carpetBombProjectileDelay+Mathf.Epsilon); //DBZ error prevent
            float projectileDelay = Parameters.carpetBombProjectileDelay;
            int projectileCount = Mathf.FloorToInt(projectileCountf);

            while(projectileCount > 0)
            {
                yield return new WaitForSeconds(projectileDelay);
                FireExplosiveProjectile();
                --projectileCount;
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
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            float time = 2f;
            float timer = time;

            Vector3 offset = Vector3.zero;

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
                transform.position = offset + pos + UnityEngine.Random.onUnitSphere*0.15f;
                offset += Vector3.up * Time.deltaTime;
            }

            Debug.Log("Carcass died.");
            GameObject.Destroy(gameObject);
        }

        private void Die()
        {
            if (dead)
                return;

            StopAllCoroutines();

            dead = true;
            Components.EnemyIdentifier.dead = true;

            Components.Animation.Writhe();
            for (int i = 0; i < spawnedEyes.Count; i++)
            {
                if (spawnedEyes[i] == null)
                    continue;

                spawnedEyes[i].Explode();
            }

            StartCoroutine(DeathCoroutine());
        }


        #endregion

        #region GoreFix Nonsense

        private float GetLocationDamage(string location)
        {
            if (string.IsNullOrEmpty(location))
                return 0f;

            if (location == "Head")
                return 1f;

            if (location == "Limb" || location == "EndLimb")
                return 0.5f;

            return 0f;
        }

        private float CalcDamage(HurtEventData hurtData)
        {
            return CalcDamage(hurtData.multiplier, GetLocationDamage(hurtData.target.tag), hurtData.critMultiplier);
        }

        private float CalcDamage(float damageMultiplier, float locationDamage, float critMultiplier)
        {
            return damageMultiplier + locationDamage * damageMultiplier * critMultiplier;
        }

        //Frankensteined from Machine.cs
        private void OnHurtGore(HurtEventData hurtData)
        {
            GameObject gore = null;

            float locationDamage = GetLocationDamage(hurtData.target.tag);
            float damage = CalcDamage(hurtData.multiplier, locationDamage, hurtData.critMultiplier);

            if (hurtData.hitter == "fire" || damage <= 0f)
                return;

            GoreType? goreType = null;

            if (locationDamage == 1f && (damage >= 1f || this.health <= 0f))
            {
                goreType = GoreType.Head;
            }
            else if (((damage >= 1f || this.health <= 0f) && hurtData.hitter != "explosion") || (hurtData.hitter == "explosion" && hurtData.target.tag == "EndLimb"))
            {
                if (hurtData.target.CompareTag("Body"))
                    goreType = GoreType.Body;
                else
                    goreType = GoreType.Limb;
            }
            else if (Components.EnemyIdentifier.hitter != "explosion")
                goreType = GoreType.Small;

            if (goreType != null)
                gore = BloodsplatterManager.Instance.GetGore(goreType.Value, Components.EnemyIdentifier.underwater, Components.EnemyIdentifier.sandified, Components.EnemyIdentifier.blessed);

            GoreZone goreZone = null;

            if (gore == null)
                return;

            goreZone = (goreZone == null) ? GoreZone.ResolveGoreZone(transform) : goreZone;

            gore.transform.position = hurtData.target.transform.position;
            if (Components.EnemyIdentifier.hitter == "drill")
            {
                gore.transform.localScale *= 2f;
            }

            if (goreZone != null && goreZone.goreZone != null)
            {
                gore.transform.SetParent(goreZone.goreZone, true);
            }

            if (!gore.TryGetComponent<Bloodsplatter>(out Bloodsplatter bloodSplatter))
                return;
         
            ParticleSystem.CollisionModule collision = bloodSplatter.GetComponent<ParticleSystem>().collision;
            if (hurtData.hitter == "shotgun" || hurtData.hitter == "shotgunzone" || hurtData.hitter == "explosion")
            {
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                {
                    collision.enabled = false;
                }
                bloodSplatter.hpAmount = 3;
            }
            else if (hurtData.hitter == "nail")
            {
                bloodSplatter.hpAmount = 1;
                bloodSplatter.GetComponent<AudioSource>().volume *= 0.8f;
            }

            bloodSplatter.GetReady();

        }

        private void OnHurtStyle(HurtEventData hurtData)
        {
            if (hurtData.hitter == "enemy")
                return;
            StyleCalculator scalc = MonoSingleton<StyleCalculator>.Instance;

            if (dead)
            {
                if (hurtData.hitter == "explosion" || hurtData.hitter == "ffexplosion" || hurtData.hitter == "railcannon")
                {
                    scalc.shud.AddPoints(120, "ultrakill.fireworks", hurtData.sourceWeapon, Components.EnemyIdentifier, -1, "", "");
                }
                else if (hurtData.hitter == "ground slam")
                {
                    scalc.shud.AddPoints(160, "ultrakill.airslam", hurtData.sourceWeapon, Components.EnemyIdentifier, -1, "", "");
                }
                else if (hurtData.hitter != "deathzone")
                {
                    scalc.shud.AddPoints(50, "ultrakill.airshot", hurtData.sourceWeapon, Components.EnemyIdentifier, -1, "", "");
                }
            }

            if (hurtData.hitter == "secret")
                return;

            scalc.HitCalculator(hurtData.hitter, "machine", hurtData.target.tag, dead, Components.EnemyIdentifier, hurtData.sourceWeapon);
        }

        private void OnHookStateChanged(bool isHooked)
        {
            this.isHooked = isHooked;
            if(isHooked)
            {
                InvokeAfterTime (() =>
                {
                    if (!Components.HookDetector.IsHooked)
                        return;

                    Debug.Log("Chomp!!");
                    Components.HookDetector.ForceUnhook();

                    FieldInfo cooldown = typeof(HookArm).GetField("cooldown", BindingFlags.NonPublic | BindingFlags.Instance);
                    //NewMovement.Instance.ForceAddAntiHP(Parameters.hookBiteYellowHP); TODO: Difficulty
                    CameraController.Instance.CameraShake(0.8f);
                    cooldown.SetValue(HookArm.Instance, Parameters.hookPlayerCooldown);

                }, Parameters.hookBiteDelay);
            }
        }

        #endregion

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        string lastActionName;

        private float targetDistance;

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
            stateName += $"\nDg: {isDodging} DT:{dashTimeLeft}";
            stateName += $"\nMA: {inModalAction}";
            stateName += $"\nRAGE: {IsEnraged}";
            stateName += $"\nTGT: {targetDistance.ToString("0.00")} M";
            GUILayout.Box(stateName.TrimEnd('\n', '\r'));
        }
    }
}
