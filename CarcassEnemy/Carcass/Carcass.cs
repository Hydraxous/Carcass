using CarcassEnemy.Assets;
using CarcassEnemy.Components;
using Sandbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CarcassEnemy
{
    [RequireComponent(typeof(CarcassComponents))]
    public class Carcass : MonoBehaviour, IEnemy
    {
        //References
        [SerializeField] private CarcassComponents components;
        [SerializeField] private Transform target;
        [SerializeField] private CarcassParametersAsset serializedParameters;
        [SerializeField] private bool disableDeathSequence;
        [SerializeField] private bool dontAttackPlayer;

        private CarcassParameters parameters;
        public event Action<Carcass> OnDeath;

        //Public
        public float Health => health;
        public bool IsEnraged { get; private set; } = false;
        public bool IsDashing => dashTimeLeft > 0f;
        public bool Dead => isDead;
        public bool IsAlive() => !isDead;

        //State
        private float health;
        private float randomStrafeDirection;
        private float currentDashBrakeForce;

        private bool isDead;
        private bool isHealing;
        private bool isHooked;
        private bool isStunned;
        private bool isDodging;
        private bool isBlind;
        private bool isActioning;
        private bool inModalAction;

        //Timers
        private float dodgeCooldown;
        private float dashTimeLeft;
        private float hookTimer;
        private float enrageTimer;
        private float actionTimer = 3f;
        private float timeUntilDirectionChange = 2f;
        private float healCooldownTimer;
        private float eyeRespawnTimer;
        private float targetCheckTimer = 1f;

        private Delegate lastAttack;

        private Action onInterupt;
        private Action onActionEnd;

        //External
        private List<Drone> spawnedEyes = new List<Drone>();
        private List<GameObject> activeSigils = new List<GameObject>();
        private GameObject spawnedEnrageEffect;

        private EnemyIdentifier targetedEnemy;
        private static FieldInfo droneTargetField = typeof(Drone).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo droneFleshTargetField = typeof(DroneFlesh).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic);


        #region UnityMessages

        private void Awake()
        {
            Patcher.QueryInstance();
            health = Parameters.maxHealth;
            Components.Machine.health = Parameters.maxHealth;
            foreach (GameObject go in components.Hitboxes)
            {
                Rigidbody rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (Components.ProjectileDetector != null)
                Components.ProjectileDetector.OnProjectileDetected += OnProjectileDetected;

            if (Components.SpinHitbox != null)
                Components.SpinHitbox.OnTriggerEntered += OnHurtboxEnter;

            if (Components.HookDetector != null)
                Components.HookDetector.OnHookstateChanged += OnHookStateChanged;

            //SetHitboxVisibility(CarcassCFG.HitboxesVisible);

            if (Components.EnemyIdentifier.spawnEffect == null)
                Components.EnemyIdentifier.spawnEffect = UKPrefabs.SpawnEffect.Asset;

            if(SceneHelper.CurrentScene == "Endless") //Dont do cinematic death in Cybergrind please
            {
                disableDeathSequence = true;
            }

        }

        private void Start()
        {
            ResolveTarget();
            StartCoroutine(ActionFailsafe());
            SummonEyes();
        }

        private void Update()
        {
            isBlind = ULTRAKILL.Cheats.BlindEnemies.Blind;
            TimerUpdate();
        }

        private void FixedUpdate()
        {
            MovementUpdate();
        }

        private void LateUpdate()
        {
            if (target == null || isDodging || isDead || isBlind)
                return;

            TurnTowards(target.position, 20000f);
        }

        #endregion
        
        

        private void TimerUpdate()
        {
            if (isDead)
                return;

            float dt = Time.deltaTime;

            eyeRespawnTimer -= dt;
            timeUntilDirectionChange -= dt;
            dashTimeLeft -= dt;
            dodgeCooldown -= dt;

            //Bite hook
            float hookBiteTriggerTime = Parameters.hookBiteDelay / ((IsEnraged) ? Parameters.enrageHookBiteSpeedMultiplier : 1);
            hookTimer = Mathf.Clamp((hookTimer + (isHooked ? dt : -dt)), 0f, hookBiteTriggerTime);
            if(hookTimer >= hookBiteTriggerTime && !isBlind)
            {
                BiteHook();
                hookTimer = 0f;
            }

            if (timeUntilDirectionChange <= 0f)
            {
                ChangeStrafeDirection();
                timeUntilDirectionChange = Parameters.directionChangeDelay;
            }

            if(!isBlind)
                actionTimer = Mathf.Max(0, actionTimer - (dt * ((IsEnraged) ? Parameters.enrageAttackTimerMultiplier : 1f)));

            if (actionTimer <= 0f && !isStunned && !isBlind)
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

            if(targetCheckTimer > 0f)
            {
                targetCheckTimer -= dt;
                if (targetCheckTimer <= 0f)
                {
                    ResolveTarget();
                    targetCheckTimer = Parameters.targetCheckDelay;
                }
            }
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
            {
                if (target == null)
                    return -1f;

                return Mathf.Sign(target.position.y - position.y);
            }

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

            if (isDead || isHealing || isStunned || isBlind || target == null)
                velocity = ApplyBrake(velocity);
            else if(IsDashing)
                velocity = ApplyDashMovement(velocity);
            else
                velocity = ApplyMovement(velocity);

            Components.Rigidbody.velocity = velocity;
        }

        private void SolveMovementDash()
        {
            //When enraged, dash towards V1 if out of range.
            if (!IsEnraged || isDead || IsDashing || target == null || isActioning || isBlind)
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

        private Vector3 GetVelocity()
        {
            return Components.Rigidbody.velocity;
        }

        private Vector3 ApplyMovement(Vector3 velocity)
        {
            Vector3 travelVector = transform.right * randomStrafeDirection;

            //If we see a wall in the direction we're moving, flip the strafe direction.
            if(Physics.Raycast(Components.CenterMass.position, travelVector, out RaycastHit hit, Parameters.strafeObstacleCheckDistance, LayerMaskDefaults.Get(LMD.Environment)))
                if(hit.distance < Parameters.strafeObstacleCheckDistance*0.5f)
                {
                    randomStrafeDirection = -randomStrafeDirection;
                    travelVector = -travelVector;
                }


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

        private void Dash(Vector3 direction, float force, float brakeForce, float length)
        {
            dashTimeLeft = length;
            currentDashBrakeForce = brakeForce;
            Components.Rigidbody.velocity = direction.normalized * force;
        }

        #endregion

        #region Actions
        private void PerformAction()
        {
            //If can spawn eyes. Spawn eyes.
            if (spawnedEyes.Count < Parameters.eyeSpawnCount && eyeRespawnTimer <= 0f && !IsEnraged)
            {
                SummonEyes();
                return;
            }

            if (target == null)
                return;

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
                        attackPool.Add(StartHealing);

            //In range and higher up than the player
            //if (lateralDistance < Parameters.spinMaxRange && verticalDistance < 0 && hasLineOfSight)

            if (IsEnraged && Parameters.enableEnrageWildAttacks)
                attackPool.Add(CarpetBomb);
            else if(hasLineOfSight)
                attackPool.Add(SpinAttack);

            if((verticalDistance < 0f || !hasLineOfSight || IsEnraged) && (activeSigils.Count == 0 || IsEnraged))
                attackPool.Add(SummonSigil);

            if (IsEnraged && Parameters.enableEnrageWildAttacks)
                attackPool.Add(BarrageAttack);
            else if(hasLineOfSight)
                attackPool.Add(ShakeAttack);

            if (lastAttack != null)
                attackPool.Remove(lastAttack);

            if (attackPool.Count == 0)//do nothing
            {
                lastAttack = null;
                return;
            }
           
            int index = UnityEngine.Random.Range(0, attackPool.Count);
            lastAttack = attackPool[index];
            lastAttack.DynamicInvoke();
        }

        public void ShakeAttack()
        {
            Components.Animation.Shake();
            ActionStart();

            if(Components.PsychosisFXPrefab != null)
            {
                GameObject psychosisFX = GameObject.Instantiate(Components.PsychosisFXPrefab, Components.CenterMass);

                Action destroyPsychosisFX = () =>
                {
                    if (psychosisFX != null)
                        GameObject.Destroy(psychosisFX);
                };

                onInterupt = destroyPsychosisFX;
                onActionEnd = destroyPsychosisFX;
            }

            StartCoroutine(ShakeAttackCoroutine());
        }

        public void SpinAttack()
        {
            isActioning = true;
            inModalAction = true;
            Components.Animation.Spin();
            Components.SpinHitbox.gameObject.SetActive(true);

            StartCoroutine(InvokeAfterAnimation(() =>
            {
                ActionEnd();
                Components.SpinHitbox.gameObject.SetActive(false);
            }));
        }

        public void SummonSigil()
        {
            Components.Animation.Summon();
            ActionStart();
            StartCoroutine(InvokeAfterAnimation(ActionEnd));
        }

        public void SummonEyes()
        {
            eyeRespawnTimer = Parameters.eyeSpawnCooldown;
            isActioning = true;
            Components.Animation.SetVibrating(true);
            Components.Animation.Writhe();

            int eyesToSpawn = Parameters.eyeSpawnCount - spawnedEyes.Count;

            for (int i = 0; i < eyesToSpawn; i++)
            {
                float delay = Parameters.eyeSpawnDelay * (i + 1);
                InvokeDelayed(SpawnEye, delay);
            }

            StartCoroutine(InvokeAfterAnimation(ActionEnd));
        }

        public void BarrageAttack()
        {
            Components.Animation.Writhe();
            ActionStart();
            StartCoroutine(EnragedBarrageCoroutine());
        }

        public void CarpetBomb()
        {
            Components.Animation.Spin();
            ActionStart();
            StartCoroutine(EnragedCarpetBomb());
        }

        public void StartHealing()
        {
            isActioning = true;
            isHealing = true;
            inModalAction = true;

            healCooldownTimer = Parameters.healCooldown;
            Components.Animation.KillEyes();

            Action endHealing = null;

            if(Components.HealAuraFX != null)
            {
                GameObject healAuraFX = GameObject.Instantiate(Components.HealAuraFX, Components.CenterMass);

                endHealing = () =>
                {
                    if (healAuraFX != null)
                        GameObject.Destroy(healAuraFX);

                    isHealing = false;
                };

                onInterupt = endHealing;
            }

            

            int eyeCount = spawnedEyes.Count;
            float totalHealTime = (Parameters.eyeHealDelay * eyeCount) + Parameters.eyeInitialHealDelay;

            InvokeDelayed(() =>
            {
                for (int i = 0; i < eyeCount; i++)
                {
                    InvokeDelayed(SacrificeEyeForHealth, (Parameters.eyeHealDelay) * (i));
                }

            }, Parameters.eyeInitialHealDelay);

            InvokeDelayed(endHealing, totalHealTime + 0.2f);

            StartCoroutine(InvokeAfterAnimation(() =>
            {
                ActionEnd();
            }));
        }

        public void InterruptAction()
        {
            StopAllCoroutines();
            StartCoroutine(ActionFailsafe());
            onInterupt?.Invoke();
            onInterupt = null;
        }

        public void Stun()
        {
            if (isDead)
                return;

            isStunned = true;
            isActioning = false;
            inModalAction = false;
            isHealing = false;

            //possible cleanup
            InterruptAction();
            ActionEndCallback();

            if(Components.StunnedFX != null)
                GameObject.Instantiate(Components.StunnedFX, Components.CenterMass);

            Components.Animation.Stunned();
            Components.Animation.SetVibrating(false);

            if (IsEnraged)
                enrageTimer += Parameters.enrageAddEnrageTimeOnStun;
         
            onActionEnd = () => { isStunned = false; };
        }

        public bool CanDodge()
        {
            if (isDead)
                return false;

            bool canDodge = !isDodging && !inModalAction && dodgeCooldown <= 0f && !isBlind;

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
            isActioning = true;
            isDodging = true;
            SetHitboxesActive(false);
            Components.Animation.Dodge();

            Dash(direction, Parameters.dodgeForce, Parameters.dodgeBrakeSpeed, Parameters.dodgeLength);

            InvokeDelayed(() =>
            {
                SetHitboxesActive(true);
            }, dashTimeLeft/2f);

            InvokeDelayed(()=>
            {
                ActionEnd();
                isDodging = false;
            }, dashTimeLeft);
        }

        public void SetHitboxesActive(bool enabled)
        {
            foreach (GameObject gameObject in Components.Hitboxes)
            {
                gameObject.SetActive(enabled);
            }
        }

        public void ActionStart()
        {
            isActioning = true;
            Components.Animation.SetVibrating(true);
        }

        public void ActionEnd()
        {
            isActioning = false;
            inModalAction = false;
            Components.Animation.SetVibrating(false);
            ActionEndCallback();
        }

        private void ActionEndCallback()
        {
            if (onActionEnd != null)
            {
                onActionEnd.Invoke();
                onActionEnd = null;
            }
        }

        public void Heal(float amount)
        {
            if (isDead)
                return;

            health = Mathf.Clamp(health + amount, 0f, Parameters.maxHealth);

            if (health <= 0f)
            {
                Die();
                return;
            }

            if (amount > 0)
            {
                if(Components.HealFX != null)
                    GameObject.Instantiate(Components.HealFX, Components.CenterMass);
            }
        }

        public void Enrage()
        {
            if (isDead || gameObject == null)
                return;

            SetEnraged(true);
        }

        public void SacrificeEyeForHealth()
        {
            if (isStunned || isDead || !isHealing)
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
            Vector3 eyePosition = spawnedEye.transform.position;
            spawnedEye.Explode();
            Heal(Parameters.eyeHealPerEye);
            if(Components.GenericSpawnFX != null) 
                GameObject.Instantiate(Components.GenericSpawnFX, eyePosition, Quaternion.identity);
        }

        public void SetEnraged(bool enraged)
        {
            bool wasEnraged = this.IsEnraged;
            this.IsEnraged = enraged;

            if(!IsEnraged)
            {
                Components?.MaterialChanger?.ResetMaterials();

                enrageTimer = 0f;
                if (spawnedEnrageEffect != null)
                    GameObject.Destroy(spawnedEnrageEffect.gameObject);
            }
            else if(!wasEnraged)
            {
                if(Components.EnragedMaterials != null)
                    Components?.MaterialChanger?.SetMaterialSet(Components.EnragedMaterials);

                enrageTimer = Parameters.enrageLength;
                spawnedEnrageEffect = GameObject.Instantiate(UKPrefabs.RageEffect.Asset, Components.CenterMass);
            }
        }

        public void BiteHook()
        {
            if (!isHooked)
                return;

            if (activeSigils.Count > 0)
                return;

            Vector3 pos = Components.CenterMass.position;
            Vector3 player = target.position;
            Vector3 toPlayer = player - pos;

            if(Components.HookSnapFX != null)
                GameObject.Instantiate(Components.HookSnapFX, Components.CenterMass).transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);

            Components.HookDetector.ForceUnhook();

            FieldInfo cooldown = typeof(HookArm).GetField("cooldown", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if(Parameters.hookBiteYellowHP > 0)
                NewMovement.Instance.ForceAddAntiHP(Parameters.hookBiteYellowHP);

            CameraController.Instance.CameraShake(1.25f);
            
            if(HookArm.Instance != null)
                cooldown.SetValue(HookArm.Instance, Parameters.hookPlayerCooldown);
        }

        public void Instakill()
        {
            Die();
        }

        public void ForceCountDeath()
        {
            if (Components.EnemyIdentifier.dontCountAsKills)
                return;

            GoreZone gz = GoreZone.ResolveGoreZone(transform);

            if (gz != null && gz.checkpoint != null)
            {
                gz.AddDeath();
                gz.checkpoint.sm.kills++;
            }
            else
            {
                MonoSingleton<StatsManager>.Instance.kills++;
            }

            GetComponentInParent<ActivateNextWave>()?.AddDeadEnemy();
        }


        private void Die()
        {
            if (isDead)
                return;

            //Force cleanup
            InterruptAction();
            ActionEndCallback();

            isDead = true;
            //Components.EnemyIdentifier.dead = true;

            DestroyAllSigils();
            DestroyAllEyes();

            ForceCountDeath();

            OnDeath?.Invoke(this);

            if(disableDeathSequence)
            {
                DeathVFX();
                Remove();
                return;
            }

            DeathSequence();
        }

        public void DestroyAllSigils()
        {
            if (activeSigils.Count > 0)
            {
                foreach (GameObject circleObject in activeSigils)
                {
                    if (circleObject.TryGetComponent<SummonCircle>(out SummonCircle circle))
                        circle.Die();
                    else
                        GameObject.Destroy(circleObject);
                }
            }
        }

        public void DestroyAllEyes()
        {
            for (int i = 0; i < spawnedEyes.Count; i++)
            {
                if (spawnedEyes[i] == null)
                    continue;

                spawnedEyes[i].Explode();
            }
        }

        private void DeathSequence()
        {
            if (IsEnraged)
                SetEnraged(false);

            GameObject bloodSpray = null;
            if(Components.CarcassScreamPrefab != null)
                GameObject.Instantiate(Components.CarcassScreamPrefab, Components.CenterMass);

            bool goreEnabled = MonoSingleton<PrefsManager>.Instance.GetBoolLocal("bloodEnabled", false);

            if (goreEnabled)
                if (Components.BloodSprayFX != null)
                    bloodSpray = GameObject.Instantiate(Components.BloodSprayFX, Components.CenterMass);

            Components.Animation.Death();
            Components.Animation.SetVibrating(true);
            Components.Animation.SetVibrationRange(0.25f);

            TimeController.Instance.SlowDown(0.001f);

            InvokeDelayed(SpawnLightShaft, 0.25f);
            InvokeDelayed(SpawnLightShaft, 1f);
            InvokeDelayed(SpawnLightShaft, 1.75f);
            InvokeDelayed(SpawnLightShaft, 2.5f);
            InvokeDelayed(SpawnLightShaft, 3f);
            InvokeDelayed(SpawnLightShaft, 3.4f);
            InvokeDelayed(SpawnLightShaft, 3.6f);
            InvokeDelayed(SpawnLightShaft, 3.7f);

            if (bloodSpray != null)
            {
                InvokeDelayed(() =>
                {
                    foreach (ParticleSystem ps in bloodSpray.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.Stop();
                    }
                }, 3.7f);
            }

            InvokeDelayed(DeathVFX, 4.6f);

            InvokeDelayed(() =>
            {
                TimeController.Instance.SlowDown(0.001f);
                Remove();
            }, 4.85f);
        }

        private void DeathVFX()
        {
            if (Components.CarcassScreamPrefab != null)
                GameObject.Instantiate(Components.CarcassScreamPrefab, Components.CenterMass.transform).transform.parent = null;

            if (Components.CarcassDeathFX != null)
                GameObject.Instantiate(Components.CarcassDeathFX, Components.CenterMass.transform).transform.parent = null;

            Components.Animation.SetVisible(false);
        }

        public void Remove()
        {
            GameObject.Destroy(gameObject);
        }

        public void SpawnEye()
        {
            Vector3 position = GetRingSpawnPosition();

            GameObject eyePrefab = GetEye();

            if (eyePrefab == null)
                return;

            GoreZone gz = GoreZone.ResolveGoreZone(transform);

            GameObject eyeObject = GameObject.Instantiate(eyePrefab, position, Quaternion.identity);

            if(gz != null)
                eyeObject.transform.SetParent(gz.transform);

            if (eyeObject.TryGetComponent<Drone>(out Drone drone))
            {
                spawnedEyes.Add(drone);
                drone.health = Parameters.eyeHealth;
                SetDroneTarget(drone, target);
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

            //Horrid ¯\_(ツ)_/¯
            
            if(Components.EyeMaterialOverride != null)
            {
                MeshRenderer renderer = eyeObject.GetComponentsInChildren<MeshRenderer>().Where(x => x.name == "Gib_Eyeball").FirstOrDefault();
                renderer.material = Components.EyeMaterialOverride;
            }
            
            if(Components.GenericSpawnFX != null)
                GameObject.Instantiate(Components.GenericSpawnFX, Components.CenterMass);
        }

        private GameObject GetEye()
        {
            if (Components.DroneFlesh == null)
                return UKPrefabs.DroneFlesh.Asset;

            return Components.DroneFlesh;
        }
        
        public void SpawnSigil()
        {
            if (Components.SummonCirclePrefab == null)
                return;

            GameObject newSigil = GameObject.Instantiate(Components.SummonCirclePrefab);
            
            if(newSigil.TryGetComponent<SummonCircle>(out SummonCircle summonCircle))
            {
                summonCircle.SetTarget(target);
                summonCircle.SetOwner(this);
            }

            newSigil.AddComponent<BehaviourRelay>().OnDisabled += () =>
            {
                if (gameObject == null)
                    return;

                if (!activeSigils.Contains(newSigil))
                    return;

                activeSigils.Remove(newSigil);
            };

            activeSigils.Add(newSigil);
            activeSigils = activeSigils.Where(x => x != null).ToList();
        }

        public void FireExplosiveProjectile()
        {
            GameObject prefab = GetLobbedProjectile();

            if (prefab == null)
                return;

            Transform tf = Components.ProjectileLobPoint;

            Vector3 position = tf.position;
            Vector3 direction = tf.forward;

            GameObject projectileObject = GameObject.Instantiate<GameObject>(prefab, position, Quaternion.LookRotation(direction));
            if(projectileObject.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.target = this.target;
                
                if(projectileObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    rb.AddForce(transform.up * 50f, ForceMode.VelocityChange);
                
                projectile.safeEnemyType = EnemyType.Mindflayer; //Hack
                projectile.speed *= Components.EnemyIdentifier.totalSpeedModifier;
                projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;
            }
            
        }

        public Projectile FireTrackingProjectileHalo()
        {
            Vector3 position = GetRingSpawnPosition();
            GameObject prefab = GetHomingProjectile();

            if (prefab == null)
                return null;

            GameObject projectileGameObject = GameObject.Instantiate(prefab, position, Quaternion.LookRotation(position - Components.CenterMass.position));

            if(projectileGameObject.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.target = this.target;
                projectile.speed = 10f * Components.EnemyIdentifier.totalSpeedModifier;
                projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;
                return projectile;
            }

            return null;
        }

        private GameObject GetHomingProjectile()
        {
            if (Components.HomingProjectilePrefab == null)
                return UKPrefabs.HomingProjectile.Asset;

            return Components.HomingProjectilePrefab;
        }

        public Projectile FireTrackingProjectileSpherical()
        {
            GameObject prefab = GetHomingProjectile();

            if (prefab == null)
                return null;

            Transform tf = Components.CenterMass;

            Vector3 position = tf.position;
            Vector3 offset = UnityEngine.Random.onUnitSphere;

            position += offset * Parameters.shakeProjectileOriginRadius;

            GameObject projectileGameObject = GameObject.Instantiate(prefab, position, Quaternion.LookRotation(position - Components.CenterMass.position));

            if (projectileGameObject.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.target = this.target;
                projectile.speed = 10f * Components.EnemyIdentifier.totalSpeedModifier;
                projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;
                return projectile;
            }

            return null;
        }

        private GameObject GetLobbedProjectile()
        {
            if (Components.LobbedExplosiveProjectilePrefab == null)
                return UKPrefabs.LobbedProjectileExplosiveHH.Asset;

            return Components.LobbedExplosiveProjectilePrefab;
        }

        private GameObject GetLightShaft()
        {
            if (Components.LightShaftFX == null)
                return UKPrefabs.LightShaft.Asset;

            return Components.LightShaftFX;
        }

        private void SpawnLightShaft()
        {
            GameObject lightShaft = GetLightShaft();
            if (lightShaft == null)
                return;

            GameObject.Instantiate(lightShaft, Components.CenterMass.position, UnityEngine.Random.rotation).transform.parent = Components.CenterMass;
        }

        public void ResolveTarget()
        {
            if(targetedEnemy != null)
                if(targetedEnemy.dead)
                {
                    target = null;
                    targetedEnemy = null;
                }

            if (target != null)
                return;

            if (!dontAttackPlayer)
            {
                SetTarget(PlayerTracker.Instance.GetTarget());
                return;
            }

            //Cache eids so we dont attack our lil eye guys or ourself.
            HashSet<EnemyIdentifier> friendlies = new HashSet<EnemyIdentifier>(spawnedEyes.Select(x => x.GetComponent<EnemyIdentifier>()));
            friendlies.Add(GetEnemyIdentifier());

            //Gets all enemies that are not it in order of distance.
            foreach(EnemyIdentifier eid in EnemyTracker.Instance.GetCurrentEnemies().OrderBy(x=>(x.transform.position-transform.position).sqrMagnitude))
            {
                //one of our eyes
                if (friendlies.Contains(eid))
                    continue;

                SetTarget(eid.transform);
                break;
            }
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
            Components.Rigidbody.velocity += force;
        }

        public void GetHurt(HurtEventData hurtEventData)
        {
            if (isDead)
                return;

            float damage = CalcDamage(hurtEventData);
            
            if(!string.IsNullOrEmpty(hurtEventData.hitter))
            {
                if (hurtEventData.hitter == "cannonball")
                    Stun();
            }

            float lastHealth = health;

            if (isStunned)
                damage *= Parameters.stunDamageMultiplier;


            //Debug.Log($"Carcass hurt for {damage} damage");
            //Debug.Log(hurtEventData);
            health = Mathf.Max(0f, health - damage);
            Components.Machine.health = health;

            //Enrage at "low health"
            float lowHealth = Parameters.maxHealth * Parameters.lowHealthThreshold;
            if (health < lowHealth && lastHealth >= lowHealth)
            {
                SetEnraged(true);
            }

            OnHurtGore(hurtEventData);
            OnHurtStyle(hurtEventData);

            if (health <= 0f)
                Die();
        }

        private void OnProjectileDetected(Collider col)
        {
            //Dodge directionally
            bool canDodge = CanDodge();
            if (!canDodge)
                return;

            Vector3 projPosition = col.transform.position;
            Vector3 pos = transform.position;
            Vector3 toProjectile = projPosition - pos;

            toProjectile = toProjectile.XZ().normalized;

            //Extremely delightful
            float signedAngle = Vector3.SignedAngle(transform.forward.XZ().normalized, toProjectile, Vector3.up);
            Vector3 dodgeVec = transform.right * -Mathf.Sign(signedAngle);
            Dodge(dodgeVec);
        }

        private void OnEyeDeath(Drone eye)
        {
            if (gameObject == null)
                return;

            if (!spawnedEyes.Contains(eye)) //If its not in the list, we probably healed from it.
                return;

            spawnedEyes.Remove(eye);

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

        private void OnHookStateChanged(bool isHooked)
        {
            this.isHooked = isHooked;
        }

        #endregion

        #region Coroutines

        private Coroutine InvokeDelayed(Action action, float time)
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
                    Projectile proj = FireTrackingProjectileHalo(); 
                    if(proj != null)
                    {
                        proj.transform.parent = spr.transform;
                        proj.spreaded = true;
                    }
                }
                --projectilesRemaining;
            }

            ActionEnd();
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
                Projectile proj = FireTrackingProjectileSpherical();
                if(proj != null)
                {
                    proj.transform.parent = spr.transform;
                    proj.spreaded = true;
                }
                --projectilesRemaining;
            }

            ActionEnd();
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

            ActionEnd();
        }

        private IEnumerator InvokeAfterTimeCoroutine(Action action, float time)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

        private IEnumerator InvokeAfterAnimation(Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            float lastTime = 0f;
            Func<bool> func = () =>
            {
                float currentAnimTime = Components.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if(lastTime < currentAnimTime)
                {
                    lastTime = currentAnimTime;
                }
                return currentAnimTime < 0f || lastTime > currentAnimTime;
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
                    ActionEnd();

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

        #region GoreFix Nonsense

        public float GetLocationCritDamageMultiplier(string location)
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
            return CalcDamage(hurtData.multiplier, GetLocationCritDamageMultiplier(hurtData.target.tag), hurtData.critMultiplier);
        }

        private float CalcDamage(float damageMultiplier, float locationDamage, float critMultiplier)
        {
            return damageMultiplier + locationDamage * damageMultiplier * critMultiplier;
        }

        //Frankensteined from Machine.cs ick!
        private void OnHurtGore(HurtEventData hurtData)
        {
            GameObject gore = null;

            float locationDamage = GetLocationCritDamageMultiplier(hurtData.target.tag);
            float damage = CalcDamage(hurtData.multiplier, locationDamage, hurtData.critMultiplier);

            if (damage <= 0f)
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

            if (health <= 0f)
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

            scalc.HitCalculator(hurtData.hitter, "machine", hurtData.target.tag, isDead, Components.EnemyIdentifier, hurtData.sourceWeapon);
        }

        #endregion

        public float GetHealth()
        {
            return health;
        }

        public EnemyIdentifier GetEnemyIdentifier()
        {
            return Components.EnemyIdentifier;
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

        private void SetDroneTarget(Drone drone, Transform target)
        {
            droneTargetField?.SetValue(drone, target);
            if(drone.TryGetComponent<DroneFlesh>(out DroneFlesh droneFlesh))
            {
                droneFleshTargetField?.SetValue(droneFlesh, target);
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
            this.targetedEnemy = null;
            
            if(target != null)
                if(target.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
                {
                    this.targetedEnemy = eid;
                }

            //Set the target for our eyes.
            foreach (Drone d in spawnedEyes)
            {
                SetDroneTarget(d, target);
            }
        }

        public void SetShouldAttackPlayer(bool attackPlayer)
        {
            this.dontAttackPlayer = !attackPlayer;
        }

        private bool TargetLineOfSightCheck()
        {
            if (target == null || isBlind)
                return false;

            Vector3 pos = Components.CenterMass.position;
            Vector3 targetPos = target.position;
            Vector3 toTarget = targetPos - pos;

            return !Physics.Raycast(pos, toTarget, toTarget.magnitude, LayerMaskDefaults.Get(LMD.Environment), QueryTriggerInteraction.Ignore);
        }

        public void SetParams(CarcassParameters parameters)
        {
            this.parameters = parameters;
        }

        public CarcassComponents Components
        {
            get
            {
                if (components == null)
                    components = GetComponent<CarcassComponents>();
                return components;
            }
        }

        public CarcassParameters Parameters
        {
            get
            {
                if (parameters == null)
                {
                    if (serializedParameters != null)
                        parameters = serializedParameters.Parameters;
                    else
                        parameters = new CarcassParameters();
                }
                return parameters;
            }
        }

        private void OnDestroy()
        {
            DestroyAllEyes();
            DestroyAllSigils();
        }
    }
}
