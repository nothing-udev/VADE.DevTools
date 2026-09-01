using System;
using UnityEngine;
using VADE.DevTools.Persistence;

namespace VADE.DevTools.Audio
{
    public interface IAudioService : IDisposable
    {
        AutoSave<bool> IsMuted { get; }

        void Init(AudioLibrary library, int poolSize = 10);
        void Play(string soundName, Vector3 position, AudioConfigOverride configOverride = null);
        void Play(AudioClip clip, Vector3 position, AudioConfigOverride configOverride = null);
    }
}
