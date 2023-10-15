using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarcassEnemy.Components.FX
{
    public class RandomizeAudioPitch : MonoBehaviour
    {
        [SerializeField] private Vector2 range = new Vector2(0.9f, 1.1f);
    
        private void OnEnable()
        {
            AudioSource src = GetComponent<AudioSource>();
            if (src == null)
                return;

            src.pitch = UnityEngine.Random.Range(range.x, range.y);
        }
    }
}
