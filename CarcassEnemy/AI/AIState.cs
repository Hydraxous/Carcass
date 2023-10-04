using System;
using System.Collections.Generic;
using System.Text;

namespace CarcassEnemy.AI
{
    public abstract class AIState
    {
        public abstract void OnEnter(AIStateMachine stateMachine);
        public abstract void OnUpdate(AIStateMachine stateMachine);
        public abstract void OnFixedUpdate(AIStateMachine stateMachine);
        public abstract void OnLateUpdate(AIStateMachine stateMachine);
        public abstract void OnExit(AIStateMachine stateMachine);
    }
}
