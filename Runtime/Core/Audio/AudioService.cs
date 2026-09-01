using System;
using System.Collections.Generic;
using UnityEngine;
using VADE.DevTools.Persistence;

namespace VADE.DevTools.Audio
{

    public class AudioService : IAudioService
    {
        public AutoSave<bool> IsMuted { get; } = new("vade_audio_muted", AutoSaveType.PlayerPrefs, false);

        private IDisposable isMutedSubscription;

        private Transform poolRoot;
        private AudioLibrary audioLibrary;
        private AudioSource backgroundSource;

        private readonly Queue<PooledAudioSource> pool = new();
        private readonly Dictionary<string, PooledAudioSource> activeLoops = new();

        public void Init(AudioLibrary library, int poolSize = 10)
        {
            audioLibrary = library;
            if (audioLibrary == null)
                throw new Exception("AudioService.Init: AudioLibrary не передан (null).");

            audioLibrary.Initialize();

            var rootObj = new GameObject("[AudioService]");
            UnityEngine.Object.DontDestroyOnLoad(rootObj);
            poolRoot = rootObj.transform;

            if (audioLibrary.BackgroundAudioData?.clip != null)
            {
                backgroundSource = rootObj.AddComponent<AudioSource>();
                backgroundSource.clip = audioLibrary.BackgroundAudioData.clip;
                backgroundSource.volume = audioLibrary.BackgroundAudioData.volume;
                backgroundSource.playOnAwake = true;
                backgroundSource.loop = true;
            }

            for (int i = 0; i < poolSize; i++)
                pool.Enqueue(new PooledAudioSource(poolRoot));

            isMutedSubscription?.Dispose();
            isMutedSubscription = IsMuted.Subscribe(muted =>
            {
                if (backgroundSource == null) return;
                if (muted) backgroundSource.Pause();
                else backgroundSource.Play();
            });
        }

        private PooledAudioSource GetFromPool()
        {
            return pool.Count > 0 ? pool.Dequeue() : new PooledAudioSource(poolRoot);
        }

        private void ReleaseToPool(PooledAudioSource source, string soundName = null)
        {
            if (soundName != null)
                activeLoops.Remove(soundName);

            pool.Enqueue(source);
        }

        public void Play(string soundName, Vector3 position, AudioConfigOverride configOverride = null)
        {
            if (IsMuted.value) return;

            var sound = audioLibrary.GetSound(soundName);
            if (sound == null) return;

            var dataToPlay = new AudioData
            {
                name = sound.name,
                clips = sound.clips,
                volume = sound.volume,
                loop = sound.loop,
                loopDuration = sound.loopDuration
            };

            if (configOverride != null)
            {
                dataToPlay.volume = configOverride.volume ?? dataToPlay.volume;
                dataToPlay.loop = configOverride.loop ?? dataToPlay.loop;
                dataToPlay.loopDuration = configOverride.loopDuration ?? dataToPlay.loopDuration;
            }

            bool isLoop = dataToPlay.loop || dataToPlay.loopDuration > 0;

            if (isLoop && activeLoops.ContainsKey(soundName))
                return;

            var pooled = GetFromPool();
            pooled.Play(dataToPlay, position, source => ReleaseToPool(source, isLoop ? soundName : null));

            if (isLoop)
                activeLoops[soundName] = pooled;
        }

        public void Play(AudioClip clip, Vector3 position, AudioConfigOverride configOverride = null)
        {
            if (IsMuted.value || clip == null) return;

            var dataToPlay = new AudioData
            {
                name = clip.name,
                clips = new[] { clip },
                volume = configOverride?.volume ?? 1f,
                loop = configOverride?.loop ?? false,
                loopDuration = configOverride?.loopDuration ?? 0f
            };

            var pooled = GetFromPool();
            pooled.Play(dataToPlay, position, source => ReleaseToPool(source));
        }

        public void Dispose()
        {
            isMutedSubscription?.Dispose();
            isMutedSubscription = null;
        }
    }
}
