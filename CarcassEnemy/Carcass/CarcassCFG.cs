using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Configgy;
using Newtonsoft.Json;
using UnityEngine;

namespace CarcassEnemy
{
    public static class CarcassCFG
    {
        private static CarcassParameters _parameters;

        [Configgy.Configgable("Carcass/Parameters", orderInList:-10)]
        private static UIMeme uiMeme = new UIMeme();

        //Health
        [Configgy.Configgable("Carcass/Parameters")] private static float maxHealth = 80f;
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
        [Configgy.Configgable("Carcass/Parameters")]  private static float spinMaxRange = 35f;
        [Configgy.Configgable("Carcass/Parameters")] private static int spinMeleeDamage = 20;
        [Configgy.Configgable("Carcass/Parameters")] private static float spinMeleeKnockback = 15f;

        //Blue projectile attack
        [Configgy.Configgable("Carcass/Parameters")]  private static float shakeProjectileOriginRadius = 4f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float shakeProjectileBurstLengthInSeconds = 2.2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static int shakeProjectileCount = 3;
        [Configgy.Configgable("Carcass/Parameters")]  private static int shakeProjectileGroup = 3;


        //Eye spawning
        [Configgy.Configgable("Carcass/Parameters")]  private static int eyeSpawnCount = 3;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeSpawnDelay = 0.14f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeHealDelay = 0.3f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeInitialHealDelay = 0.4f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeHealPerEye = 6.666f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeHealth = 2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float eyeSpawnCooldown = 12f;

        //Stun
        [Configgy.Configgable("Carcass/Parameters")]  private static float stunTime = 2.5f; //Unused, currently animation length controls stun time.
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageAddEnrageTimeOnStun = 8.5f;
        [Configgy.Configgable("Carcass/Parameters")] private static float stunDamageMultiplier = 1.2f;

        [Configgy.Configgable("Carcass/Parameters")] private static float dodgeForce = 65f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeCooldownTime = 2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeLength = 1.1f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeBrakeSpeed = 0.45f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeStaminaCost = 2f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float dodgeActionTimerAddition = 1.4f;
        [Configgy.Configgable("Carcass/Parameters")] private static float dodgeMinRange = 19f;


        //Enrage
        [Configgy.Configgable("Carcass/Parameters")]  private static float enrageLength = 16f;
        [Configgy.Configgable("Carcass/Parameters")]  private static float enrageAttackTimerMultiplier = 1.5f;
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageDashForce = 100f;
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageDashLength = 0.8f;
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageDashBrakeForce = 0.04f;
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageDashMaxRange = 40f;
        [Configgy.Configgable("Carcass/Parameters")] private static float enrageDashActionDelay = 0.7f;
        [Configgy.Configgable("Carcass/Parameters")] private static int enrageBlueProjectileCount = 4;

        [Configgy.Configgable("Carcass/Parameters")] private static float hookBiteDelay = 1f;
        [Configgy.Configgable("Carcass/Parameters")] private static float hookCooldown = 6f;
        [Configgy.Configgable("Carcass/Parameters")] private static float hookPlayerCooldown = 2.2f;
        [Configgy.Configgable("Carcass/Parameters")] private static int hookBiteYellowHP = 20;

        [Configgy.Configgable("Carcass/Parameters")] private static int barrageProjectileCount = 24;
        [Configgy.Configgable("Carcass/Parameters")] private static float barrageAttackLength = 4f;
        [Configgy.Configgable("Carcass/Parameters")] private static float barrageAttackProjectileDelay = 0.25f;

        //CarpetBomb
        [Configgy.Configgable("Carcass/Parameters")] private static float carpetBombLength = 3f;
        [Configgy.Configgable("Carcass/Parameters")] private static float carpetBombProjectileDelay = 0.4f;

        [Configgy.Configgable("Carcass/Parameters", description:"Enables Barrage and Carpet Bomb attacks while enraged.")] private static bool enableEnrageWildAttacks;


        public static CarcassParameters GetParameters()
        {
            if(_parameters == null)
            {
                _parameters = BuildParams();
            }

            return _parameters;
        }

        private const string fileName = "carcass.cfg";

        [Configgy.Configgable("Carcass/Parameters", orderInList:-1, displayName:"Export Parameters To File")]
        private static void ExportParameters()
        {
            CarcassParameters cParams = BuildParams();

            string folder = HydraDynamics.DataPersistence.DataManager.GetDataPath();

            string filePath = Path.Combine(folder, fileName);

            string json = JsonConvert.SerializeObject(cParams, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Log("Exported Parameters.");

            Application.OpenURL("file://" + folder);
        }

        [Configgy.Configgable("Carcass/Parameters", orderInList: 0, displayName: "Import Parameters File")]
        private static void ImportParameters()
        {
            string folder = HydraDynamics.DataPersistence.DataManager.GetDataPath();
            string filePath = Path.Combine(folder, fileName);

            if(!File.Exists(filePath)) 
            {
                Application.OpenURL("file://" + folder);
                return;
            }

            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                CarcassParameters p = JsonConvert.DeserializeObject<CarcassParameters>(json);
                if (p != null)
                {
                    _parameters = p;
                    ApplyParamsInternal();
                }

                Debug.Log("Imported carcass file.");
            }
        }


        [Configgy.Configgable("Carcass", "Apply Parameters")]
        public static void ApplyParameters()
        {
            _parameters = BuildParams();
            ApplyParamsInternal();
        }

        private static void ApplyParamsInternal()
        {
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
                spinMaxRange = spinMaxRange,
                spinMeleeDamage = spinMeleeDamage,
                spinMeleeKnockback = spinMeleeKnockback,

                //Blue projectile attack
                shakeProjectileOriginRadius = shakeProjectileOriginRadius,
                shakeProjectileBurstLengthInSeconds = shakeProjectileBurstLengthInSeconds,
                shakeProjectileCount = shakeProjectileCount,
                shakeProjectileGroup = shakeProjectileGroup,


                //Eye spawning
                eyeSpawnCount = eyeSpawnCount,
                eyeSpawnDelay = eyeSpawnDelay,

                eyeHealDelay = eyeHealDelay,
                eyeInitialHealDelay = eyeInitialHealDelay,
                eyeHealPerEye = eyeHealPerEye,
                eyeHealth = eyeHealth,
                eyeSpawnCooldown = eyeSpawnCooldown,
                //Stun
                stunTime = stunTime, //Unused, currently animation length controls stun time.
                stunDamageMultiplier = stunDamageMultiplier,
                enrageAddEnrageTimeOnStun = enrageAddEnrageTimeOnStun,

                dodgeCooldownTime = dodgeCooldownTime,
                dodgeLength = dodgeLength,
                dodgeBrakeSpeed = dodgeBrakeSpeed,
                dodgeStaminaCost = dodgeStaminaCost,
                dodgeActionTimerAddition = dodgeActionTimerAddition,
                dodgeMinRange = dodgeMinRange,

                //Enrage
                enrageLength = enrageLength,
                enrageAttackTimerMultiplier = enrageAttackTimerMultiplier,
                enrageDashBrakeForce = enrageDashBrakeForce,
                enrageDashForce = enrageDashForce,
                enrageDashLength = enrageDashLength,
                enrageDashMaxRange = enrageDashMaxRange,
                enrageDashActionDelay= enrageDashActionDelay,
                enrageBlueProjectileCount = enrageBlueProjectileCount,

                hookBiteDelay = hookBiteDelay,
                hookCooldown= hookCooldown,
                hookPlayerCooldown= hookPlayerCooldown,
                hookBiteYellowHP= hookBiteYellowHP,

                barrageAttackLength = barrageAttackLength,
                barrageAttackProjectileDelay = barrageAttackProjectileDelay,
                barrageProjectileCount= barrageProjectileCount,

                carpetBombLength=carpetBombLength,
                carpetBombProjectileDelay=carpetBombProjectileDelay,

                enableEnrageWildAttacks = enableEnrageWildAttacks,
            };
        }
    }
}
