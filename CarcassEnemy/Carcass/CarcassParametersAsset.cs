using UnityEngine;

namespace CarcassEnemy
{
    //Unity Editor QoL thing
    public class CarcassParametersAsset : ScriptableObject
    {
        //Match fields from CarcassParameters
        public float lowHealthThreshold = 0.4f;
        public float healCooldown = 9f;

        //Movement
        public float lateralFlySpeed = 30f;
        public float verticalFlySpeed = 10f;
        public float fallSpeed = 20f;
        public float strafeSpeed = 15f;
        public float desiredFlyHeight = 7f;
        public float movementSmoothing = 8f;
        public float directionChangeDelay = 2f;
        public float strafeObstacleCheckDistance = 8f;

        //Targeting
        public float minTargetDistance = 16f;
        public float maxTargetDistance = 18f;
        public float targetCheckDelay = 1f;

        //General Attack
        public float attackDelay = 3f;
        public float speedWhileAttackingMultiplier = 0.25f;
        public float enragedSpeedMultiplier = 1.5f;
        public float maxAttackTimeFailsafe = 8f;

        //Lob projectile attack
        public float spinMaxRange = 35f;
        public int spinMeleeDamage = 20;
        public float spinMeleeKnockback = 15f;

        //Blue projectile attack
        public float shakeProjectileOriginRadius = 4f;
        public float shakeProjectileBurstLengthInSeconds = 2.2f;
        public int shakeProjectileCount = 3;
        public int shakeProjectileGroup = 3;

        //Eye spawning
        public int eyeSpawnCount = 3;
        public float eyeSpawnDelay = 0.14f;
        public float eyeHealDelay = 0.3f;
        public float eyeInitialHealDelay = 0.4f;
        public float eyeHealPerEye = 6.666f;
        public float eyeHealth = 2f;
        public float eyeSpawnCooldown = 12f;

        //Stun
        public float stunTime = 2.5f; //Unused, currently animation length controls stun time.
        public float enrageAddEnrageTimeOnStun = 8.5f;
        public float stunDamageMultiplier = 1.2f;

        public float dodgeForce = 65f;
        public float dodgeCooldownTime = 2f;
        public float dodgeLength = 1.1f;
        public float dodgeBrakeSpeed = 0.45f;
        public float dodgeStaminaCost = 2f;
        public float dodgeActionTimerAddition = 1.4f;
        public float dodgeMinRange = 19f;


        //Enrage
        public float enrageLength = 16f;
        public float enrageAttackTimerMultiplier = 1.5f;
        public float enrageDashForce = 100f;
        public float enrageDashLength = 0.8f;
        public float enrageDashBrakeForce = 0.04f;
        public float enrageDashMaxRange = 40f;
        public float enrageDashActionDelay = 0.7f;
        public int enrageBlueProjectileCount = 4;
        public float enrageHookBiteSpeedMultiplier = 2f;

        public float hookBiteDelay = 1f;
        public float hookCooldown = 6f;
        public float hookPlayerCooldown = 2.2f;
        public int hookBiteYellowHP = 0;

        public int barrageProjectileCount = 24;
        public float barrageAttackLength = 4f;
        public float barrageAttackProjectileDelay = 0.25f;

        //CarpetBomb
        public float carpetBombLength = 3f;
        public float carpetBombProjectileDelay = 0.4f;

        public bool enableEnrageWildAttacks;

        private CarcassParameters _parameters;
        public CarcassParameters Parameters 
        {
            get
            {
                if(_parameters == null)
                {
                    _parameters = Convert();
                }
                return _parameters;
            }
        }

        private CarcassParameters Convert()
        {
            CarcassParameters parameters = new CarcassParameters();

            parameters.lowHealthThreshold = lowHealthThreshold;
            parameters.healCooldown = healCooldown;

            //Movement
            parameters.lateralFlySpeed = lateralFlySpeed;
            parameters.verticalFlySpeed = verticalFlySpeed;
            parameters.fallSpeed = fallSpeed;
            parameters.strafeSpeed = strafeSpeed;
            parameters.desiredFlyHeight = desiredFlyHeight;
            parameters.movementSmoothing = movementSmoothing;
            parameters.directionChangeDelay = directionChangeDelay;
            parameters.strafeObstacleCheckDistance = strafeObstacleCheckDistance;

            //Targeting
            parameters.minTargetDistance = minTargetDistance;
            parameters.maxTargetDistance = maxTargetDistance;
            parameters.targetCheckDelay = targetCheckDelay;

            //General Attack
            parameters.attackDelay = attackDelay;
            parameters.speedWhileAttackingMultiplier = speedWhileAttackingMultiplier;
            parameters.enragedSpeedMultiplier = enragedSpeedMultiplier;
            parameters.maxAttackTimeFailsafe = maxAttackTimeFailsafe;

            //Lob projectile attack
            parameters.spinMaxRange = spinMaxRange;
            parameters.spinMeleeDamage = spinMeleeDamage;
            parameters.spinMeleeKnockback = spinMeleeKnockback;

            //Blue projectile attack
            parameters.shakeProjectileOriginRadius = shakeProjectileOriginRadius;
            parameters.shakeProjectileBurstLengthInSeconds = shakeProjectileBurstLengthInSeconds;
            parameters.shakeProjectileCount = shakeProjectileCount;
            parameters.shakeProjectileGroup = shakeProjectileGroup;

            //Eye spawning
            parameters.eyeSpawnCount = eyeSpawnCount;
            parameters.eyeSpawnDelay = eyeSpawnDelay;
            parameters.eyeHealDelay = eyeHealDelay;
            parameters.eyeInitialHealDelay = eyeInitialHealDelay;
            parameters.eyeHealPerEye = eyeHealPerEye;
            parameters.eyeHealth = eyeHealth;
            parameters.eyeSpawnCooldown = eyeSpawnCooldown;

            //Stun
            parameters.stunTime = stunTime; //Unused, currently animation length controls stun time.
            parameters.enrageAddEnrageTimeOnStun = enrageAddEnrageTimeOnStun;
            parameters.stunDamageMultiplier = stunDamageMultiplier;

            parameters.dodgeForce = dodgeForce;
            parameters.dodgeCooldownTime = dodgeCooldownTime;
            parameters.dodgeLength = dodgeLength;
            parameters.dodgeBrakeSpeed = dodgeBrakeSpeed;

            //Enrage
            parameters.enrageLength = enrageLength;
            parameters.enrageAttackTimerMultiplier = enrageAttackTimerMultiplier;
            parameters.enrageDashForce = enrageDashForce;
            parameters.enrageDashLength = enrageDashLength;
            parameters.enrageDashBrakeForce = enrageDashBrakeForce;
            parameters.enrageDashMaxRange = enrageDashMaxRange;
            parameters.enrageDashActionDelay = enrageDashActionDelay;
            parameters.enrageBlueProjectileCount = enrageBlueProjectileCount;
            parameters.enrageHookBiteSpeedMultiplier = enrageHookBiteSpeedMultiplier;

            //Hookbite
            parameters.hookBiteDelay = hookBiteDelay;
            parameters.hookCooldown = hookCooldown;
            parameters.hookBiteYellowHP = hookBiteYellowHP;

            //Barage
            parameters.barrageProjectileCount = barrageProjectileCount;
            parameters.barrageAttackLength = barrageAttackLength;
            parameters.barrageAttackProjectileDelay = barrageAttackProjectileDelay;

            //CarpetBomb
            parameters.carpetBombLength = carpetBombLength;
            parameters.carpetBombProjectileDelay = carpetBombProjectileDelay;

            parameters.enableEnrageWildAttacks = enableEnrageWildAttacks;

            return parameters;
        }

    }
}
