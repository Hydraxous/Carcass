using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.AI
{
    public class AIStateMachine : MonoBehaviour
    {
        public AIState CurrentState => currentState;
        protected AIState currentState;

        protected AIState lastState;

        public void SetState(AIState state)
        {
            SetStateCore(state);
        }

        protected virtual void SetStateCore(AIState state)
        {
            lastState = currentState;
            currentState = state;
            lastState?.OnExit(this);
            currentState?.OnEnter(this);
        }
     
        private void Update()
        {
            UpdateCore();
        }

        protected virtual void UpdateCore()
        {
            currentState?.OnUpdate(this);
        }

        private void FixedUpdate()
        {
            FixedUpdateCore();
        }

        protected virtual void FixedUpdateCore()
        {
            currentState?.OnFixedUpdate(this);
        }

        private void LateUpdate()
        {
            LateUpdateCore();
        }

        protected virtual void LateUpdateCore()
        {
            currentState?.OnLateUpdate(this);
        }
    }
}
