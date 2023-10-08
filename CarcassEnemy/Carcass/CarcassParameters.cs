using System;

namespace CarcassEnemy
{
    public class CarcassParameters
    {
        //Health
        public float maxHealth = 65f;
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
        public float dodgeForce = 10f;

        //Targeting
        public float minTargetDistance = 16f;
        public float maxTargetDistance = 18f;
        
        //General Attack
        public float attackDelay = 3f;
        public float speedWhileAttackingMultiplier = 0.25f;
        public float enragedSpeedMultiplier = 1.5f;
        public float maxAttackTimeFailsafe = 8f;

        //Lob projectile attack
        public float spin_MaxRange = 35f;
        public int spin_MeleeDamage = 20;
        public float spin_MeleeKnockback = 15f;

        //Blue projectile attack
        public float shake_ProjectileOriginRadius = 4f;
        public float shake_ProjectileBurstLengthInSeconds = 2.2f;
        public int shake_ProjectileCount = 3;
        public int shake_ProjectileGroup = 3;


        //Eye spawning
        public int eye_SpawnCount = 3;
        public float eye_SpawnDelay = 0.14f;
        public float eye_HealDelay = 0.3f;
        public float eye_InitialHealDelay = 0.33f;
        public float eye_HealPerEye = 6.666f;
        public float eye_Health = 2f;
        public float eye_SpawnCooldown = 12f;

        //Stun
        public float stunTime = 2.5f; //Unused, currently animation length controls stun time.
        
        public float dodgeCooldownTime = 6f;
        public float dodgeLength = 0.65f;
        public float dodgeBrakeSpeed = 2f;
        public float dodgeStaminaCost = 2f;
        public float dodgeActionTimerAddition = 0.75f;

        //Enrage
        public float enrageLength = 16f;
        public float enrageAttackTimerMultiplier = 1.5f;

        public float maxStamina = 5f;
        public float staminaRechargeRate = 0.25f;
    }
}
