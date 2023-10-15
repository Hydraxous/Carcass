using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class HashedTrigger : MonoBehaviour
    {
        private HashSet<Collider> checkedColliders = new HashSet<Collider>();

        public event Action<Collider> OnTriggerEntered;
        public event Action<Collider> OnTriggerStayed;
        public event Action<Collider> OnTriggerExited;

        private Collider col;
        private void Awake()
        {
            col = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            Clear();
        }

        public void Clear()
        {
            checkedColliders.Clear();
        }

        public bool Contains(Collider col)
        {
            return checkedColliders.Contains(col);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (Contains(col))
                return;

            checkedColliders.Add(col);
            OnTriggerEntered?.Invoke(col);
        }

        private void OnTriggerStay(Collider col)
        {
            OnTriggerStayed?.Invoke(col);
        }

        private void OnTriggerExit(Collider col)
        {
            OnTriggerExited?.Invoke(col);
        }

    }
}
