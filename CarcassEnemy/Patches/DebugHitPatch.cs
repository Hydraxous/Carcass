using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    [HarmonyPatch(typeof(RevolverBeam))]
    public static class DebugHitPatch
    {

        [HarmonyPatch(nameof(RevolverBeam.ExecuteHits)), HarmonyPrefix]
        public static void LogExecuteHits(RevolverBeam __instance, RaycastHit currentHit)
        {
            if (!(currentHit.transform.gameObject.tag == "Enemy" || currentHit.transform.gameObject.tag == "Body" || currentHit.transform.gameObject.tag == "Limb" || currentHit.transform.gameObject.tag == "EndLimb" || currentHit.transform.gameObject.tag == "Head"))
                return;

            if (currentHit.transform == null)
                return;

            Debug.Log($"TF NAME: {currentHit.transform.name}");

            EnemyIdentifierIdentifier eidid = currentHit.transform.GetComponentInParent<EnemyIdentifierIdentifier>();

            if (eidid == null)
            {
                Debug.Log("EIDID IS NULL!");
            }
            else
            {
                Debug.Log("EIDID IS GOOD!");
            }

            EnemyIdentifier eid = currentHit.transform.GetComponentInParent<EnemyIdentifierIdentifier>().eid;

            if(eid == null)
            {
                Debug.Log("EID IS NULL!");
            }
            else 
            {
                Debug.Log("EID IS GOOD!");
            }
        }

    }
}
