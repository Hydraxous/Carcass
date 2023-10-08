using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace CarcassEnemy.Patches
{
    //Because EnemyIdentifier component is... *uniquely engineered*, here we force a Machine component to become a relay for interface behavior.
    [HarmonyPatch(typeof(Machine))]
    public static class MachineLinker
    {
        private static HashSet<Machine> machineRelays = new HashSet<Machine>();
        private static Dictionary<Machine, ICustomEnemy> links = new Dictionary<Machine, ICustomEnemy>();

        public static void ClearCache()
        {
            machineRelays.Clear();
            links.Clear();
            eidLinks.Clear();
        }

        //We check if our interface is on the same object as Machine component.
        //Since Machine is the simplest enemy component, we just use it as a relay.
        //we can cache Machines's signature and then prevent it from actually running any code.
        [HarmonyPatch("Start"), HarmonyPrefix]
        private static bool Start(Machine __instance)
        {
            if (!__instance.TryGetComponent<ICustomEnemy>(out ICustomEnemy enemy))
                return true;

            if (!machineRelays.Contains(__instance))
            {
                machineRelays.Add(__instance);
                links.Add(__instance, enemy);
                if(__instance.TryGetComponent<EnemyIdentifier>(out EnemyIdentifier eid))
                    eidLinks.Add(__instance, eid);
                //__instance.enabled = false; Dunno what effect this will cause, it works fine without doing this so...
                return false;
            }

            return true;
        }

        private static bool IsCustom(Machine machine)
        {
            return machineRelays.Contains(machine);
        }

        [HarmonyPatch("Update"), HarmonyPrefix]
        private static bool Update(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("FixedUpdate"), HarmonyPrefix]
        private static bool FixedUpdate(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        private static Dictionary<Machine, EnemyIdentifier> eidLinks = new Dictionary<Machine, EnemyIdentifier>();

        [HarmonyPatch(nameof(Machine.GetHurt)), HarmonyPrefix]
        private static bool GetHurt(Machine __instance, GameObject target, Vector3 force, float multiplier, float critMultiplier, GameObject sourceWeapon = null)
        {
            if (!IsCustom(__instance))
                return true;

            string hitter = "";
            if (eidLinks.ContainsKey(__instance))
                hitter = eidLinks[__instance].hitter;

            //Pass logic to linked ICustomEnemy, and clean up the parameters
            links[__instance]?.GetHurt(new HurtEventData()
            {
                target = target,
                force = force,
                multiplier = multiplier,
                critMultiplier = critMultiplier,
                sourceWeapon = sourceWeapon,
                hitter = hitter
            });

            return false;
        }

        [HarmonyPatch(nameof(Machine.GoLimp)), HarmonyPrefix]
        private static bool GoLimp(Machine __instance)
        {
            if (!IsCustom(__instance))
                return true;

            //Pass logic to linked ICustomEnemy
            links[__instance]?.Instakill();

            return false;
        }

        [HarmonyPatch(nameof(Machine.KnockBack)), HarmonyPrefix]
        private static bool KnockBack(Machine __instance, Vector3 force)
        {
            if (!IsCustom(__instance))
                return true;

            //Pass logic to linked ICustomEnemy
            links[__instance]?.Knockback(force);

            return false;
        }

        [HarmonyPatch(nameof(Machine.CanisterExplosion)), HarmonyPrefix]
        private static bool CanisterExplosion(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("ReadyGib"), HarmonyPrefix]
        private static bool ReadyGib(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StartHealing"), HarmonyPrefix]
        private static bool StartHealing(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StopHealing"), HarmonyPrefix]
        private static bool StopHealing(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("StopKnockBack"), HarmonyPrefix]
        private static bool StopKnockBack(Machine __instance)
        {
            return !IsCustom(__instance);
        }

        [HarmonyPatch("OnEnable"), HarmonyPrefix]
        private static bool OnEnable(Machine __instance)
        {
            return !IsCustom(__instance);
        }
    }
}
