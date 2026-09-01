using System;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Audio
{
    [Serializable]
    public class AudioData
    {
        public string name;
        public AudioClip[] clips;

        [Range(0f, 1f)]
        public float volume = 1f;

        public bool loop;

        [ShowIf(nameof(loop))]
        [Tooltip("0 = бесконечно")]
        public float loopDuration = 0f;
    }

    [Serializable]
    public class BackGroundAudioData
    {
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;
    }

    public class AudioConfigOverride
    {
        public float? volume;
        public bool? loop;
        public float? loopDuration;

        public AudioConfigOverride(float? volume = null, bool? loop = null, float? loopDuration = null)
        {
            this.volume = volume;
            this.loop = loop;
            this.loopDuration = loopDuration;
        }
    }
}
