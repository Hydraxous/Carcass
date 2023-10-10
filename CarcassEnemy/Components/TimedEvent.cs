using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace CarcassEnemy.Components
{
    public class TimedEvent : MonoBehaviour
    {
        [SerializeField] private float lengthInSeconds;

        public UnityEvent OnInvoke;

        private bool active;
        private float timeLeft;

        private void Update()
        {
            if (!active)
                return;

            timeLeft -= Time.deltaTime;
            if (timeLeft > 0f)
                return;

            timeLeft = 0f;
            active = false;

            OnInvoke?.Invoke();
        }

        public void Execute()
        {
            timeLeft = lengthInSeconds;
            Resume();
        }

        public void SkipTimer()
        {
            timeLeft = 0f;
        }

        public void Pause()
        {
            active = false;
        }

        public void Resume()
        {
            active = true;
        }
    }
}
