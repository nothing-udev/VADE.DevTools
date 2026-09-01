using System;
using System.Collections.Generic;
using UnityEngine;

namespace VADE.DevTools.Audio
{
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Configs/VADE/Audio/AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [SerializeField] private BackGroundAudioData backgroundAudioData;
        public BackGroundAudioData BackgroundAudioData => backgroundAudioData;

        [SerializeField] private List<AudioData> sounds = new();

        private Dictionary<string, AudioData> soundMap;

        public void Initialize()
        {
            soundMap = new Dictionary<string, AudioData>(StringComparer.OrdinalIgnoreCase);
            foreach (var sound in sounds)
            {
                if (!string.IsNullOrEmpty(sound.name) && !soundMap.ContainsKey(sound.name))
                    soundMap.Add(sound.name, sound);
            }
        }

        public AudioData GetSound(string soundName)
        {
            if (soundMap == null) Initialize();
            soundMap.TryGetValue(soundName, out var sound);
            return sound;
        }
    }
}
