using System;

namespace CarcassEnemy
{
    public class CarcassParameters
    {
        //Health
        public float maxHealth = 80f;
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

        //Targeting
        public float minTargetDistance = 16f;
        public float maxTargetDistance = 18f;

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
        public int hookBiteYellowHP = 20;

        public int barrageProjectileCount = 24;
        public float barrageAttackLength = 4f;
        public float barrageAttackProjectileDelay = 0.25f;

        //CarpetBomb
        public float carpetBombLength = 3f;
        public float carpetBombProjectileDelay = 0.4f;

        public bool enableEnrageWildAttacks;
    }
}
