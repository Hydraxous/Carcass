using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components
{
    public class DeleteAfterTime : MonoBehaviour
    {
        [SerializeField] private float timeLeft;

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if(timeLeft <= 0f)
            {
                timeLeft = 0f;
                Destroy(gameObject);
            }
        }

    }
}
