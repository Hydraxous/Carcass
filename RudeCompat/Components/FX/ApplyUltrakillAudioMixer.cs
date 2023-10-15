using CarcassEnemy.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace CarcassEnemy.Components.FX
{
    public class ApplyUltrakillAudioMixer : MonoBehaviour
    {
        [SerializeField] private MixerGroup mixer = MixerGroup.All;

        private void OnEnable()
        {
            AudioSource src = GetComponent<AudioSource>();
            if (src == null)
                return;

            src.outputAudioMixerGroup = ResolveMixer(mixer);
        }

        private static AudioMixerGroup ResolveMixer(MixerGroup group)
        {
            switch (group)
            {
                default:
                    return null;

                case MixerGroup.All:
                    return UKPrefabs.AllMixer.Asset;

                case MixerGroup.Door:
                    return UKPrefabs.DoorMixer.Asset;

                case MixerGroup.Music:
                    return UKPrefabs.MusicMixer.Asset;

                case MixerGroup.Gore:
                    return UKPrefabs.GoreMixer.Asset;

                case MixerGroup.Unfreezable:
                    return UKPrefabs.UnfreezableMixer.Asset;
            }
        }
    }

    [Serializable]
    public enum MixerGroup
    {
        None,
        All,
        Door,
        Gore,
        Unfreezable,
        Music,
    }
}
