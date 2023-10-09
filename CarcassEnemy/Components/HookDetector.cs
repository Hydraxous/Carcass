using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy
{
    public class HookDetector : MonoBehaviour
    {
        public event Action<bool> OnHookstateChanged;
        private EnemyIdentifier enemyIdentifier;

        public bool IsHooked => hooked;

        private bool hooked;
        private bool hookedLastFrame;


        private void Awake()
        {
            enemyIdentifier = GetComponent<EnemyIdentifier>();
        }

        private void Update()
        {
            hooked = enemyIdentifier.hooked;

            if(!hookedLastFrame && hooked)
                OnHookstateChanged?.Invoke(hooked);

            if(hookedLastFrame && !hooked)
                OnHookstateChanged?.Invoke(hooked);

            hookedLastFrame = hooked;
        }

        public void ForceUnhook(float animationTime = 0f, bool sparks = false)
        {
            if (!hooked)
                return;

            HookArm.Instance.StopThrow(animationTime, sparks);
        }
    }
}
