using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Patches
{
    [HarmonyPatch(typeof(Drone))]
    public static class FixDroneNFE
    {

        [HarmonyPatch("Update"), HarmonyPrefix]
        public static void OnUpdate(Drone __instance, ref Transform ___target)
        {
            if(___target == null)
            {
                if(!__instance.friendly)
                {
                    ___target = PlayerTracker.Instance.GetTarget();
                }
            }
        }

        [HarmonyPatch("FixedUpdate"), HarmonyPrefix]
        public static void OnFixedUpdate(Drone __instance, ref Transform ___target)
        {
            if (___target == null)
            {
                if (!__instance.friendly)
                {
                    ___target = PlayerTracker.Instance.GetTarget();
                }
            }
        }

        [HarmonyPatch("SlowUpdate"), HarmonyPrefix]
        public static void OnSlowUpdate(Drone __instance, ref Transform ___target)
        {
            if (___target == null)
            {
                if (!__instance.friendly)
                {
                    ___target = PlayerTracker.Instance.GetTarget();
                }
            }
        }
    }

    [HarmonyPatch(typeof(DroneFlesh))]
    public static class FixDroneFleshNFE
    {

        [HarmonyPatch("Update"), HarmonyPrefix]
        public static void OnUpdate(DroneFlesh __instance, Drone ___drn, ref Transform ___target)
        {
            if (___target == null)
            {
                if(___drn == null || !___drn.friendly)
                {
                    ___target = PlayerTracker.Instance.GetTarget();
                }
            }
        }

        [HarmonyPatch("PrepareBeam"), HarmonyPrefix]
        public static void OnPrepareBeam(DroneFlesh __instance, Drone ___drn, ref Transform ___target)
        {
            if (___target == null)
            {
                if (___drn == null || !___drn.friendly)
                {
                    ___target = PlayerTracker.Instance.GetTarget();
                }
            }
        }
    }
}
