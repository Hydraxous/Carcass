using CarcassEnemy.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public abstract class CarcassState : AIState
    {
        public abstract void OnEnter(Carcass carcass);
        public override void OnEnter(AIStateMachine stateMachine)
        {
            Carcass c = (Carcass)stateMachine;
            OnEnter(c);
        }


        public abstract void OnUpdate(Carcass carcass);
        public override void OnUpdate(AIStateMachine stateMachine)
        {
            Carcass c = (Carcass)stateMachine;
            OnUpdate(c);
        }



        public abstract void OnFixedUpdate(Carcass carcass);
        public override void OnFixedUpdate(AIStateMachine stateMachine)
        {
            Carcass c = (Carcass)stateMachine;
            OnFixedUpdate(c);
        }

        public abstract void OnLateUpdate(Carcass carcass);
        public override void OnLateUpdate(AIStateMachine stateMachine)
        {
            Carcass c = (Carcass)stateMachine;
            OnLateUpdate(c);
        }


        public abstract void OnExit(Carcass carcass);
        public override void OnExit(AIStateMachine stateMachine)
        {
            Carcass c = (Carcass) stateMachine;
            OnExit(c);
        }

        public abstract void OnCollisionEnter(Carcass carcass, Collision col);
    }
}
