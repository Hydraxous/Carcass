using System;
using System.Collections.Generic;
using System.Text;
using Configgy;
using UnityEngine;

namespace CarcassEnemy
{
    public static class CarcassCFG
    {
        private static CarcassParameters _parameters;

        [Configgy.Configgable("Carcass/Parameters", orderInList:-10)]
        private static UIMeme uiMeme = new UIMeme();

        //Health
        [Configgy.Configgable("Carcass/Parameters")] private static float maxHealth = 65f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float lowHealthThreshold = 0.4f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float healCooldown = 9f;

        //Movement
        [Configgy.Configgable("Carcass/Parameters")]  private static float lateralFlySpeed = 30f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float verticalFlySpeed = 10f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float fallSpeed = 20f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float strafeSpeed = 15f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float desiredFlyHeight = 7f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float movementSmoothing = 8f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float directionChangeDelay = 2f;

        //Targeting
        [Configgy.Configgable("Carcass/Parameters")]  private static float minTargetDistance = 16f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float maxTargetDistance = 18f;

        //General Attack
        [Configgy.Configgable("Carcass/Parameters")]  private static float attackDelay = 3f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float speedWhileAttackingMultiplier = 0.25f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float enragedSpeedMultiplier = 1.5f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float maxAttackTimeFailsafe = 8f;

        //Lob projectile attack
        [Configgy.Configgable("Carcass/Parameters")]  private static float spin_MaxRange = 35f;
        [Configgy.Configgable("Carcass/Parameters")] private static int spin_MeleeDamage = 20;
        [Configgy.Configgable("Carcass/Parameters")] private static float spin_MeleeKnockback = 15f;

        //Blue projectile attack
        [Configgy.Configgable("Carcass/Parameters")]  private static float shake_ProjectileOriginRadius = 4f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float shake_ProjectileBurstLengthInSeconds = 2.2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static int shake_ProjectileCount = 3;
        [Configgy.Configgable("Carcass/Parameters")]  private static int shake_ProjectileGroup = 3;


        //Eye spawning
        [Configgy.Configgable("Carcass/Parameters")]  private static int eye_SpawnCount = 3;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_SpawnDelay = 0.14f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_HealDelay = 0.3f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_InitialHealDelay = 0.4f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_HealPerEye = 6.666f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_Health = 2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eye_SpawnCooldown = 12f;

        //Stun
        [Configgy.Configgable("Carcass/Parameters")]  private static float stunTime = 2.5f; //Unused, currently animation length controls stun time.

        [Configgy.Configgable("Carcass/Parameters")] private static float dodgeForce = 65f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeCooldownTime = 6f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeLength = 1.1f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeBrakeSpeed = 0.45f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeStaminaCost = 2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeActionTimerAddition = 1.4f;

        //Enrage
        [Configgy.Configgable("Carcass/Parameters")]  private static float enrageLength = 16f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float enrageAttackTimerMultiplier = 1.5f;

        [Configgy.Configgable("Carcass/Parameters")]  private static float maxStamina = 5f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float staminaRechargeRate = 0.7f;

        public static CarcassParameters GetParameters()
        {
            if(_parameters == null)
            {
                _parameters = BuildParams();
            }

            return _parameters;
        }

        [Configgy.Configgable("Carcass", "Apply Parameters")]
        public static void ApplyParameters()
        {
            _parameters = BuildParams();
            foreach (Carcass c in GameObject.FindObjectsOfType<Carcass>())
            {
                c.SetParams(_parameters);
            }
        }

        [Configgy.Configgable("Carcass", "Toggle Hitbox Visibility")]
        private static void ToggleHitboxVisibility()
        {
            HitboxesVisible = !HitboxesVisible;
            foreach (Carcass c in GameObject.FindObjectsOfType<Carcass>())
            {
                c.SetHitboxVisibility(HitboxesVisible);
            }
        }

        public static bool HitboxesVisible { get; private set; }

        private static CarcassParameters BuildParams()
        {
            return new CarcassParameters()
            {
                maxHealth = maxHealth,
                lowHealthThreshold = lowHealthThreshold,
                healCooldown = healCooldown,

                //Movement
                lateralFlySpeed = lateralFlySpeed,
                verticalFlySpeed = verticalFlySpeed,
                fallSpeed = fallSpeed,
                strafeSpeed = strafeSpeed,
                desiredFlyHeight = desiredFlyHeight,
                movementSmoothing = movementSmoothing,
                directionChangeDelay = directionChangeDelay,
                dodgeForce = dodgeForce,

                //Targeting
                minTargetDistance = minTargetDistance,
                maxTargetDistance = maxTargetDistance,

                //General Attack
                attackDelay = attackDelay,
                speedWhileAttackingMultiplier = speedWhileAttackingMultiplier,
                enragedSpeedMultiplier = enragedSpeedMultiplier,
                maxAttackTimeFailsafe = maxAttackTimeFailsafe,

                //Lob projectile attack
                spin_MaxRange = spin_MaxRange,
                spin_MeleeDamage = spin_MeleeDamage,
                spin_MeleeKnockback= spin_MeleeKnockback,

                //Blue projectile attack
                shake_ProjectileOriginRadius = shake_ProjectileOriginRadius,
                shake_ProjectileBurstLengthInSeconds = shake_ProjectileBurstLengthInSeconds,
                shake_ProjectileCount = shake_ProjectileCount,
                shake_ProjectileGroup = shake_ProjectileGroup,


                //Eye spawning
                eye_SpawnCount = eye_SpawnCount,
                eye_SpawnDelay = eye_SpawnDelay,

                eye_HealDelay = eye_HealDelay,
                eye_InitialHealDelay = eye_InitialHealDelay,
                eye_HealPerEye = eye_HealPerEye,
                eye_Health = eye_Health,
                eye_SpawnCooldown = eye_SpawnCooldown,
                //Stun
                stunTime = stunTime, //Unused, currently animation length controls stun time.

                dodgeCooldownTime = dodgeCooldownTime,
                dodgeLength = dodgeLength,
                dodgeBrakeSpeed = dodgeBrakeSpeed,
                dodgeStaminaCost = dodgeStaminaCost,
                dodgeActionTimerAddition = dodgeActionTimerAddition,

                //Enrage
                enrageLength = enrageLength,
                enrageAttackTimerMultiplier = enrageAttackTimerMultiplier,

                maxStamina = maxStamina,
                staminaRechargeRate = staminaRechargeRate
            };
        }
    }
}
