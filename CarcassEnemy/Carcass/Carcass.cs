using CarcassEnemy.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        //internal state
        private float health;
        public float Health => health;
        public bool IsAlive => health > 0;
        public bool Enraged { get; private set; } = false;

        private bool isStunned;

        private float randomStrafeDirection;

        private float timeUntilDirectionChange = 2f;

        private float attackTimer = 3f;


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

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private void TimerUpdate()
        {
            timeUntilDirectionChange -= Time.deltaTime;
            if (timeUntilDirectionChange <= 0f)
            {
                ChangeStrafeDirection();
                timeUntilDirectionChange = Parameters.directionChangeDelay;
            }

            if (!isAttacking && !DisableAttackTimer)
                attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f && !isStunned)
            {
                //DoAttack
                RandomAttack();
                attackTimer = Parameters.attackDelay;
            }
        }

        public static bool DisableAttackTimer;

        private void RandomAttack()
        {
            Delegate[] attacks = new Delegate[] { SpinAttack, ShakeAttack, () => { isAttacking = false; } };
            Delegate d = attacks[UnityEngine.Random.Range(0, attacks.Length)];
            d.DynamicInvoke();
        }

        private void PerformAction()
        {
            //If can spawn eyes. Spawn eyes.



        }

        private void ChangeStrafeDirection()
        {
            randomStrafeDirection = Mathf.Sign((UnityEngine.Random.value - 0.5f) * 2f);
        }

        //Meh
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
            
            if(isStunned || dead)
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


            if (IsAttacking)
                latSpeed *= Parameters.speedWhileAttackingMultiplier;

            Vector3 moveDelta = travelVector * latSpeed;

            float vertSpeed = Parameters.verticalFlySpeed;

            if (IsAttacking)
                vertSpeed *= Parameters.speedWhileAttackingMultiplier;

            moveDelta.y += CalculateVerticalMoveDirection(transform.position, Parameters.desiredHeight) * vertSpeed;
            velocity = Vector3.MoveTowards(velocity, moveDelta, Time.deltaTime * Parameters.movementSmoothing);
            return velocity;
        }

        private Vector3 ApplyBrake(Vector3 velocity)
        {
            return Vector3.MoveTowards(velocity, Vector3.zero, Time.deltaTime * Parameters.movementSmoothing);
        }

        //Shows state on screen
        private void OnGUI()
        {
            GUI.skin.box.fontSize = 35;
            GUI.skin.box.normal.textColor = Color.white;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;

            string stateName = $"AT: ({attackTimer}) ATK:{isAttacking}";
            stateName += $"\nHP:{health}";
            stateName += $"\nST: {timeUntilDirectionChange}";
            GUILayout.Box(stateName.TrimEnd('\n', '\r'));
        }

        public void Instakill()
        {
            if (dead)
                return;

            dead = true;
            StopAllCoroutines();
            StartCoroutine(DeathCoroutine());
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

            Debug.Log(hurtDbg);

            //Impl: Crit damage
            health = Mathf.Max(0f, health - hurtEventData.multiplier);

            if (health <= 0f && !dead)
            {
                dead = true;
                StartCoroutine(DeathCoroutine());
            }
        }

        public void SpinAttack()
        {
            isAttacking = true;
            Components.Animation.Spin();
            StartCoroutine(AwaitAttackAnimationEnd(AttackDone));
        }

        public void Stun()
        {
            isStunned = true;
            isAttacking = false;
            Components.Animation.Stunned();
            StopAllCoroutines();
            InvokeAfterTime(StopStun, Parameters.stunTime);
        }

        private void StopStun()
        {
            isStunned = false;
        }

        public void ShakeAttack()
        {
            isAttacking = true;
            Components.Animation.Shake();
            StartCoroutine(ShakeAttackCoroutine());
        }

        private GameObject GetEyePrefab()
        {
            return null;
        }

        public void SpawnEyes()
        {
            isAttacking = true;
            Components.Animation.Shake();

            for(int i = 0; i < Parameters.eyeSpawnCount; i++)
            {
                float delay = Parameters.eyeSpawnDelay * (i+1);
                InvokeAfterTime(SpawnSingleEye, delay);
            }

        }

        private void SpawnSingleEye()
        {

        }

        public void HealAction()
        {

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

            isAttacking = false;
        }

        private List<GameObject> spawnedEyes = new List<GameObject>();

        private void InvokeAfterTime(Action action, float time, Func<bool> condition = null)
        {
            StartCoroutine(InvokeAfterTimeCoroutine(action, time, condition));
        }

        private IEnumerator InvokeAfterTimeCoroutine(Action action, float time, Func<bool> condition = null)
        {
            float timer = time;
            if(condition == null)
            {
                condition = () => { return true; };
            }

            while (timer > 0f)
            {
                yield return new WaitForEndOfFrame();
                timer -= Time.deltaTime;
            }

            if (condition.Invoke())
            {
                action?.Invoke();
            }
        }

        private IEnumerator AwaitAttackAnimationEnd(Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            Func<bool> func = () =>
            {
                return Components.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
            };
            yield return new WaitWhile(func);

            onComplete?.Invoke();
        }

        private bool isAttacking;
        public bool IsAttacking => isAttacking;
        public void AttackDone()
        {
            isAttacking = false;
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
            GameObject projectileGameObject = GameObject.Instantiate(UKPrefabs.HomingProjectile.Asset, position, Quaternion.LookRotation(position-Components.CenterMass.position));
            Projectile projectile = projectileGameObject.GetComponent<Projectile>();
            projectile.target = this.target;
            projectile.speed = 10f * Components.EnemyIdentifier.totalSpeedModifier;
            projectile.damage *= Components.EnemyIdentifier.totalDamageModifier;

            return projectile;
        }

        private Vector3 GetRingSpawnPosition()
        {
            Vector3 position = Components.CenterMass.position;
            Vector2 disc = UnityEngine.Random.onUnitSphere;
            disc.Normalize();

            Vector3 targetPos = position+Vector3.up;
            if (target != null)
            {
                targetPos = target.position;
            }

            Quaternion targetAlignedRotation = Quaternion.LookRotation(targetPos - position);
            Vector3 offset = targetAlignedRotation * new Vector3(disc.x, disc.y, 0f);

            position += offset * Parameters.shake_ProjectileOriginRadius;
            return position;
        }

        private bool dead;

        //Placeholder stuff
        private IEnumerator DeathCoroutine()
        {
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;

            float time = 0.35f;
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


        public Vector3 ExternalForce { get; private set; }

        public void Knockback(Vector3 force)
        {
            ExternalForce += force;
        }
    }
}
