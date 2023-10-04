using CarcassEnemy.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class CarcassIdle : CarcassState
    {
        public override void OnEnter(Carcass carcass) {}

        public override void OnExit(Carcass carcass)
        {

        }

        public override void OnFixedUpdate(Carcass carcass)
        {
            
        }

        public override void OnLateUpdate(Carcass carcass)
        {

        }

        public override void OnUpdate(Carcass carcass)
        {
            if (carcass.Target != null)
            {
                carcass.SetState(carcass.FloatAround);
            }
        }

        public override void OnCollisionEnter(Carcass carcass, Collision col)
        {

        }
    }
}
