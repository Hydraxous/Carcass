using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components
{
    public class BehaviourRelay : MonoBehaviour
    {
        public event Action OnAwake;
        public event Action OnStart;
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;
        public event Action OnEnabled;
        public event Action OnDisabled;
        public event Action OnDestroyed;
        public event Action<Collider> OnTriggerEntered;
        public event Action<Collider> OnTriggerExited;
        public event Action<Collider> OnTriggerStayed;
        public event Action<Collision> OnCollisionEntered;

        private void Awake()
        {
            OnAwake?.Invoke();
        }

        private void Start()
        {
            OnStart?.Invoke();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void FixedUpdate() 
        {
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void OnEnable()
        {
            OnEnabled?.Invoke();
        }

        private void OnDisable()
        {
            OnDisabled?.Invoke();
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }

        private void OnTriggerEnter(Collider col)
        {
            OnTriggerEntered?.Invoke(col);
        }

        private void OnTriggerExit(Collider col)
        {
            OnTriggerExited?.Invoke(col);
        }

        private void OnTriggerStay(Collider col)
        {
            OnTriggerStayed?.Invoke(col);
        }

        private void OnCollisionEnter(Collision col)
        {
            OnCollisionEntered?.Invoke(col);
        }

    }
}
